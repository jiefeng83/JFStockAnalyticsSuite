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

namespace DailyReportExtractor
{
    public partial class Form1 : Form
    {
        bool isFirstLoad = true;
        bool startExtract = false;
        int retries = 0;
        string currStockCode = "";
        string cookies = "";

        int totalStockNum = 0;
        int currStockNum = 0;

        string selectedType = "ALL";
        DateTime startDate = new DateTime(0);

        ConcurrentQueue<string> stockAddressQueue = new ConcurrentQueue<string>();

        Regex regex = new Regex(@"^-?\d+(?:\.\d+)?");

        List<string> stockList = new List<string>();

        int secCounter = 0;
 
        public Form1()
        {
            InitializeComponent();
            LogTextbox.Text += "Loading...\n";
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.ScrollBarsEnabled = false;
            webBrowser1.Navigate("http://www.shareinvestor.com/");
        }
    
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            label1.Text = "OK";
            secCounter = 0;
            timer1.Enabled = true;
        }

        private void Start_Click(object sender, EventArgs e)
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

            startExtract = true;
            Task.Factory.StartNew(ReadStockFromStockQueue);
        }

        private void ReadStockFromStockQueue()
        {
            if (!stockAddressQueue.IsEmpty && stockAddressQueue.TryDequeue(out currStockCode))
            {
                currStockNum++;
                printText("Reading " + currStockCode + "... (" + currStockNum + "/" + totalStockNum + ")");
                webBrowser1.Navigate("http://www.shareinvestor.com/fundamental/events_calendar.html#/?type=events_historical&counter="+currStockCode+".SI&market=sgx&page=1");
            }
            else if (stockAddressQueue.IsEmpty)
            {
                printText("Extract Completed!");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (secCounter > 2)
            {
                if (startExtract) //ExtractMode
                {
                    if (extractInfo(webBrowser1.DocumentText))
                    {
                        timer1.Enabled = false;
                        retries = 0;
                        printText("Extract Success! (" + currStockNum + "/" + totalStockNum + ")");
                        webBrowser1.DocumentText = "";
                        Task.Factory.StartNew(ReadStockFromStockQueue);
                    }
                    else
                    {
                        if (retries > 3)
                        {
                            errorStockTextBox.Text += currStockCode + "\n";
                            ReadStockFromStockQueue();
                            timer1.Enabled = false;
                            retries = 0;
                        }
                        else
                        {
                            retries++;
                            printText("Extract Fail. Retry #: " + retries);
                        }
                    }
                }
                else //Starting Mode
                {
                    if (webBrowser1.DocumentText.Contains("Welcome,"))
                    {
                        StartButton.Enabled = true;
                        printText("Login Success!\n\nPress READ to read Stock List from file.\nPress START to extract data of stocks in Stock List.");
                        cookies = GetCookies();
                    }
                    else
                    {
                        if (isFirstLoad)
                        {
                            isFirstLoad = false;
                            LogTextbox.Text = "Welcome!\n";
                        }
                        LogTextbox.Text += "Please log into Share Investor!\n";
                        LogTextbox.SelectionStart = LogTextbox.Text.Length;
                        LogTextbox.ScrollToCaret();
                    }

                    webBrowser1.Document.Window.ScrollTo(3000, 0);
                    timer1.Enabled = false;
                    secCounter = 0;
                }
            }
            else
                secCounter++;
        }

        bool extractInfo(string contents)
        {
            //Get stockName
            contents = trimFront(contents, "<OPTION selected");
            string stockName = getBetween2(contents, ">", " (").Replace("\r", "").Replace("\n", "");
            contents = trimFront(contents, stockName);
            stockName = Regex.Match(stockName, @"[0-9a-zA-Z\s^.]+").Value;
            stockName = stockName.Replace(" ", "");

            //Get symbol
            string stockCode = getBetween2(contents, "(", ")").Replace(".SI", "");
            if (stockCode != currStockCode)
            {
                secCounter = 0;
                return false;
            }

            List<FileInfo> fl = extractList(contents, stockName, stockCode);
            SaveAllFiles(fl);
            return true;
        }

        List<FileInfo> extractList(string contents, string symbol, string code)
        {
            var list = new List<FileInfo>();
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

                    list.Add(new FileInfo(symbol, code, urlString, typeString, date));
                }
                catch { }
            }

            return list;
        }


        void SaveAllFiles(List<FileInfo> fl)
        {
            foreach (var fi in fl)
            {
                if (fi.date > startDate && (fi.type == selectedType || selectedType == "ALL"))
                    Task.Factory.StartNew(() => saveFile(fi));
            }
        }

        void saveFile(FileInfo fi)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("Cookie: " + cookies);
                wc.DownloadFile(fi.url, @"C:\Users\GenkCapital\Desktop\StockReports\" + fi.symbol + "_" + fi.code + "_" + fi.type + "_" +  fi.date.ToString("ddMMMyy") + ".pdf");
                printText(fi.code + " Download completed! ");
            }
            catch (Exception e)
            {
                Console.WriteLine("savefile error: " + e.Message);
            }
        }


        private string GetCookies()
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

        private void printText(string text)
        {
            Invoke((MethodInvoker)delegate
            {
                LogTextbox.Text += text + "\n";
                LogTextbox.SelectionStart = LogTextbox.Text.Length;
                LogTextbox.ScrollToCaret();
            });
        }


        public static string getBetween(string strSource, string strStart, string strEnd)
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


        public static string getBetween2(string strSource, string strStart, string strEnd)
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

        public static string trimFront(string strSource, string strStart)
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

        public static string trimFront(string strSource, string strStart, int noOfCharBefore)
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

    public class FileInfo
    {
        public string symbol = "";
        public string code = "";
        public string url  = "";
        public string type = "";
        public DateTime date = new DateTime(0);

        public FileInfo(string symbol, string code, string url, string type, DateTime date)
        {
            this.symbol = symbol;
            this.code = code;
            this.url = url;
            this.type = type;
            this.date = date;
        }
    }

}
