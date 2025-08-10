using Microsoft.Extensions.Configuration;
using PluginBase;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Security.Policy;
using System.Windows.Forms;

namespace RedisPlugin
{
    public class Plugin : BroadcastPlugin
    {
        #region Constants
        const int DEFAULT_SAMPLE_RATE = 2000;
        const int DEFAULT_SCAN_RATE = 5000; // Default reconnection rate in milliseconds
        const int DEFAULT_PORT = 6379;
        const string DEFAULT_SERVER = "localhost";
        #endregion

        #region Private fields

        private bool started = false;
        private Connection? _connection = null;
        private int SamplingRate { get; set; } = DEFAULT_SAMPLE_RATE; // Default sampling rate
        private int Port { get; set; } = DEFAULT_PORT;
        private string Server { get; set; } = DEFAULT_SERVER;
        private static System.Timers.Timer? aTimer = null;
        #endregion

        #region IPLugin Implementation
        public override string Stanza => "Redis";

        public Plugin () :base()
        {
            _infoPage = new Info();

            ((Info)_infoPage).Url = $"redis://{this.Server}:{this.Port}";
            Name = "REDIS Plugin";
            Description = "Plugin for reading and writing to a REDIS Cache";
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            Icon = Properties.Resources.red;
        }

        public override void Start()
        {
            if (Configuration is not null)
            {
                SamplingRate = int.Parse(base.Configuration["sample"] ?? DEFAULT_SAMPLE_RATE.ToString());
                Server = this.Configuration["server"] ?? DEFAULT_SERVER ;
                Port = int.Parse(this.Configuration["port"] ?? DEFAULT_PORT.ToString());
            }

            Debug.WriteLine($"Starting {Name} plugin with sampling rate: {SamplingRate} ms");
            started = true;
            SetTimer(false);
        }
        #endregion

        #region Public Methods
        public void Connect()
        {
          //  if (_infoPage is not null) _infoPage.Url = $"redis://{this.Server}:{this.Port}";
            _connection?.Dispose();
            _connection = new Connection(this.Server, this.Port);
        }

        #endregion

        #region Private Methods
        private void SetTimer(bool connected)
        {
            if ( started == false)
            {
                return;
            }
            int rate = SamplingRate;
            
            if (connected == false && aTimer?.Interval != DEFAULT_SCAN_RATE)
            {
                rate = DEFAULT_SCAN_RATE; // Default reconnection rate
                Debug.WriteLine($"Setting timer with interval: {rate} ms");
            }

            if (aTimer != null)
            {
                // If the timer is already running, stop it.
                aTimer.Stop();
                aTimer.Dispose();
            }

            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(rate);
            // Hook up the Elapsed event for the timer. 
            // This event will be raised when the timer interval elapses.
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
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

            if (_connection?.IsConnected() == true)
            {
                Icon = Properties.Resources.green;
            }
            else
            {
                Icon = Properties.Resources.red;
            }
        }
    }
    #endregion
}   
