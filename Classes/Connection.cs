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
    private readonly object _syncRoot = new();
    private bool _disposed;

    public bool isConnected
    {
        get
        {
            lock (_syncRoot)
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
        lock (_syncRoot)
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
    }

    public void Write(string key, string value)
    {
        lock (_syncRoot)
        {
            if (!isConnected) Connect();
            try
            {
                db?.StringSet(key, value);
            }
            catch (Exception ex)
            {
                LogOnce($"Error writing to Redis: {ex.Message}");
            }
        }
    }

    public string? Read(string key)
    {
        lock (_syncRoot)
        {
            if (!isConnected) Connect();

            try
            {
                if (db != null)
                {
                    var value = db.StringGet(key);
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
    }

    public void Dispose()
    {
        lock (_syncRoot)
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
}