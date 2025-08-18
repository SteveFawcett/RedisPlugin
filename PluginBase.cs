using BroadcastPluginSDK.abstracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedisPlugin.Properties;
using System.ComponentModel.DataAnnotations;
using System.Net.Security;
using BroadcastPluginSDK.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RedisPlugin;

public class PluginBase : BroadcastCacheBase
{
    private static readonly CachePage s_infoPage = new();
    private static readonly Image s_icon = Resources.red;
    private static readonly string Stanza = "Redis";
    private readonly ILogger<IPlugin> _logger;
    private int Port = 6379; // Default Redis port
    private string Server = "localhost"; // Default Redis server
    private Connection connection;

    public PluginBase(IConfiguration configuration, ILogger<IPlugin> logger) : base(configuration, s_infoPage, s_icon, "Redis Cache", Stanza,
        "REDIS Cache")
    {
        _logger = logger;
        Port = configuration.GetSection(Stanza).GetValue<int>("Port", 6379);
        Server = configuration.GetSection(Stanza).GetValue<string>("Server", "localhost");
        
        s_infoPage.URL = $"redis://{Server}:{Port}";
        _logger.LogInformation( s_infoPage.URL );
        connection = new Connection(_logger , Server, Port);
    }

    public override void Clear()
    {
        if (connection.Connected )
        {
            _logger.LogError("Connected to Redis database.");
        }
    }

    public override void Write(Dictionary<string, string> data)
    {
        if (connection.Connected)
        {
            foreach (var kvp in data)
            {
                connection.Write(kvp.Key, kvp.Value);
            }
            _logger.LogError("Connected to Redis database.");
        }
        s_infoPage.Redraw( data );
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
        if (connection.Connected)
        {
            _logger.LogError("Connected to Redis database.");
        }
        yield return new KeyValuePair<string, string>( "a", "b");
    }

    public KeyValuePair<string, string> ReadValue(string value)
    {
        if (connection.Connected)
        {
            var data = new KeyValuePair<string, string>(value, connection.Read(value) ?? string.Empty);
            s_infoPage.Redraw( data );
            return data;
        }
        return new KeyValuePair<string, string>(value, string.Empty);
    }
}