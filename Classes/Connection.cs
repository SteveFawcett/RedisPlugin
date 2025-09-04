using System.Diagnostics;
using System.Security.Cryptography.Xml;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RedisPlugin.Classes;

public class Connection : IDisposable
{
    private const int PORT = 6379;
    private const string SERVER = "localhost";

    private const int REDIS_TIMEOUT = 5000; // 5 seconds

    private ConnectionMultiplexer? _redis;
    private IDatabase? db;
    private ILogger<IPlugin>? _logger;
    private string _server;
    private int _port;
    public bool isConnected => _redis?.IsConnected ?? false;

    public EventHandler<bool>? OnConnectionChange;

    public string Server => _server;
    public int Port => _port;

    private string? _lastmessage = string.Empty;
    private void LogOnce(string message)
    {
        if (_lastmessage == message) return;
        _logger?.LogError(message);
        _lastmessage = message;
    }

    public Connection(ILogger<IPlugin>? logger, string server = SERVER, int port = PORT)
    {
        _logger = logger;
        _server = server ?? SERVER ;
        _port = port ;
        if (_logger == null) return;
        Connect();
    }

    public void Connect()
    {
        var config = new ConfigurationOptions
        {
            EndPoints = { $"{_server}:{_port}" },
            ConnectTimeout = REDIS_TIMEOUT,
            AbortOnConnectFail = false, // prevents hard exceptions on startup
            AllowAdmin = true
        };

        try
        {
            _redis = ConnectionMultiplexer.Connect(config);

            if (_redis.IsConnected)
            {
                LogOnce($"Successfully connected to Redis server at {_server}:{_port}");
                OnConnectionChange?.Invoke(this, true);
            }
            else
            {
                LogOnce($"Failed to connect to Redis server at {_server}:{_port}.");
                OnConnectionChange?.Invoke(this, false);
                _redis = null;
                return;
            }

        }
        catch (RedisConnectionException)
        {
            LogOnce($"Failed to connect to Redis server at {_server}:{_port}.");
            OnConnectionChange?.Invoke(this, false);
            _redis = null;
            return;
        }
        catch (Exception ex)
        {
            LogOnce($"An unexpected error occurred while connecting to Redis: {ex.Message}");
            OnConnectionChange?.Invoke(this, false);
            _redis = null;
            return;
        }

        db ??= _redis?.GetDatabase();
    }

    public void Dispose()
    {
        if (_redis != null)
        {
            try
            {
                 LogOnce("Closing Redis connection...");
                _redis.Close();
                _redis.Dispose();
            }
            catch (Exception ex)
            {
                LogOnce($"Error while closing Redis connection: {ex.Message}");
            }

            _redis = null;
        }

        db = null;
    }

    public void Write(string key, string value)
    {
        if( isConnected == false ) Connect();

        if (db != null) db.StringSet(key, value);
    }

    public string? Read(string key)
    {
        if (isConnected == false) Connect();

        if (db != null) return db.StringGet(key);
        return null;
    }
}