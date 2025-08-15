using BroadcastPluginSDK;
using System.Diagnostics;
using System.Reflection;

namespace RedisPlugin
{
    public class PluginBase : BroadcastPluginBase
    {
        #region Constants
        private const int DEFAULT_SAMPLE_RATE = 2000;
        private const int DEFAULT_SCAN_RATE = 5000; // Default reconnection rate in milliseconds
        private const int DEFAULT_PORT = 6379;
        private const string DEFAULT_SERVER = "localhost";
        #endregion

        #region Private fields

        private bool started = false;
        private Connection? _connection = null;
        private int SamplingRate { get; set; } = DEFAULT_SAMPLE_RATE; // Default sampling rate
        private int Port { get; set; } = DEFAULT_PORT;
        private string Server { get; set; } = DEFAULT_SERVER;

        private readonly object _timerLock = new();
        private System.Timers.Timer? aTimer = null;
        #endregion

        #region IPLugin Implementation
        public override string Stanza => "Redis";

        public PluginBase() : base()
        {

            // ((Info)_infoPage).Url = $"redis://{this.Server}:{this.Port}";
            Name = "REDIS PluginBase";
            Description = "PluginBase for reading and writing to a REDIS Cache";
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            Icon = Properties.Resources.red;
        }

        public override string Start()
        {
            if (Configuration is not null)
            {
                SamplingRate = int.Parse(base.Configuration["sample"] ?? DEFAULT_SAMPLE_RATE.ToString());
                Server = Configuration["server"] ?? DEFAULT_SERVER;
                Port = int.Parse(Configuration["port"] ?? DEFAULT_PORT.ToString());
            }

            Debug.WriteLine($"Starting {Name} plugin with sampling rate: {SamplingRate} ms");
            started = true;
            SetTimer(false);

            return $"Starting {Name} plugin with sampling rate: {SamplingRate} ms";
        }
        #endregion

        #region Public Methods
        public void Connect()
        {
            //  if (_infoPage is not null) _infoPage.Url = $"redis://{this.Server}:{this.Port}";
            _connection?.Dispose();
            _connection = new Connection(Server, Port);
        }

        #endregion

        #region Private Methods
        private void SetTimer(bool connected)
        {
            if (started == false)
            {
                return;
            }

            int rate = SamplingRate;

            if (connected == false && aTimer?.Interval != DEFAULT_SCAN_RATE)
            {
                rate = DEFAULT_SCAN_RATE; // Default reconnection rate
                Debug.WriteLine($"Setting timer with interval: {rate} ms");
            }

            lock (_timerLock) // Lock to prevent race conditions
            {
                if (aTimer != null)
                {
                    aTimer.Stop();
                    aTimer.Dispose();
                    aTimer = null;
                }

                aTimer = new System.Timers.Timer(rate);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }
        }
        #endregion
        #region Event Handlers
        private void OnTimedEvent(object? source, EventArgs e)
        {
            // This method is called when the timer elapses.
            // So when the timer ticks we will send some test data
            if (_connection == null || _connection.IsConnected() == false)
            {
                Debug.WriteLine($"Attempting connecting to Redis at {Server}:{Port}");
                try
                {
                    Connect();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error with REDIS {typeof(SourceFilter)} connection: {ex.Message} ");
                }
            }

            SetTimer(_connection?.IsConnected() ?? false);

            Icon = _connection?.IsConnected() == true ? Properties.Resources.green : Properties.Resources.red;
        }
    }
    #endregion
}
