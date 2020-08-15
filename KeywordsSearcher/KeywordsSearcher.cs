using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace KeywordsSearcher
{
    public partial class KeywordsSearcher : Form
    {
        List<FileInfo> fileInfoList = new List<FileInfo>();
        public KeywordsSearcher()
        {
            InitializeComponent();
            DateTime t = DateTime.MinValue;

            dateTimePicker2.Value = DateTime.Now;
            textBox1.Text = Properties.Settings.Default["Path"].ToString();
            textBox2.Text = Properties.Settings.Default["SearchString"].ToString();
            dateTimePicker1.Value = (DateTime)Properties.Settings.Default["StartDate"];
            textBox1.Text = (string)Properties.Settings.Default["Path"];
            
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                textBox1.Text = args[1];

                if (args.Length > 2)
                {
                    dateTimePicker1.Value = DateTime.ParseExact(args[2], "ddMMMyy", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["Path"] = textBox1.Text;
            Properties.Settings.Default["SearchString"] = textBox2.Text;
            Properties.Settings.Default["StartDate"] = dateTimePicker1.Value;
            Properties.Settings.Default.Save();

            fileInfoList.Clear();
            dataGridView1.Rows.Clear();

            string path = @textBox1.Text+ @"\";
            string[] searchText = textBox2.Text.Split(',');

            if (!System.IO.Directory.Exists(path))
            {
                MessageBox.Show("Directory does not exist.", "Error");
                return;
            }

            foreach (string filepath in Directory.EnumerateFiles(path))
            {
                try
                {
                    if (filepath.Contains(".txt"))
                    {
                        string name = filepath.Replace(path, "");
                        string[] str = name.Split('_', '.');
                        DateTime date = DateTime.ParseExact(str[3], "ddMMMyy",
                                        System.Globalization.CultureInfo.InvariantCulture);
                        string code = str[1];
                        string symbol = str[0];

                        if(date > dateTimePicker1.Value && date < dateTimePicker2.Value && (symbol.Contains(textBox3.Text) || code == textBox3.Text || textBox3.Text == ""))
                            fileInfoList.Add(new FileInfo(filepath.Replace(path, ""), filepath, searchText));
                    }
                }
                catch { }
            }

            dataGridView1.SuspendLayout();
            foreach (var fi in fileInfoList)
            {
                foreach (var ti in fi.textInfoList)
                {
                    dataGridView1.Rows.Add();
                    try
                    {
                        int rowCount = dataGridView1.RowCount - 1;
                        dataGridView1.Rows[rowCount].Cells[0].Value = ti.filename;
                        dataGridView1.Rows[rowCount].Cells[1].Value = ti.keyword;
                        dataGridView1.Rows[rowCount].Cells[2].Value = ti.text;
                    }
                    catch { }
                }
            }
                dataGridView1.ResumeLayout();
        }

        private void convertButton_Click(object sender, EventArgs e)
        {
            string path = @textBox1.Text + @"\";

            if (!System.IO.Directory.Exists(path))
            {
                MessageBox.Show("Directory does not exist.", "Error");
                return;
            }

            int count = 0;

            foreach (string filepath in Directory.EnumerateFiles(path))
            {
                try
                {
                    if (filepath.Contains(".pdf"))
                    {
                        string text = ExtractTextFromPdf(filepath);
                        System.IO.StreamWriter file = new System.IO.StreamWriter(filepath.Replace(".pdf",".txt"));
                        file.Write(text);
                        file.Close();
                        count++;
                    }
                }
                catch { }
            }

            MessageBox.Show(count + " files converted.", "KeywordSearcher");
        }

        public static string ExtractTextFromPdf(string path)
        {
            using (PdfReader reader = new PdfReader(path))
            {
                StringBuilder text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    string thePage = PdfTextExtractor.GetTextFromPage(reader, i);
                    string[] theLines = thePage.Split('\n');
                    foreach (var theLine in theLines)
                    {
                        text.AppendLine(theLine);
                    }
                }

                return text.ToString();
            }
        } 

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            PopupText popupText = new PopupText(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
            popupText.Left = getPopupLeft(Cursor.Position.X, popupText.Width);
            popupText.Top  = getPopupTop(Cursor.Position.Y, popupText.Height);
            popupText.Show();
        }


        public static int getPopupLeft(int mouseX, int popupWidth)
        {
            while (mouseX > System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width)
            {
                mouseX -= System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            }
            while (mouseX < 0)
            {
                mouseX += System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            }
            if (mouseX >= (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2))
                return System.Windows.Forms.Cursor.Position.X - popupWidth + 40;
            else
                return System.Windows.Forms.Cursor.Position.X - 20;
        }

        public static int getPopupTop(int mouseY, int popupHeight)
        {
            int height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            while (mouseY > height)
            {
                mouseY -= height;
            }
            while (mouseY < 0)
            {
                mouseY += height;
            }

            if (mouseY >= (height / 2))
                return System.Windows.Forms.Cursor.Position.Y - popupHeight + 20;
            else
                return System.Windows.Forms.Cursor.Position.Y - 20;
        }


    }

    public class FileInfo
    {
        public string filename = "";
        public string completePath = "";
        public string completeText = "";
        public List<TextInfo> textInfoList = new List<TextInfo>();

        public FileInfo(string name, string completePath, string[] searchText)
        {
            this.filename = name;
            this.completePath = completePath;

            try
            {
                this.completeText = File.ReadAllText(completePath);
            }
            catch { }

            string content = completeText;
            string[] lines = completeText.Split(new string[] { "\r\n \r\n" }, StringSplitOptions.None);
            List<string> lList = new List<string>();
            foreach (string l in lines)
            {
                if (l.Replace(" ", "") != "")
                    lList.Add(l);
            }

            foreach (string s in searchText)
            {
                for (int i = 0; i < lList.Count; i++)
                {
                    if (lList[i].Contains(s))
                    {
                        string myLine = i - 1 >= 0 ? lList[i - 1] + "\n\n" + lList[i] : lList[i];
                        myLine = i + 1 < lList.Count ? myLine + "\n\n" + lList[i + 1] : myLine;
                        textInfoList.Add(new TextInfo(filename, s, myLine));
                    }
                }
            }
        }
        
        public static string getChunk(string strSource, string strStart)
        {
            if (strSource.Contains(strStart))
            {
                int Index = strSource.IndexOf(strStart, 0);
                int Start = Math.Max(0, Index - 100);
                int End = Math.Min(strSource.Length-1, Index + 100);
                
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "EMPTY";
            }
        }

        public static string trimFront(string strSource, string strStart)
        {
            if (strSource.Contains(strStart))
            {
                int Start = Math.Min(strSource.Length-1, strSource.IndexOf(strStart, 0) + strStart.Length + 80);
                return strSource.Substring(Start, strSource.Length - Start);
            }
            else
            {
                return strSource;
            }
        }
    }
    public class TextInfo
    {
        public string filename = "";
        public string keyword = "";
        public string text = "";

        public TextInfo(string fn, string kw, string t)
        {
            filename = fn;
            keyword = kw;
            text = t;
        }
    }
}
