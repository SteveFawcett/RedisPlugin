using BroadcastPluginSDK;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
namespace RedisPlugin;

public partial class CachePage : UserControl, IInfoPage
{
    private string? _description;
    private Image? _icon;
    private string? _name;
    private string? _version;
    private readonly ILogger<IPlugin> _logger;

    public string URL
    {
        get => textBox1.Text; set => textBox1.Text = value;
    }
    public CachePage( ILogger<IPlugin> logger , string server , int port)
    {
        this._logger = logger;
        InitializeComponent();

        URL = $"redis://{server}:{port}";
        _logger.LogInformation(URL);

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

    public void Redraw(KeyValuePair<string, string> kvp)
    {
        if (listView1.InvokeRequired)
        {
            if (listView1 != null && listView1.IsHandleCreated && !listView1.IsDisposed)
                listView1.Invoke(() => Redraw(kvp));
            return;
        }

        if (listView1.View != View.Details)
            listView1.View = View.Details;

        if (listView1.Columns.Count < 2)
        {
            listView1.Columns.Clear();
            listView1.Columns.Add("Key", 250);
            listView1.Columns.Add("Value", 150);
        }

        var items = listView1.Items.Find(kvp.Key, false);

        if (items.Length > 0)
        {
            items[0].SubItems[1].Text = kvp.Value;
        }
        else
        {
            _logger.LogDebug( "{0} => {1}", kvp.Key, kvp.Value);
           
            var item = new ListViewItem(kvp.Key)
            {
                Name = kvp.Key
            };
            item.SubItems.Add(kvp.Value);
            listView1.Items.Add(item);
        }
    }
    public void Redraw(Dictionary<string, string> myDict)
    {
        foreach (var kvp in myDict)
        {
            Redraw(kvp);
        }
    }
}