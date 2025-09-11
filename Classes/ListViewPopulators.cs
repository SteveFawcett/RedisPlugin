using BroadcastPluginSDK.Classes;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace RedisPlugin.Classes
{
    enum populatorType
    {
        Raw = 0,
        Simple = 1,
        Command = 2,
        Default = 99
    }

    internal static class ListViewPopulators
    {
        static populatorType _type = populatorType.Default;
        static public void Raw( ListView listView, Connection items , RedisPrefixes? prefix)
        {
            if (_type != populatorType.Raw)
            {
                _type = populatorType.Raw;
                AddListColumns(listView, ["Key", "Value"]);
            }

            listView.Items.Clear();
            int count = 0;
            foreach (var kvp in items.GetKeysByPrefix(prefix))
            {
                var item = new ListViewItem( kvp.Key );
                item.SubItems.Add(kvp.Value);

                listView.Items.Add(item);

                count++;
            }

            if (count > 0)
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            else
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            ResizeColumns(listView);
        }

        static public void Command(ListView listView, Connection items, RedisPrefixes prefix)
        {
            if (_type != populatorType.Command)
            {
                _type = populatorType.Command;
                AddListColumns(listView, ["Id", "Command" , "Status" , "Started"]);
            }
            listView.Items.Clear();

            int count = 0;
            foreach (var kvp in items.GetKeysByPrefix(prefix))
            {
                CommandItem item = JsonSerializer.Deserialize<CommandItem>(kvp.Value) ?? throw new JsonException("Deserialization returned null");
                var lv = new ListViewItem(item.Id);
                lv.SubItems.Add(item.Command.ToString());
                lv.SubItems.Add(item.Status.ToString());    
                lv.SubItems.Add(item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

                if( item.Status == CommandStatus.InProgress )
                    lv.BackColor = System.Drawing.ColorTranslator.FromHtml("#D0F0C0"); //tea green
                else if( item.Status == CommandStatus.Completed )
                    lv.BackColor = System.Drawing.Color.LightGray;
                else if( item.Status == CommandStatus.Failed )
                    lv.BackColor = System.Drawing.Color.LightCoral;
                else if (item.Status == CommandStatus.New)
                    lv.BackColor = System.Drawing.Color.AliceBlue;

                listView.Items.Add(lv);

                count++;
            }

            if( count > 0)
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            else
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            ResizeColumns(listView);
        }


        static public void Simple(ListView listView, Connection items, RedisPrefixes prefix)
        {
            if (_type != populatorType.Simple)
            {
                _type = populatorType.Simple;
                AddListColumns(  listView , [ "Key", "Value"]);
            }

            listView.Items.Clear();

            int count = 0;
            foreach (var kvp in items.GetKeysByPrefix(prefix))
            {
                string clean_name = kvp.Key.Replace( $"{prefix.ToString()}:" , "" );
                var item = new ListViewItem(clean_name);
                
                item.SubItems.Add(kvp.Value);

                listView.Items.Add(item);

                count++;
            }

            if (count > 0)
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            else
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            ResizeColumns(listView);

        }

        static void ResizeColumns(ListView listView)
        {
            foreach (ColumnHeader column in listView.Columns)
            {
                Size textSize = TextRenderer.MeasureText(column.Text, listView.Font);
                if (column.Width < textSize.Width + 20) // Add some padding
                {
                    column.Width = textSize.Width + 20;
                }

            }
        }

        static public void AddListColumns(ListView listView, List<string> items)
        {
            listView.Columns.Clear();
            foreach (var item in items)
            {
                ColumnHeader header = new ColumnHeader();
                Size textSize = TextRenderer.MeasureText(item, listView.Font);
                
                header.Text = item;
                header.Width = textSize.Width + 20; // Add some padding

                listView.Columns.Add(header);
            }
        }
    }
}

