namespace DailyReportExtractor
{
    partial class DailyReportExtractor
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
            this.components = new System.ComponentModel.Container();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.label2 = new System.Windows.Forms.Label();
            this.LogTextbox = new System.Windows.Forms.RichTextBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SearchButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(102, 9);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.ScrollBarsEnabled = false;
            this.webBrowser1.Size = new System.Drawing.Size(490, 82);
            this.webBrowser1.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(99, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Status";
            // 
            // LogTextbox
            // 
            this.LogTextbox.Location = new System.Drawing.Point(102, 118);
            this.LogTextbox.Name = "LogTextbox";
            this.LogTextbox.ReadOnly = true;
            this.LogTextbox.Size = new System.Drawing.Size(492, 81);
            this.LogTextbox.TabIndex = 10;
            this.LogTextbox.Text = "";
            // 
            // StartButton
            // 
            this.StartButton.BackColor = System.Drawing.Color.LightGreen;
            this.StartButton.Enabled = false;
            this.StartButton.Location = new System.Drawing.Point(9, 38);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(86, 23);
            this.StartButton.TabIndex = 14;
            this.StartButton.Text = "START";
            this.StartButton.UseVisualStyleBackColor = false;
            this.StartButton.Click += new System.EventHandler(this.Start_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            // 
            // SearchButton
            // 
            this.SearchButton.BackColor = System.Drawing.Color.Khaki;
            this.SearchButton.Location = new System.Drawing.Point(10, 67);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(86, 23);
            this.SearchButton.TabIndex = 15;
            this.SearchButton.Text = "SEARCH";
            this.SearchButton.UseVisualStyleBackColor = false;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.BackColor = System.Drawing.Color.Salmon;
            this.ClearButton.Location = new System.Drawing.Point(9, 9);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(86, 23);
            this.ClearButton.TabIndex = 16;
            this.ClearButton.Text = "CLEAR";
            this.ClearButton.UseVisualStyleBackColor = false;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 209);
            this.Controls.Add(this.SearchButton);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.LogTextbox);
            this.Controls.Add(this.webBrowser1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "DailyReportExtractor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox LogTextbox;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.Button ClearButton;
    }
}

