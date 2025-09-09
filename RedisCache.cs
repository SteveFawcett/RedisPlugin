
using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedisPlugin.Classes;
using RedisPlugin.Forms;
using RedisPlugin.Properties;
using System.Text.Json;
using System.Timers;

namespace RedisPlugin;

public class RedisCache : BroadcastCacheBase, IDisposable
{
    private const string STANZA = "Redis";
    private System.Threading.Timer? Timer;
    private static readonly Image s_icon = Resources.red;
    private readonly ILogger<IPlugin>? _logger;

#pragma warning disable CS8618
    private static Connection _connection;
#pragma warning restore CS8618

    public static CachePage? _infoPage;
    private bool _disposed = false;

    public RedisCache() : base() { }

    public RedisCache(IConfiguration configuration, ILogger<IPlugin> logger) :
        base(configuration, LoadCachePage(logger, configuration), s_icon, STANZA)
    {
        _logger = logger;

        var port = configuration.GetSection(STANZA).GetValue<int>("port");
        var server = configuration.GetSection(STANZA).GetValue<string>("server") ?? string.Empty;

            _connection = new(logger, server, port);
            _connection.OnConnectionChange += Connection_OnConnectionChange;
        

        Timer = new System.Threading.Timer(
            callback => Timer_Elapsed(),
            null,
            dueTime: 0,         // 🔥 Fire immediately
            period: 10000
        );
    }

    private void Timer_Elapsed()
    {
        _logger?.LogDebug("Timer elapsed, checking Redis connection...");

            _connection.Connect();
      
    }

    private void Connection_OnConnectionChange(object? sender, bool isConnected)
    {
        _logger?.LogDebug($"Redis connection status changed: {(isConnected ? "Connected" : "Disconnected")}");
            _infoPage?.SetState(isConnected);
            if (isConnected)
            {
                if (_infoPage != null) _infoPage.URL = $"{_connection.Server}:{_connection.Port}";
            }
            else
            {
                if (_infoPage != null) _infoPage.URL = $"Connecting to: {_connection.Server}:{_connection.Port}";
            }
    }

    public static CachePage LoadCachePage(ILogger<IPlugin> logger, IConfiguration configuration)
    {
            _infoPage = new CachePage(logger, _connection);
            return _infoPage;

    }

    public override void Clear()
    {
        //TODO: Not Implemented yet
    }

    public override void CacheWriter(Dictionary<string, string> data) => InternalCacheWriter(data, RedisPrefixes.DATA);
    

    private void InternalCacheWriter(Dictionary<string, string> data, RedisPrefixes prefix = RedisPrefixes.DATA)
    {
        try
        {
                if (!_connection.isConnected) _connection.Connect();

                if ( _connection.isConnected)
                {
                    foreach (var kvp in data)
                    {
                        if( prefix == RedisPrefixes.COMMAND )
                            _logger?.LogDebug("Writing Command: {Key} with data length: {Length}", kvp.Key, kvp.Value.Length);
                        else
                            _logger?.LogDebug("Writing Data: {Key} with data length: {Length}", kvp.Key, kvp.Value.Length);

                        _connection?.Write(kvp.Key, kvp.Value , prefix);
                    }
                    _infoPage?.Redraw(data);
                }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in CacheWriter method: {Message}", ex.Message);
        }
    }

    public override List<KeyValuePair<string, string>> CacheReader(List<string> values) => InternalCacheReader(values , RedisPrefixes.DATA);
    public override List<CommandItem> CommandReader( BroadcastPluginSDK.Classes.CommandStatus status)
    {
        _logger?.LogDebug("Starting CommandReader with status: {Status}", status);
        List<CommandItem> commands = new();
        var requests = Read(RedisPrefixes.COMMAND);

        _logger?.LogDebug("Reading commands from Redis, total found: {Count}", requests.Count());

        foreach (var kvp in requests)
        {
            _logger?.LogDebug("Deserializing command: {Key}", kvp.Key);

            if( string.IsNullOrWhiteSpace(kvp.Value) )
            {
                _logger?.LogWarning("Empty or whitespace value for key {Key}, deleting from Redis.", kvp.Key);
                _connection.Delete(kvp.Key , RedisPrefixes.COMMAND );
                continue;
            }

            try
            {
                CommandItem? item = JsonSerializer.Deserialize<CommandItem>(kvp.Value);
                _logger?.LogDebug("Deserialized command: {Item}", item != null ? item.Id : "null");
                if (item != null && item.Status == status)
                {
                    _logger?.LogDebug("Adding command to list: {Item}", item.Id);
                    commands.Add(item);
                }
            }
            catch (JsonException jsonEx)
            {
                _logger?.LogError(jsonEx, "JSON deserialization error for key {Key}", kvp.Key );
                _connection.Delete(kvp.Key , RedisPrefixes.COMMAND );
                continue;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error deserializing key {Key}:", kvp.Key);
                continue;
            }
        }

        return commands;
    }
    public override void CommandWriter(CommandItem data)
    {
        _logger?.LogDebug("Starting CommandWriter for command: {Id}", data.Id);

        var json = JsonSerializer.Serialize(data);

        _logger?.LogDebug("Serializing command: {Id}", data.Id);
        _logger?.LogDebug("Serialized JSON: {Json}", json);
        InternalCacheWriter(new Dictionary<string, string> { { data.Id, json } }, RedisPrefixes.COMMAND);
    }
    private List<KeyValuePair<string, string>> InternalCacheReader(List<string> values, RedisPrefixes prefix = RedisPrefixes.DATA)
    {
            if (values.Count == 0) return Read(prefix).ToList();
            return Read(values, prefix).ToList();
    }

    public IEnumerable<KeyValuePair<string, string>> Read(List<string> values , RedisPrefixes prefix = RedisPrefixes.DATA)
    {
            foreach (var value in values) yield return ReadValue(value , prefix);
    }

    public IEnumerable<KeyValuePair<string, string>> Read( RedisPrefixes prefix = RedisPrefixes.DATA)
    {
            if (!_connection.isConnected) _connection.Connect();

            if (_connection.isConnected)
            {
                _logger?.LogDebug("Read: Connected to Redis database.");
            }

            foreach( string key in _connection.Keys( prefix ) )
                yield return ReadValue( key , prefix);
    }

    public KeyValuePair<string, string> ReadValue(string value, RedisPrefixes prefix = RedisPrefixes.DATA)
    {
            if (_connection.isConnected)
            {
                _logger?.LogDebug("ReadValue: Reading {key}" , value );
                var data = new KeyValuePair<string, string>(value, _connection.Read(value , prefix) ?? string.Empty);
                _infoPage?.Redraw(data);
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

                if (Timer != null)
                {
                    Timer.Dispose();
                }
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
}