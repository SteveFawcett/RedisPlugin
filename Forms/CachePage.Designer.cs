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
            textBox1 = new TextBox();
            Connected = new CustomRadioButton();
            btnFlush = new Button();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.BackColor = SystemColors.Control;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Enabled = false;
            textBox1.Font = new Font("Segoe UI", 12F);
            textBox1.Location = new Point(133, 362);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(250, 22);
            textBox1.TabIndex = 2;
            textBox1.Text = "Initializing";
            textBox1.TextAlign = HorizontalAlignment.Center;
            // 
            // Connected
            // 
            Connected.AutoSize = true;
            Connected.Font = new Font("Segoe UI", 12F);
            Connected.ImageAlign = ContentAlignment.BottomCenter;
            Connected.Location = new Point(3, 362);
            Connected.Name = "Connected";
            Connected.Size = new Size(121, 25);
            Connected.TabIndex = 3;
            Connected.TabStop = true;
            Connected.Text = "Disconnected";
            Connected.UseVisualStyleBackColor = true;
            Connected.Click += Connected_Click;
            // 
            // btnFlush
            // 
            btnFlush.Location = new Point(368, 390);
            btnFlush.Name = "btnFlush";
            btnFlush.Size = new Size(75, 23);
            btnFlush.TabIndex = 4;
            btnFlush.Text = "Flush";
            btnFlush.UseVisualStyleBackColor = true;
            btnFlush.Click += btnFlush_Click;
            // 
            // CachePage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnFlush);
            Controls.Add(Connected);
            Controls.Add(textBox1);
            Name = "CachePage";
            Size = new Size(448, 420);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox textBox1;
        private CustomRadioButton Connected;
        private Button btnFlush;
    }
}
