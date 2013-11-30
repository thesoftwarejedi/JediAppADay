namespace AnAppADay.JediDigg.WinApp
{
    partial class BrowserForm
    {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(700, 700);
            this.webBrowser1.TabIndex = 0;
            // 
            // BrowserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = global::AnAppADay.JediDigg.WinApp.Properties.Settings.Default.BrowserSize;
            this.ControlBox = false;
            this.Controls.Add(this.webBrowser1);
            this.DataBindings.Add(new System.Windows.Forms.Binding("ClientSize", global::AnAppADay.JediDigg.WinApp.Properties.Settings.Default, "BrowserSize", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BrowserForm";
            this.Opacity = 0.8;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.SizeChanged += new System.EventHandler(this.BrowserForm_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
    }
}