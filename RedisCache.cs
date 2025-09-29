
using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedisPlugin.Classes;
using RedisPlugin.Forms;
using RedisPlugin.Properties;
using System.Text.Json;

namespace RedisPlugin;

public class RedisCache : BroadcastCacheBase, IProvider,  IDisposable
{
    private const string STANZA = "Redis";
    private static Image s_icon = Resources.red;
    private readonly ILogger<RedisCache>? _logger;

#pragma warning disable CS8618
    private static Connection _connection;
#pragma warning restore CS8618

    private static CachePage? _infoPage;
    private bool _disposed = false;

    public event EventHandler<CacheData>? DataReceived;
    public event EventHandler<CommandItem>? CommandReceived;

    public RedisCache() : base() { }

    public RedisCache(IConfiguration configuration, ILogger<RedisCache> logger) :
        base(configuration, LoadCachePage(logger, configuration , _connection ), s_icon, STANZA)
    {
        _logger = logger;
        _logger.LogInformation("REDIS Plugin Initializing ... ");

        var config = configuration.GetSection(STANZA);

        var port = config.GetValue<int>("port");
        var server = config.GetValue<string>("server") ?? string.Empty;
        double ReconnectRate = config.GetValue<int?>("ReconnectRate") ?? 1000;
        double JobScanRate = config.GetValue<int?>("JobScanRate") ?? 1000;

        _connection = new( logger, config);
        _connection.OnConnectionChange += Connection_OnConnectionChange;

        PeriodicTimer timer1 = new PeriodicTimer(TimeSpan.FromMilliseconds(ReconnectRate));
        _ = Task.Run(async () =>
        {
            while (await timer1.WaitForNextTickAsync())
            {
                Reconnector();
            }
        });

        PeriodicTimer timer2 = new PeriodicTimer(TimeSpan.FromMilliseconds(JobScanRate));
        _ = Task.Run(async () =>
        {
            while (await timer2.WaitForNextTickAsync())
            {
                JobScanner();
            }
        });
    }

    private void Reconnector()
    {
        _logger?.LogDebug("Timer elapsed, checking Redis connection {state}", _connection.isConnected );

        if ( _connection.isConnected == false ) _connection.Connect();

        SetState( _connection.isConnected );
    }

    private void JobScanner()
    {      
        if (_connection.isConnected == true)
        {
            _logger?.LogDebug("Timer elapsed, checking for New Command Jobs");

            foreach (var job in CommandReader( CommandStatus.New ) )
            {
                job.Status = CommandStatus.Queued;
                CommandReceived?.Invoke(this, job);
                CommandWriter( job );
            }
        }
    }
    private void Connection_OnConnectionChange(object? sender, bool isConnected)
    {
        _logger?.LogDebug($"Redis connection status changed: {(isConnected ? "Connected" : "Disconnected")}");
        SetState(isConnected);
    }

    private void SetState( bool isConnected = false)
    {
        _infoPage?.UpdateInfoPage(_connection);

        s_icon = isConnected ? Resources.green : Resources.red;
        
        ImageChangedInvoke( s_icon);

    }

    private static CachePage LoadCachePage(ILogger<RedisCache> logger, IConfiguration configuration, Connection conn)
    {
            _infoPage = new CachePage(logger, conn);
            return _infoPage;

    }

    public override void Clear()
    {
        //TODO: Not Implemented yet
    }

    public override void CacheWriter(CacheData payload)
    {
        foreach (var kvp in payload.Data)
            InternalCacheWriter(kvp, payload.Prefix );
    }

    private void InternalCacheWriter(KeyValuePair<string, string> kvp, CachePrefixes prefix = CachePrefixes.DATA)
    {
        try
        {
            if ( _connection.isConnected)
            {
                if( prefix == CachePrefixes.COMMAND )
                    _logger?.LogDebug("Writing Value: {Key} with data length: {Length}", kvp.Key, kvp.Value.Length);
                else
                    _logger?.LogDebug("Writing Data: {Key} with data length: {Length}", kvp.Key, kvp.Value.Length);

                _connection?.Write(kvp.Key, kvp.Value , prefix);   
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in CacheWriter method: {Message}", ex.Message);
        }
    }

    public override void CommandWriter(CommandItem data)
    {
        _logger?.LogDebug("Starting CommandWriter for command: {Key}", data.Key);

        var json = JsonSerializer.Serialize(data);

        _logger?.LogDebug("Serializing command: {Key}", data.Key);
        _logger?.LogDebug("Serialized JSON: {Json}", json);
        InternalCacheWriter(new KeyValuePair<string, string>( data.Key, json ), CachePrefixes.COMMAND);
        _logger?.LogInformation("CommandWriter completed for command: {Key} {status}", data.Key , data.Status.ToString() );
    }
    private List<KeyValuePair<string, string>> InternalCacheReader(List<string> values, CachePrefixes prefix = CachePrefixes.DATA)
    {
            if (values.Count == 0) return Read(prefix).ToList();
            return Read(values, prefix).ToList();
    }

    public IEnumerable<KeyValuePair<string, string>> Read(List<string> values , CachePrefixes prefix = CachePrefixes.DATA)
    {
            foreach (var value in values) yield return ReadValue(value , prefix);
    }

    public IEnumerable<KeyValuePair<string, string>> Read(CachePrefixes prefix = CachePrefixes.DATA)
    {
            if (_connection.isConnected)
            {
                _logger?.LogDebug("Read: Connected to Redis database.");
            }

            foreach( string key in _connection.Keys( prefix ) )
                yield return ReadValue( key , prefix);
    }

    public KeyValuePair<string, string> ReadValue(string value, CachePrefixes prefix = CachePrefixes.DATA)
    {
            if (_connection.isConnected)
            {
                _logger?.LogDebug("ReadValue: Reading {key}" , value );
                var data = new KeyValuePair<string, string>(value, _connection.Read(value , prefix) ?? string.Empty);
                return data;
            }
            return new KeyValuePair<string, string>(value, string.Empty);
    }

    // IDisposable implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {

                if (_connection != null)
                {
                    _connection.Dispose();
                }
                if (_infoPage is IDisposable disposablePage)
                {
                    disposablePage.Dispose();
                }
        }
        _disposed = true;
    }

    private IEnumerable<CommandItem> CommandReader(CommandStatus status)
    {
        _logger?.LogDebug("Starting CommandReader for status: {Status}", status);

        foreach (var kvp in Read(CachePrefixes.COMMAND))
        {
            CommandItem item;

            _logger?.LogDebug("Deserializing command: {Key}", kvp.Key);

            if (string.IsNullOrWhiteSpace(kvp.Value))
            {
                _logger?.LogWarning("Empty or whitespace value for key {Key}, deleting from Redis.", kvp.Key);
                _connection.Delete(kvp.Key, CachePrefixes.COMMAND);
                continue;
            }

            try
            {
                item = JsonSerializer.Deserialize<CommandItem>(kvp.Value) ?? throw new JsonException("Deserialization returned null");

                _logger?.LogDebug("Deserialized command: {Item}", item != null ? item.Key : "null");
            }
            catch (JsonException jsonEx)
            {
                _logger?.LogError(jsonEx, "JSON deserialization error for key {Key}", kvp.Key);
                _connection.Delete(kvp.Key, CachePrefixes.COMMAND);
                continue;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error deserializing key {Key}:", kvp.Key);
                continue;
            }

            if (item != null && item.Status == status)
            {
                _logger?.LogInformation("Adding command to list: {Item} {command}", item.Key, item.Value.ToString());

                yield return item;
            }
        }

        yield break;
    }

}