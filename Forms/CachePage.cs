using BroadcastPluginSDK;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Logging;
using RedisPlugin.Classes;

namespace RedisPlugin.Forms;

public partial class CachePage : UserControl, IInfoPage
{
    private string? _description;
    private Image? _icon;
    private string? _name;
    private string? _version;
    private readonly ILogger<IPlugin> _logger;
    private Connection _connection;
    public Connection Connection
    {
        get { return _connection; }
        set { _connection = value; }
    }

    public string URL
    {
        set
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action(() => textBox1.Text = value));
            }
            else
            {
                textBox1.Text = value;
            }
        }
    }
    public CachePage(ILogger<IPlugin> logger, Connection connection)
    {
        _logger = logger;
        _connection = connection;

        InitializeComponent();
    }

    public Image? Icon
    {
        get => _icon;
        set => _icon = value;
    }

    public new string Name
    {
        set => _name = value;
        get => _name ?? string.Empty;
    }

    public string Version
    {
        set => _version = value;
        get => _version ?? string.Empty;
    }

    public string Description
    {
        set => _description = value;
        get => _description ?? string.Empty;
    }

    public Control GetControl()
    {
        return this;
    }

    public void SetState(bool value)
    {
        if (Connected.InvokeRequired)
        {
            if (Connected != null && Connected.IsHandleCreated && !Connected.IsDisposed)
                Connected.Invoke(() => SetState(value));
            return;
        }
        Connected.Checked = value;
        if (_connection != null) URL = $"{_connection.Server}:{_connection.Port}";

        btnFlush.Enabled = value;

    }

    private void Connected_Click(object sender, EventArgs e)
    {
        Connected.Checked = !Connected.Checked;
    }

    private void btnFlush_Click(object sender, EventArgs e)
    {
        if( Connection  != null) 
            Connection.flush();
        else
            _logger.LogWarning("No connection to flush");
    }
}