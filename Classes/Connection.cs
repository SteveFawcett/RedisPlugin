using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedisPlugin.Forms;
using StackExchange.Redis;
using System.Diagnostics;
using System.Net;

namespace RedisPlugin.Classes;

public class Connection : IDisposable
{
    private const int PORT = 6379;
    private const string SERVER = "localhost";
    private const int REDIS_TIMEOUT = 5000; // 5 seconds

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
            _logger?.LogDebug("Checking connection is {state}" , _redis?.IsConnected ?? false );
            if( _redis == null )
            {
                db = null;
                OnConnectionChange?.Invoke(this, false);
                return false;
            }
            //_logger?.LogInformation("Multiplexer is Status: {status} , Counters {counters}.", _redis?.GetStatus(), _redis?.GetCounters());
            if ( ! _redis.IsConnected )
            {
                _logger?.LogWarning("Triggering Lost Connection {state}", _redis.IsConnected );
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

    public Connection(ILogger<IPlugin>? logger, IConfiguration configuration)
    {
        _logger = logger;
        _server = configuration.GetValue<string>("Server") ?? SERVER;
        _port = configuration.GetValue<int>("Port");
        var _flush = configuration.GetValue<bool>("FlushOnStartUp");

        _logger?.LogInformation($"Instance created with {_server}:{_port}");
        Connect();
        if( _flush )flush();
    }

    [DebuggerNonUserCode]
    public void Connect()
    {
        if (_disposed)
                throw new ObjectDisposedException(nameof(Connection));

        if ( isConnected )
            {
                // Already connected
                _logger?.LogWarning("Already connected to Redis, skipping Connect()");
                 return;
            }
        
            var config = new ConfigurationOptions
            { 
                ConnectTimeout = REDIS_TIMEOUT,
                AbortOnConnectFail = false,
                AllowAdmin = true,
                SyncTimeout = REDIS_TIMEOUT,
                KeepAlive = 180
            };

            config.EndPoints.Add(_server, _port);

            try
            {
                _logger?.LogInformation($"Attempting to connect to {config.EndPoints.FirstOrDefault()}");

                do {
                    _redis = ConnectionMultiplexer.Connect(config);
                    _logger?.LogInformation($"Redis connection state: IsConnected={_redis.IsConnected}, IsConnecting={_redis.IsConnecting}" );
                }
                while ( _redis.IsConnecting == true ) ;

                if ( _redis.IsConnected == true )
                {
                    db = _redis.GetDatabase();
                    _logger?.LogInformation($"Successfully connected to Redis database {_redis.ClientName}");
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

    public void flush()
    {
        if (!isConnected)
        {
            _logger?.LogWarning("Cannot flush Redis: Not connected.");
            return;
        }
        try
        {
            var endpoint = _redis?.GetEndPoints().FirstOrDefault();
            if (endpoint == null)
            {
                _logger?.LogWarning("Cannot flush Redis: No Redis endpoints available.");
                return;
            }
            var server = endpoint != null ? _redis?.GetServer(endpoint) : null;
            if (server == null)
            {
                _logger?.LogWarning("Cannot flush Redis: No Redis server available.");
                return;
            }
            server.FlushDatabase();
            _logger?.LogInformation("Flushed all keys from Redis database.");
        }
        catch (Exception ex)
        {
            LogOnce($"Error flushing Redis: {ex.Message}");
        }
    }
    public IEnumerable<string> Keys(RedisPrefixes prefix = RedisPrefixes.DATA)
    {
        if ( !isConnected)
        {
            _logger?.LogWarning("Cannot retrieve keys: Not connected to Redis.");
            yield break;
        }

        foreach (var ep in _redis?.GetEndPoints() ?? [] )
        {
            var s = _redis?.GetServer(ep);

            _logger?.LogDebug($"Server at {ep} - IsConnected: {s?.IsConnected}" );
        }

        var endpoint = _redis?.GetEndPoints().FirstOrDefault();

        if ( endpoint == null )
        {
            _logger?.LogWarning("Cannot retrieve keys: No Redis endpoints available.");
            yield break;
        }

        var server = endpoint != null ? _redis?.GetServer(endpoint) : null;

        if( server == null)
        {
            _logger?.LogWarning("Cannot retrieve keys: No Redis server available.");
            yield break;
        }

        string prefixString = $"{prefix}:";

        foreach (var key in server.Keys(pattern: $"{prefixString}*"))
        {
            _logger?.LogDebug($"Found key: {key}");
            yield return key.ToString().Replace(prefixString, string.Empty);
        }
    }

    public void Delete(string key, RedisPrefixes prefix )
    {
        if (!isConnected)
        {
            _logger?.LogWarning("Cannot delete from Redis: Not connected.");
            return;
        }

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
        if( !isConnected)
        {
            _logger?.LogWarning("Cannot write to Redis: Not connected.");
            return;
        }

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
        if ( !isConnected )
        {
            _logger?.LogWarning("Cannot write to Redis: Not connected.");
            return string.Empty;
        }

        try
        {
            _logger?.LogDebug($"Attempting to get : {prefix.ToString()}:{key}");
            var value = db?.StringGet($"{prefix.ToString()}:{key}");
            return value.HasValue ? (string?)value : null;
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

    internal IEnumerable<KeyValuePair<string,string>> GetKeysByPrefix(RedisPrefixes? prefix = null)
    {
        if (!isConnected)
        {
            _logger?.LogWarning("Cannot retrieve keys: Not connected to Redis.");
            yield break;
        }

        if ( _redis == null )
        {
            _logger?.LogWarning("Cannot retrieve keys: Redis connection is null.");
            yield break;
        }

        string prefixString = prefix is null ? "*:" : $"{prefix.ToString()}:";
        _logger?.LogDebug("Retrieving keys with prefix: {prefix}", prefixString);

        var endpoint = _redis.GetEndPoints().FirstOrDefault();
        var server =   _redis.GetServer(endpoint);

        foreach (var key in server.Keys(pattern: $"{prefixString}*"))
        {           
            string? value = db?.StringGet(key) ?? string.Empty;
            
            if ( string.IsNullOrEmpty(value))
                continue;
            
            yield return new KeyValuePair<string, string>(key, value);
        }

        yield break;
    }
}