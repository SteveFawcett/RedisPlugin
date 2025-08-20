using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RedisPlugin;

internal class Connection : IDisposable
{
    private const int REDIS_TIMEOUT = 5000; // 5 seconds

    private static ConnectionMultiplexer? _redis;
    private ConnectionMultiplexer? _redis;
    private IDatabase? db;
    private ILogger _logger;
    private string _server;
    private int _port;

    public bool Connected => _redis?.IsConnected ?? false;

    private string? _lastmessage = String.Empty;
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
            if (_redis.IsConnected) LogOnce($"Successfully connected to Redis server at {_server}:{_port}");
  
        }
        catch (RedisConnectionException)
        {
            LogOnce($"Failed to connect to Redis server at {_server}:{_port}.");
            _redis = null;
        }
        catch (Exception ex)
        {
            LogOnce($"An unexpected error occurred while connecting to Redis: {ex.Message}");
            _redis = null;

        }

        if (_redis is { IsConnected: true } && db == null)
            LogOnce("Redis connection established: true");

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
        if( Connected == false ) Connect();

        if (db != null) db.StringSet(key, value);
    }

    public string? Read(string key)
    {
        if (Connected == false) Connect();

        if (db != null) return db.StringGet(key);
        return null;
    }
}