using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RedisPlugin.Classes;

public class Connection : IDisposable
{
    private const int REDIS_TIMEOUT = 5000; // 5 seconds

    private ConnectionMultiplexer? _redis;
    private IDatabase? db;
    private ILogger _logger;
    private string _server;
    private int _port;
    public bool isConnected => _redis?.IsConnected ?? false;

    public EventHandler<bool>? OnConnectionChange;

    private string? _lastmessage = string.Empty;
    private void LogOnce(string message)
    {
        if (_lastmessage == message) return;
        _logger.LogError(message);
        _lastmessage = message;
    }

    public Connection(ILogger logger, string server, int port)
    {
        _logger = logger;
        _server = server;
        _port = port;
        Connect();
    }

    private void Connect()
    {
        try
        {
            _redis = ConnectionMultiplexer.Connect($"{_server}:{_port},ConnectTimeout={REDIS_TIMEOUT}");
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
            }

        }
        catch (RedisConnectionException)
        {
            LogOnce($"Failed to connect to Redis server at {_server}:{_port}.");
            OnConnectionChange?.Invoke(this, false);
            _redis = null;
        }
        catch (Exception ex)
        {
            LogOnce($"An unexpected error occurred while connecting to Redis: {ex.Message}");
            OnConnectionChange?.Invoke(this, false);
            _redis = null;

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