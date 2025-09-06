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
            listView1 = new ListView();
            textBox1 = new TextBox();
            Connected = new CustomRadioButton();
            SuspendLayout();
            // 
            // listView1
            // 
            listView1.Location = new Point(3, 7);
            listView1.Name = "listView1";
            listView1.Size = new Size(440, 349);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // textBox1
            // 
            textBox1.BackColor = SystemColors.Control;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Text = "Initializing";
            textBox1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox1.Enabled = false;
            textBox1.Location = new Point(133, 362);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(250, 26);
            textBox1.TabIndex = 2;
            textBox1.TextAlign = HorizontalAlignment.Center;
            // 
            // Connected
            // 
            Connected.AutoSize = true;
            Connected.ImageAlign = ContentAlignment.BottomCenter;
            Connected.Location = new Point(3, 362);
            Connected.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Connected.Name = "Connected";
            Connected.Size = new Size(83, 19);
            Connected.TabIndex = 3;
            Connected.TabStop = true;
            Connected.Text = "Connected";
            Connected.UseVisualStyleBackColor = true;
            Connected.Click += Connected_Click;
            // 
            // CachePage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Connected);
            Controls.Add(textBox1);
            Controls.Add(listView1);
            Name = "CachePage";
            Size = new Size(448, 420);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ListView listView1;
        private TextBox textBox1;
        private CustomRadioButton Connected;
    }
}
