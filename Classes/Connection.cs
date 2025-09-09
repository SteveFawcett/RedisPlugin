using System.Diagnostics;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RedisPlugin.Classes;

public class Connection : IDisposable
{
    private const int PORT = 6379;
    private const string SERVER = "localhost";
    private const int REDIS_TIMEOUT = 10000; // 5 seconds

    private ConnectionMultiplexer? _redis;
    private IDatabase? db;
    private readonly ILogger<IPlugin>? _logger;
    private readonly string _server;
    private readonly int _port;
    private bool _disposed;

    public bool isConnected
    {
        get
        {
                if (_redis?.IsConnected == false)
                {
                    LogOnce("Redis connection lost.", isError: true);
                    db = null;
                    OnConnectionChange?.Invoke(this, false);
                    return false;
                }
                return true;
        }
    }

    public EventHandler<bool>? OnConnectionChange;

    public string Server => _server;
    public int Port => _port;

    private string? _lastmessage = string.Empty;
    private void LogOnce(string message, bool isError = true)
    {
        if (_lastmessage == message) return;
        if (isError)
            _logger?.LogError(message);
        else
            _logger?.LogInformation(message);
        _lastmessage = message;
    }

    public Connection(ILogger<IPlugin>? logger, string server = SERVER, int port = PORT)
    {
        _logger = logger;
        _server = server ?? SERVER;
        _port = port;
    }

    [DebuggerNonUserCode]
    public void Connect()
    {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Connection));

            if (_redis != null && _redis.IsConnected)
            {
                // Already connected
                return;
            }
        
            var config = new ConfigurationOptions
            {
                EndPoints = { $"{_server}:{_port}" },
                ConnectTimeout = REDIS_TIMEOUT,
                AbortOnConnectFail = false,
                AllowAdmin = true,
                SyncTimeout = REDIS_TIMEOUT,
                KeepAlive = 180
            };

            try
            {
                _logger?.LogDebug($"Attempting to connect to {config.EndPoints}");

                _redis = ConnectionMultiplexer.Connect(config);

                if ( _redis != null)
                {
                    db = _redis.GetDatabase();
                    LogOnce($"Successfully connected to Redis database {_redis.ClientName}", isError: false);
                    OnConnectionChange?.Invoke(this, true);
                }
                else
                {
                    LogOnce($"Failed to connect to Redis server at {_server}:{_port}.");
                    db = null;
                    OnConnectionChange?.Invoke(this, false);
                    _redis = null;
                }
            }
            catch (RedisConnectionException)
            {
                LogOnce($"Failed to connect to Redis server at {_server}:{_port}.");
                db = null;
                OnConnectionChange?.Invoke(this, false);
                _redis = null;
            }
            catch (Exception ex)
            {
                LogOnce($"An unexpected error occurred while connecting to Redis: {ex.Message}");
                db = null;
                OnConnectionChange?.Invoke(this, false);
                _redis = null;
            }
    }

    public List<string> Keys(RedisPrefixes prefix = RedisPrefixes.DATA)
    {
        var endpoint = _redis?.GetEndPoints().FirstOrDefault();
        var server = endpoint != null ? _redis?.GetServer(endpoint) : null;

        string prefixString = $"{prefix}:";

        return server?.Keys(pattern: $"{prefixString}*")
            .Select(k => k.ToString().StartsWith(prefixString)
                ? k.ToString().Substring(prefixString.Length)
                : k.ToString())
            .ToList() ?? [];
    }

    public void Delete(string key, RedisPrefixes prefix )
    {
            try
            {
                var deleted = db?.KeyDelete( $"{prefix.ToString()}:{key}") ?? false;
                _logger?.LogDebug($"Deleted key {key} from Redis: {deleted}");
             }
            catch (Exception ex)
            {
                LogOnce($"Error deleting from Redis: {ex.Message}");
            }
    }
    public void Write(string key, string value , RedisPrefixes prefix = RedisPrefixes.DATA )
    {
            try
            {
                _logger?.LogDebug($"Attempting to write : {prefix.ToString()}:{key} = {value}");
                db?.StringSet( $"{prefix.ToString()}:{key}", value);
            }
            catch (Exception ex)
            {
                LogOnce($"Error writing to Redis: {ex.Message}");
            }
    }

    public string? Read(string key, RedisPrefixes prefix = RedisPrefixes.DATA )
    {
            try
            {
                if (db != null)
                {
                    _logger?.LogDebug($"Attempting to get : {prefix.ToString()}:{key}");
                    var value = db.StringGet($"{prefix.ToString()}:{key}");
                    return value.HasValue ? (string?)value : null;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogOnce($"Error reading from Redis: {ex.Message}");
                return null;
            }
    }


    public void Dispose()
    {
            if (_disposed) return;
            try
            {
                LogOnce("Closing Redis connection...", isError: false);
                _redis?.Close();
                _redis?.Dispose();
            }
            catch (Exception ex)
            {
                LogOnce($"Error while closing Redis connection: {ex.Message}");
            }
            _redis = null;
            db = null;
            _disposed = true;
    }
}