using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Collections.Specialized;
using System.Web;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

//ReportExtractor extracts annual and quarterly reports from StockInvestor.com. A webpro membership is required.

namespace ReportExtractor
{
    public partial class Form1 : Form
    {
        enum MyState { FIRST_LOAD, TO_LOGIN, READY }

        MyState myState = MyState.FIRST_LOAD;
        string extractFolderPath = @".\StockReports\";
        string currStockCode = "";
        string cookies = "";

        int totalStockNum = 0;
        int currStockNum = 0;

        string selectedType = "ALL";
        DateTime startDate = new DateTime(0);

        ConcurrentQueue<string> stockAddressQueue = new ConcurrentQueue<string>();
        Regex regex = new Regex(@"^-?\d+(?:\.\d+)?");
        List<string> stockList = new List<string>();
        List<MyFileInfo> CompleteFileList = new List<MyFileInfo>();
 
        public Form1()
        {
            InitializeComponent();

            if (!System.IO.Directory.Exists(extractFolderPath))
                System.IO.Directory.CreateDirectory(extractFolderPath);

            LogTextbox.Text += "Loading...\n";

            string userName = "";
            string password = "";
            string hdr = "Authorization: Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + password)) + System.Environment.NewLine;
            
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.ScrollBarsEnabled = false;
            webBrowser1.Navigate(String.Format("https://{0}:{1}@www.shareinvestor.com/user/do_login.html?use_https=0", userName, password), null, null, hdr);
            
            LogTextbox.Text = "Welcome!\n";
            LogTextbox.Text += "Please log into Share Investor!\n";
            LogTextbox.SelectionStart = LogTextbox.Text.Length;
            LogTextbox.ScrollToCaret();

            Task.Factory.StartNew(InitSetup);
        }
    
        void Start_Click(object sender, EventArgs e)
        {
            selectedType = comboBox1.Text;
            startDate = dateTimePicker1.Value;
            errorStockTextBox.Text = "";
            stockAddressQueue = new ConcurrentQueue<string>();
            currStockNum = 0;
            if (stockAddressQueue.Count == 0)
            {
                StringReader strReader = new StringReader(stockTextbox.Text);
                string str;

                for (; ; )
                {
                    str = strReader.ReadLine();
                    if (str != null && str != "") stockAddressQueue.Enqueue(str);
                    else break;
                }

                totalStockNum = stockAddressQueue.Count;
            }

            Task.Factory.StartNew(ReadStockFromStockQueue); //start ReadStockFromStockQueue from another thread
        }
        
        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(@"KeywordsSearcher.exe"))
            {
                MessageBox.Show("KeywordSearcher.exe does not exist.", "Error");
                return;
            }

            Process keywordSearcher = new Process();
            keywordSearcher.StartInfo.FileName = "KeywordsSearcher.exe";
            keywordSearcher.StartInfo.Arguments = extractFolderPath + " " + dateTimePicker1.Value.ToString("ddMMMyy"); // if you need some
            keywordSearcher.Start();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            System.IO.DirectoryInfo extractFolderInfo = new DirectoryInfo(extractFolderPath);

