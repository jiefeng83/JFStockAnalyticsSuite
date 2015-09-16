using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeywordsSearcher
{
    public partial class PopupText : Form
    {
        public PopupText(string text, string searchString)
        {
            InitializeComponent();
            richTextBox1.Text = text;

            int selstart = 0;
            int sellength = 0;

            for (; ; )
            {
                selstart = richTextBox1.Text.IndexOf(searchString,selstart+sellength);
                if(selstart==-1) break;
                sellength = searchString.Length;
                richTextBox1.Select(selstart, sellength);
                richTextBox1.SelectionFont = new Font(richTextBox1.Font, FontStyle.Bold);
                richTextBox1.SelectionLength = 0;
            }
        }

        private void PopupText_Load(object sender, EventArgs e)
        {

        }

        private void richTextBox1_MouseLeave(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
