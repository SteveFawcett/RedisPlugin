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
    public Connection Connection => _connection;
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
    public CachePage(ILogger<IPlugin> logger,  Connection connection)
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
        if( _connection != null )URL = $"{ _connection.Server}:{ _connection.Port}";
        
    }
    public void Redraw(KeyValuePair<string, string> kvp)
    {
        if (listView1.InvokeRequired)
        {
            listView1.Invoke(() => Redraw(kvp));
            return;
        }

        if (listView1.View != View.Details)
            listView1.View = View.Details;

        if (listView1.Columns.Count < 2)
        {
            listView1.Columns.Clear();
            listView1.Columns.Add("Key");
            listView1.Columns.Add("Value");
        }

        listView1.BeginUpdate();

        try
        {
            var items = listView1.Items.Find(kvp.Key, false);

            if (items.Length > 0)
            {
                items[0].SubItems[1].Text = kvp.Value;
            }
            else
            {
                _logger.LogDebug("{0} => {1}", kvp.Key, kvp.Value);

                var item = new ListViewItem(kvp.Key)
                {
                    Name = kvp.Key
                };
                item.SubItems.Add(kvp.Value);
                listView1.Items.Add(item);
            }

            int totalWidth = listView1.ClientSize.Width;

            listView1.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);

            int firstColWidth = listView1.Columns[0].Width;
            listView1.Columns[1].Width = Math.Max(100, totalWidth - firstColWidth);
        }
        finally
        {
            listView1.EndUpdate();
        }
    }

    public void Redraw(Dictionary<string, string> myDict)
    {
     
        foreach (var kvp in myDict)
        {
            Redraw(kvp);
        }
    }

    private void Connected_Click(object sender, EventArgs e)
    {
        Connected.Checked = !Connected.Checked;
    }
}