            foreach (FileInfo file in extractFolderInfo.GetFiles())
            {
                file.Delete();
            }
        }

        void InitSetup()
        {
            while (myState != MyState.READY)
            {
                Thread.Sleep(2000);

                try
                {
                    Invoke((MethodInvoker)delegate
                    {
                        switch (myState)
                        {
                            case MyState.FIRST_LOAD:
                                webBrowser1.Document.Window.ScrollTo(3000, 0);
                                myState = MyState.TO_LOGIN;
                                break;

                            case MyState.TO_LOGIN:
                                if (webBrowser1.DocumentText.Contains("Welcome,"))
                                {
                                    webBrowser1.Document.Window.ScrollTo(3000, 0);
                                    myState = MyState.READY;
                                    StartButton.Enabled = true;
                                    printText("Login Success!\n\nPress START to extract data of stocks.");
                                    cookies = GetCookies();
                                }
                                break;
                        }
                    });
                }
                catch { }
            }
        }

        void ReadStockFromStockQueue() //Single Thread Recurring Function
        {
            if (stockAddressQueue.IsEmpty)
            {
                printText("Extract Completed!");
				SaveAllFiles(CompleteFileList);
                return;
            }

            if (!stockAddressQueue.IsEmpty && stockAddressQueue.TryDequeue(out currStockCode))
            {
                int counter = 0;
                bool extractSuccess = false;

                currStockNum++;
                printText("Reading " + currStockCode + "... (" + currStockNum + "/" + totalStockNum + ")");
                webBrowser1.DocumentText = "";
                webBrowser1.Navigate("http://www.shareinvestor.com/fundamental/events_calendar.html#/?type=events_historical&counter=" + currStockCode + ".SI&market=sgx&page=1");

                while (!extractSuccess && counter <= 3)
                {
                    Thread.Sleep(2000);

                    Invoke((MethodInvoker)delegate
                    {
                        counter++;

                        extractSuccess = ExtractInfo(webBrowser1.DocumentText);

                        if (extractSuccess)
                            printText("Extract Success! (" + currStockNum + "/" + totalStockNum + ")");
                        else
                            printText("Extract Fail! Retry " + counter + "...");
                    });
                }
                if(!extractSuccess)
                    Invoke((MethodInvoker)delegate { errorStockTextBox.Text += currStockCode + "\n"; });

                ReadStockFromStockQueue();
            }
        }

        //########################### EXTRACT METHODS ###########################

        bool ExtractInfo(string contents)
        {
            //Get stockName
            //contents = trimFront(contents, "<OPTION selected");
            contents = trimFront(contents, "-- Select Counter --");
            contents = trimFront(contents, "selected");
            string stockName = getBetween2(contents, ">", " (").Replace("\r", "").Replace("\n", "");
            contents = trimFront(contents, stockName);
            stockName = Regex.Match(stockName, @"[0-9a-zA-Z\s^.]+").Value;
            stockName = stockName.Replace(" ", "");

            //Get symbol
            string stockCode = getBetween2(contents, "(", ")").Replace(".SI", "");
            if (stockCode != currStockCode)
            {
                return false;
            }

            List<MyFileInfo> fl = extractList(contents, stockName, stockCode);
            if (fl.Count == 0)
                return false;
            else
            {
                CompleteFileList.AddRange(fl);
                return true;
            }
        }

        List<MyFileInfo> extractList(string contents, string symbol, string code)
        {
            var list = new List<MyFileInfo>();
            string tempContent = contents;

            for (;;)
            {
                string temp = trimFront(tempContent, "http://repository.shareinvestor.com/rpt_view.pl", 400);

                tempContent = temp != "INVALID" ? temp : tempContent;
                string dateString = getBetween2(tempContent, "<TD>", "</TD>");
                tempContent = trimFront(tempContent, "href=\"/fundamental/factsheet.html?counter=");
                string typeString = getBetween2(tempContent, "<SPAN>", "</SPAN>").Replace(" ", "");
                string urlString = getBetween(tempContent, "http://repository.shareinvestor.com/rpt_view.pl", " \r\n");

                if (urlString == "EMPTY")
                    break;

                urlString = urlString.Remove(urlString.Length - 1);
                tempContent = trimFront(tempContent, urlString);

                try
                {
                    DateTime date = DateTime.ParseExact(dateString, "dd MMM yyyy",
                                    System.Globalization.CultureInfo.InvariantCulture);

                    list.Add(new MyFileInfo(symbol, code, urlString, typeString, date));
                }
                catch { }
            }

            return list;
        }

        void SaveAllFiles(List<MyFileInfo> fl)
        {
            foreach (var fi in fl)
            {
                if (fi.date > startDate && (fi.type == selectedType || selectedType == "ALL"))
                    Task.Factory.StartNew(() => saveFile(fi));
            }
        }

        void saveFile(MyFileInfo fi)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("Cookie: " + cookies);
                wc.DownloadFile(fi.url, extractFolderPath + fi.symbol + "_" + fi.code + "_" + fi.type + "_" + fi.date.ToString("ddMMMyy") + ".pdf");
                printText(fi.code + " Download completed! ");
            }
            catch (Exception e)
            {
                Console.WriteLine("savefile error: " + e.Message);
            }
        }

        string GetCookies()
        {
            if (webBrowser1.InvokeRequired)
            {
                return (string)webBrowser1.Invoke(new Func<string>(() => GetCookies()));
            }
            else
            {
                return webBrowser1.Document.Cookie;
            }
        }

        void printText(string text)
        {
            Invoke((MethodInvoker)delegate
            {
                LogTextbox.Text += text + "\n";
                LogTextbox.SelectionStart = LogTextbox.Text.Length;
                LogTextbox.ScrollToCaret();
            });
        }

        //########################### TEXT UTILITY METHODS ###########################

        static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0);
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "EMPTY";
            }
        }

        static string getBetween2(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "EMPTY";
            }
        }

        static string trimFront(string strSource, string strStart)
        {
            int Start;
            if (strSource.Contains(strStart))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                return strSource.Substring(Start, strSource.Length - Start);
            }
            else
            {
                return strSource;
            }
        }

        static string trimFront(string strSource, string strStart, int noOfCharBefore)
        {
            int Start;
            if (strSource.Contains(strStart))
            {
                Start = strSource.IndexOf(strStart, 0) - noOfCharBefore ;
                if (Start < 0)
                    return "INVALID";
                else
                    return strSource.Substring(Start, strSource.Length - Start);
            }
            else
            {
                return strSource;
            }
        }



    }

    public class MyFileInfo
    {
        public string symbol = "";
        public string code = "";
        public string url = "";
        public string type = "";
        public DateTime date = new DateTime(0);

        public MyFileInfo(string symbol, string code, string url, string type, DateTime date)
        {
            this.symbol = symbol;
            this.code = code;
            this.url = url;
            this.type = type;
            this.date = date;
        }
    }

}
