 
using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedisPlugin.Classes;
using RedisPlugin.Forms;
using RedisPlugin.Properties;
using System.Diagnostics;
using System.Timers;

namespace RedisPlugin;

public class RedisCache : BroadcastCacheBase, IDisposable
{
    private const string STANZA = "Redis";
    private System.Threading.Timer? Timer;
    private static readonly Image s_icon = Resources.red;
    private readonly ILogger<IPlugin>? _logger;
    private static readonly object _syncRoot = new();

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

        lock (_syncRoot)
        {
            _connection = new(logger, server, port);
            _connection.OnConnectionChange += Connection_OnConnectionChange;
        }

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
        lock (_syncRoot)
        {
            _connection.Connect();
        }
    }

    private void Connection_OnConnectionChange(object? sender, bool isConnected)
    {
        _logger?.LogInformation($"Redis connection status changed: {(isConnected ? "Connected" : "Disconnected")}");
        lock (_syncRoot)
        {
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
    }

    public static CachePage LoadCachePage(ILogger<IPlugin> logger, IConfiguration configuration)
    {
        lock (_syncRoot)
        {
            _infoPage = new CachePage(logger, _connection);
            return _infoPage;
        }
    }

    public override void Clear()
    {
        //TODO: Not Implemented yet
    }

    public override void Write(Dictionary<string, string> data)
    {
        try
        {
            lock (_syncRoot)
            {
                if (!_connection.isConnected) _connection.Connect();

                if ( _connection.isConnected)
                {
                    foreach (var kvp in data)
                    {
                        _connection?.Write(kvp.Key, kvp.Value);
                    }
                    _infoPage?.Redraw(data);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in Write method: {Message}", ex.Message);
       
            return;
        }
    }

    public override List<KeyValuePair<string, string>> CacheReader(List<string> values)
    {
        lock (_syncRoot)
        {
            if (values.Count == 0) return Read().ToList();
            return Read(values).ToList();
        }
    }

    public IEnumerable<KeyValuePair<string, string>> Read(List<string> values)
    {
        lock (_syncRoot)
        {
            foreach (var value in values) yield return ReadValue(value);
        }
    }

    public IEnumerable<KeyValuePair<string, string>> Read()
    {
        lock (_syncRoot)
        {
            if (!_connection.isConnected) _connection.Connect();

            if (_connection.isConnected)
            {
                _logger?.LogDebug("Read: Connected to Redis database.");
            }
            // TODO: Implement key enumeration
            yield break;
        }
    }

    public KeyValuePair<string, string> ReadValue(string value)
    {
        lock (_syncRoot)
        {
            if (_connection.isConnected)
            {
                var data = new KeyValuePair<string, string>(value, _connection.Read(value) ?? string.Empty);
                _infoPage?.Redraw(data);
                return data;
            }
            return new KeyValuePair<string, string>(value, string.Empty);
        }
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
            lock (_syncRoot)
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
        }
        _disposed = true;
    }
}