using BroadcastPluginSDK.Classes;
using System.Text.Json;

namespace RedisPlugin.Classes
{
    internal static class ListViewPopulators
    {
        static public void Raw( ListView listView, Connection items , RedisPrefixes? prefix)
        {
            listView.Columns.Clear();
            listView.Columns.Add("Key", 50);
            listView.Columns.Add("Value", 50);

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
        }

        static public void Command(ListView listView, Connection items, RedisPrefixes prefix)
        {
            listView.Columns.Clear();
            listView.Columns.Add("id", 50);
            listView.Columns.Add("Command", 50);
            listView.Columns.Add("Status", 50);
            listView.Columns.Add("Started", 50);

            int count = 0;
            foreach (var kvp in items.GetKeysByPrefix(prefix))
            {
                CommandItem item = JsonSerializer.Deserialize<CommandItem>(kvp.Value) ?? throw new JsonException("Deserialization returned null");
                var lv = new ListViewItem(item.Id);
                lv.SubItems.Add(item.Command.ToString());
                lv.SubItems.Add(item.Status.ToString());    
                lv.SubItems.Add(item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                listView.Items.Add( lv );

                count++;
            }

            if( count > 0)
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            else
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        static public void Simple(ListView listView, Connection items, RedisPrefixes prefix)
        {
            listView.Columns.Clear();
            listView.Columns.Add("Key", 50);
            listView.Columns.Add("Value", 50);

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
        }
    }
}

