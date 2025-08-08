using StackExchange.Redis;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;


namespace RedisPlugin
{
    internal class Connection : IDisposable
    {
        private const int REDIS_TIMEOUT = 5000; // 5 seconds

        private ConnectionMultiplexer? _redis = null;
        private IDatabase? db = null;
        public Connection(string server , int port)
        {
            if (_redis == null)
            {
                try
                {
                    _redis = ConnectionMultiplexer.Connect($"{server}:{port},ConnectTimeout={REDIS_TIMEOUT}");
                    if (_redis.IsConnected)
                    {
                        Debug.WriteLine($"Successfully connected to Redis server at {server}:{port}");
                    }
                }
                catch (RedisConnectionException)
                {
                    Debug.WriteLine($"Failed to connect to Redis server at {server}:{port}.");
                    _redis = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An unexpected error occurred while connecting to Redis: {ex.Message}");
                    _redis = null;
                }
            }
            if (_redis != null)
            {
                Debug.WriteLine($"Redis connection established: {_redis.IsConnected}");

                if ( db == null)
                {
                    db = _redis.GetDatabase();
                }
            }
        }

        public void Dispose()
        {
            if (_redis != null)
            {
                try { 
                    Debug.WriteLine("Closing Redis connection...");
                    _redis.Close();
                    _redis.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error while closing Redis connection: {ex.Message}");
                }
                _redis = null;
            }
            db = null;
        }
        public bool IsConnected()
        {
            if (_redis == null)
            {
                return false;
            }
            Debug.WriteLine($"Redis connection status: {_redis.IsConnected}");
            return _redis.IsConnected;
        }
        public void write(string key, string value)
        {
            if (db != null)
            {
                db.StringSet(key, value);
            }
        }  
        public string? read(string key)
        {
            if (db != null)
            {
                return db.StringGet(key);
            }
            return null;
        }

    }
}
