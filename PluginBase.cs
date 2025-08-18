using System.Diagnostics;
using BroadcastPluginSDK.abstracts;
using Microsoft.Extensions.Configuration;
using RedisPlugin.Properties;
using Timer = System.Timers.Timer;

namespace RedisPlugin;

public class PluginBase : BroadcastPluginBase
{
    #region Private Methods

    private void SetTimer(bool connected)
    {
        if (started == false) return;

        var rate = SamplingRate;

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

            aTimer = new Timer(rate);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
    }

    #endregion

    #region Constants

    private const int DEFAULT_SAMPLE_RATE = 2000;
    private const int DEFAULT_SCAN_RATE = 5000; // Default reconnection rate in milliseconds
    private const int DEFAULT_PORT = 6379;
    private const string DEFAULT_SERVER = "localhost";

    #endregion

    #region Private fields

    private readonly bool started = false;
    private Connection? _connection;
    private int SamplingRate { get; } = DEFAULT_SAMPLE_RATE; // Default sampling rate
    private int Port { get; } = DEFAULT_PORT;
    private string Server { get; } = DEFAULT_SERVER;

    private readonly object _timerLock = new();
    private Timer? aTimer;

    #endregion

    #region IPLugin Implementation

    public PluginBase(IConfiguration configuration) : base(
        configuration, null,
        Resources.red,
        "Redis",
        "REDIS",
        "REDIS Cache plugin.")
    {
    }

    //  public override string Start()
    //  {
    ////
    /// SamplingRate = int.Parse(base.Configuration["sample"] ?? DEFAULT_SAMPLE_RATE.ToString());
    //      Server = Configuration["server"] ?? DEFAULT_SERVER;
    //      Port = int.Parse(Configuration["port"] ?? DEFAULT_PORT.ToString());

    //       Debug.WriteLine($"Starting {Name} plugin with sampling rate: {SamplingRate} ms");
    //      started = true;
    //       SetTimer(false);

    //       return $"Starting {Name} plugin with sampling rate: {SamplingRate} ms";
    //  }

    #endregion

    #region Public Methods

    public void Connect()
    {
        //  if (_infoPage is not null) _infoPage.Url = $"redis://{this.Server}:{this.Port}";
        _connection?.Dispose();
        _connection = new Connection(Server, Port);
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

        Icon = _connection?.IsConnected() == true ? Resources.green : Resources.red;
    }
}

#endregion