using System.Drawing;
using System.Windows.Forms;
using RedisPlugin.Classes;
namespace RedisPlugin.Forms
{
    partial class CachePage
    {

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            textBox1 = new TextBox();
            btnFlush = new Button();
            pictureBox1 = new PictureBox();
            label1 = new Label();
            ComboBoxPrefixes = new ComboBox();
            listView1 = new ListView();
            timer1 = new System.Windows.Forms.Timer(components);
            btnRefresh = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.BackColor = SystemColors.Control;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Enabled = false;
            textBox1.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.Location = new Point(84, 44);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(250, 18);
            textBox1.TabIndex = 2;
            textBox1.Text = "Initializing";
            // 
            // btnFlush
            // 
            btnFlush.Location = new Point(583, 377);
            btnFlush.Name = "btnFlush";
            btnFlush.Size = new Size(75, 23);
            btnFlush.TabIndex = 4;
            btnFlush.Text = "Flush";
            btnFlush.UseVisualStyleBackColor = true;
            btnFlush.Click += btnFlush_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.red;
            pictureBox1.Location = new Point(13, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(65, 59);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 5;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(84, 3);
            label1.Name = "label1";
            label1.Size = new Size(132, 30);
            label1.TabIndex = 6;
            label1.Text = "REDIS Cache";
            // 
            // ComboBoxPrefixes
            // 
            ComboBoxPrefixes.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxPrefixes.FormattingEnabled = true;
            ComboBoxPrefixes.Location = new Point(13, 69);
            ComboBoxPrefixes.Name = "ComboBoxPrefixes";
            ComboBoxPrefixes.Size = new Size(132, 23);
            ComboBoxPrefixes.TabIndex = 7;
            ComboBoxPrefixes.SelectedIndexChanged += ComboBoxPrefixes_SelectedIndexChanged;
            // 
            // listView1
            // 
            listView1.Location = new Point(13, 103);
            listView1.Name = "listView1";
            listView1.Size = new Size(726, 268);
            listView1.TabIndex = 8;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Tick += ComboBoxPrefixes_SelectedIndexChanged;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(664, 377);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 9;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += ComboBoxPrefixes_SelectedIndexChanged;
            // 
            // CachePage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnRefresh);
            Controls.Add(listView1);
            Controls.Add(ComboBoxPrefixes);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Controls.Add(btnFlush);
            Controls.Add(textBox1);
            Name = "CachePage";
            Size = new Size(754, 420);
            Load += CachePage_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox textBox1;
        private Button btnFlush;
        private PictureBox pictureBox1;
        private Label label1;
        private ComboBox ComboBoxPrefixes;
        private ListView listView1;
        private System.Windows.Forms.Timer timer1;
        private System.ComponentModel.IContainer components;
        private Button btnRefresh;
    }
}
