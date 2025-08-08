namespace RedisPlugin
{
    partial class Info
    {
        public string Url { get => urlbox.Text; set => this.urlbox.Text = value; }
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            urlbox = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // urlbox
            // 
            urlbox.Location = new System.Drawing.Point(80, 379);
            urlbox.Name = "urlbox";
            urlbox.ReadOnly = true;
            urlbox.Size = new System.Drawing.Size(272, 23);
            urlbox.TabIndex = 4;
            urlbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Info
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            Controls.Add(urlbox);
            Name = "Info";
            Controls.SetChildIndex(urlbox, 0);
            ResumeLayout(false);
            PerformLayout();
            // 
            // Info
            // 
        }

        #endregion

        private System.Windows.Forms.TextBox urlbox;
    }
}
