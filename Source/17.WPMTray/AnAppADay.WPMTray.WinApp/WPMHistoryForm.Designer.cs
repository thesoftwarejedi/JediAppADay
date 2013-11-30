namespace AnAppADay.WPMTray.WinApp
{
    partial class WPMHistoryForm
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
            this.graphControl1 = new AnAppADay.WPMTray.WinApp.GraphControl();
            this.SuspendLayout();
            // 
            // graphControl1
            // 
            this.graphControl1.BackColor = System.Drawing.Color.Black;
            this.graphControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphControl1.Location = new System.Drawing.Point(3, 3);
            this.graphControl1.Name = "graphControl1";
            this.graphControl1.Size = new System.Drawing.Size(183, 119);
            this.graphControl1.TabIndex = 0;
            this.graphControl1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.graphControl1_MouseDoubleClick);
            // 
            // WPMHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(189, 125);
            this.ControlBox = false;
            this.Controls.Add(this.graphControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(163, 90);
            this.Name = "WPMHistoryForm";
            this.Opacity = 0.7;
            this.Padding = new System.Windows.Forms.Padding(3);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.TopMost = true;
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WPMHistoryForm_MouseDown);
            this.ResumeLayout(false);

        }

        #endregion

        private GraphControl graphControl1;
    }
}