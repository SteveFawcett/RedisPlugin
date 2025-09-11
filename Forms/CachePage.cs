using BroadcastPluginSDK;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Logging;
using RedisPlugin.Classes;
using RedisPlugin.Properties;
using System.Drawing;
using System.Globalization;

namespace RedisPlugin.Forms;

public partial class CachePage : UserControl, IInfoPage
{
    private string? _description;
    private Image? _icon;
    private string? _name;
    private string? _version;
    private readonly ILogger<IPlugin> _logger;
    private Connection _connection;

    public CachePage(ILogger<IPlugin> logger, Connection connection)
    {
        _logger = logger;
        _connection = connection;

        InitializeComponent();

        listView1.View = View.Details;
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

    public void UpdateInfoPage(Connection value)
    {
        _connection = value;

        pictureBox1.Image = value.isConnected ? Resources.green : Resources.red;
        textBox1.Text = value.isConnected ? $"{_connection.Server}:{_connection.Port}" : "Not connected";
        btnFlush.Enabled = value.isConnected;
    }

    private void btnFlush_Click(object sender, EventArgs e)
    {
        if (_connection != null)
            _connection.flush();
        else
            _logger.LogWarning("No connection to flush");
    }

    private void CachePage_Load(object sender, EventArgs e)
    {
        PopulateRedisPrefixes();
    }

    private void PopulateRedisPrefixes()
    {
        ComboBoxPrefixes.Items.Add("All");
        ComboBoxPrefixes.SelectedIndex = 0;

        foreach (var prefix in Enum.GetValues(typeof(RedisPrefixes)))
        {
            string input = prefix.ToString() ?? "Shouldnt be null";
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string result = textInfo.ToTitleCase(input.ToLower());
            ComboBoxPrefixes.Items.Add(result);
        }
    }

    private void PopulateListView(RedisPrefixes? prefix)
    {
        _logger?.LogDebug("Populating list view with prefix: {prefix}", prefix.ToString() );

        listView1.BeginUpdate();

        listView1.Items.Clear();
        if (_connection != null && _connection.isConnected)
        {
            switch (prefix)
            {
                case RedisPrefixes.COMMAND:
                    ListViewPopulators.Command(listView1, _connection, RedisPrefixes.COMMAND);
                    break;
                case RedisPrefixes.DATA:
                    ListViewPopulators.Simple(listView1, _connection, RedisPrefixes.DATA);
                    break;
                default:
                    ListViewPopulators.Raw(listView1, _connection, prefix);
                    break;
            }
        }
        else
        {
            _logger?.LogWarning("No connection to populate list view");
        }

        listView1.EndUpdate();
    }

    private void ComboBoxPrefixes_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ComboBoxPrefixes.SelectedItem is string selectedItem)
        {
            var prefix = selectedItem.ToUpper();
            if (prefix == "ALL")
            {
                PopulateListView(null);
                return;
            }
            PopulateListView( (RedisPrefixes)Enum.Parse( typeof(RedisPrefixes) , selectedItem.ToUpper() ));
        }
        else
        {
            PopulateListView( null );
        }
    }
}