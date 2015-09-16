﻿namespace StockFundamentalStudy
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.StartButton = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.stockTextbox = new System.Windows.Forms.RichTextBox();
            this.ReadButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.LogTextbox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.errorStockTextBox = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(169, 18);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(30, 31);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.ScrollBarsEnabled = false;
            this.webBrowser1.Size = new System.Drawing.Size(735, 126);
            this.webBrowser1.TabIndex = 2;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            // 
            // StartButton
            // 
            this.StartButton.BackColor = System.Drawing.Color.LightGreen;
            this.StartButton.Enabled = false;
            this.StartButton.Location = new System.Drawing.Point(18, 108);
            this.StartButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(129, 35);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "START";
            this.StartButton.UseVisualStyleBackColor = false;
            this.StartButton.Click += new System.EventHandler(this.Start_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // stockTextbox
            // 
            this.stockTextbox.Location = new System.Drawing.Point(18, 294);
            this.stockTextbox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.stockTextbox.Name = "stockTextbox";
            this.stockTextbox.Size = new System.Drawing.Size(110, 122);
            this.stockTextbox.TabIndex = 4;
            this.stockTextbox.Text = "";
            this.stockTextbox.TextChanged += new System.EventHandler(this.stockTextbox_TextChanged);
            // 
            // ReadButton
            // 
            this.ReadButton.Location = new System.Drawing.Point(18, 18);
            this.ReadButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ReadButton.Name = "ReadButton";
            this.ReadButton.Size = new System.Drawing.Size(129, 35);
            this.ReadButton.TabIndex = 5;
            this.ReadButton.Text = "READ";
            this.ReadButton.UseVisualStyleBackColor = true;
            this.ReadButton.Click += new System.EventHandler(this.ReadButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(18, 63);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(129, 35);
            this.SaveButton.TabIndex = 5;
            this.SaveButton.Text = "SAVE";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // LogTextbox
            // 
            this.LogTextbox.Location = new System.Drawing.Point(292, 294);
            this.LogTextbox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LogTextbox.Name = "LogTextbox";
            this.LogTextbox.ReadOnly = true;
            this.LogTextbox.Size = new System.Drawing.Size(592, 122);
            this.LogTextbox.TabIndex = 6;
            this.LogTextbox.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 271);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Stock List";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(288, 271);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Status";
            // 
            // errorStockTextBox
            // 
            this.errorStockTextBox.Location = new System.Drawing.Point(152, 294);
            this.errorStockTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.errorStockTextBox.Name = "errorStockTextBox";
            this.errorStockTextBox.ReadOnly = true;
            this.errorStockTextBox.Size = new System.Drawing.Size(110, 122);
            this.errorStockTextBox.TabIndex = 4;
            this.errorStockTextBox.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(152, 271);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(118, 20);
            this.label3.TabIndex = 7;
            this.label3.Text = "Error Stock List";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.LightGreen;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(18, 152);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(129, 39);
            this.button1.TabIndex = 8;
            this.button1.Text = "Historical Price";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // stopButton
            // 
            this.stopButton.BackColor = System.Drawing.Color.LightCoral;
            this.stopButton.Location = new System.Drawing.Point(169, 152);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(110, 39);
            this.stopButton.TabIndex = 9;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = false;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.LightGreen;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(18, 201);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(129, 39);
            this.button2.TabIndex = 8;
            this.button2.Text = "Annual Report";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(923, 440);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LogTextbox);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.ReadButton);
            this.Controls.Add(this.errorStockTextBox);
            this.Controls.Add(this.stockTextbox);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.StartButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "Form1";
            this.Text = "Stock Fundamental Study";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RichTextBox stockTextbox;
        private System.Windows.Forms.Button ReadButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.RichTextBox LogTextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox errorStockTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button button2;

    }
}
