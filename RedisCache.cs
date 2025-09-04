using BroadcastPluginSDK;
using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedisPlugin.Properties;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Net.Security;

namespace RedisPlugin;

public class RedisCache : BroadcastCacheBase
{
    private const string STANZA = "Redis";

    private static readonly Image s_icon = Resources.red;
    private readonly ILogger<IPlugin>? _logger;
    private static Connection? _connection;
    private static CachePage? _infoPage;

    public RedisCache() : base() {}

    public RedisCache(IConfiguration configuration, ILogger<IPlugin> logger) :
        base(configuration, LoadCachePage(configuration , logger , _connection ), s_icon, STANZA
            )
    {
        _logger = logger;

        int port = configuration.GetSection(STANZA).GetValue<int>("port", 6379);
        string server = configuration.GetSection(STANZA).GetValue<string>("server", "localhost");

        _connection = new Connection(_logger , server, port);
    }

    public static CachePage LoadCachePage( IConfiguration config, ILogger<IPlugin> logger , Connection? _connection )
    {
        int port = config.GetSection(STANZA).GetValue<int>("port", 6379);
        string server = config.GetSection(STANZA).GetValue<string>("server", "localhost");

        _infoPage =  new CachePage(logger, server, port , _connection );

        return _infoPage;
    }
    public override void Clear()
    {
        if (_connection is not null && _connection.Connected )
        {
            _logger?.LogInformation("Connected to Redis database.");
        }
    }

    public override void Write(Dictionary<string, string> data)
    {
        if (_connection is not null &&  _connection.Connected)
        {
            foreach (var kvp in data)
            {
                _connection.Write(kvp.Key, kvp.Value);
            }
            _logger?.LogInformation("Connected to Redis database.");
        }
        _infoPage?.Redraw( data );
    }

    public override List<KeyValuePair<string, string>> CacheReader(List<string> values)
    {
        if (values.Count == 0) return Read().ToList();

        return Read(values).ToList();
    }

    public IEnumerable<KeyValuePair<string, string>> Read(List<string> values)
    {
        foreach (var value in values) yield return ReadValue(value);
    }

    public IEnumerable<KeyValuePair<string, string>> Read()
    {
        if (_connection is not null && _connection.Connected)
        {
            _logger?.LogInformation("Connected to Redis database.");
        }
        // TODO: Assuming _connection.GetAllKeys() returns IEnumerable<string> of all keys in Redis
        // foreach (var key in _c)
        // {
        //     var value = _connection.Read(key) ?? string.Empty;
        //     var data = new KeyValuePair<string, string>(key, value);
        //     _infoPage.Redraw(data);
        //     yield return data;
        // }
        yield break;
     }
        // If not connected, yield nothing (empty sequence)
   

    public KeyValuePair<string, string> ReadValue(string value)
    {
        if (_connection is not null &&  _connection.Connected)
        {
            var data = new KeyValuePair<string, string>(value, _connection.Read(value) ?? string.Empty);
            _infoPage?.Redraw( data );
            return data;
        }
        return new KeyValuePair<string, string>(value, string.Empty);
    }
}