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

using mshtml;

namespace StockPriceExtractor
{
    public partial class StockPriceExtractor : Form
    {
        string extractingSymbol = "";

        int totalStockNum = 0;
        int currStockNum = 0;

        Queue<string> stockAddressQueue = new Queue<string>();

        Regex regex = new Regex(@"^-?\d+(?:\.\d+)?");

        DataSet ds = new DataSet("New_DataSet");
        DataTable dt = new DataTable("New_DataTable");
        List<string> stockList = new List<string>();

        Dictionary<string, StockInfo> StockList = new Dictionary<string, StockInfo>();

        public StockPriceExtractor()
        {
            SetFeatureBrowserEmulation();
            InitializeComponent();

            LogTextbox.Text += "Loading...\n";
            ServicePointManager.DefaultConnectionLimit = 1000;
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.ScrollBarsEnabled = false;
            //webBrowser1.Navigate("http://www.shareinvestor.com/");

            string userName = "";
            string password = "";
            string hdr = "Authorization: Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + password)) + System.Environment.NewLine;
            webBrowser1.Navigate(String.Format("https://{0}:{1}@www.shareinvestor.com/user/do_login.html?use_https=0", userName, password), null, null, hdr);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            label1.Text = "OK";
            webBrowser1.Document.Window.ScrollTo(3000, 0);

            if (webBrowser1.DocumentText.Contains("Welcome,"))
            {
                LogTextbox.Text = "Login Success!\n\nPress READ to read Stock List from file.\nPress START to extract data of stocks in Stock List.\n";
                LogTextbox.SelectionStart = LogTextbox.Text.Length;
                LogTextbox.ScrollToCaret();
            }
            else
            {
                webBrowser1.Document.Window.ScrollTo(0, 420);
                LogTextbox.Text = "Welcome!\n";
                LogTextbox.Text += "Please log into Share Investor!\n";
                LogTextbox.SelectionStart = LogTextbox.Text.Length;
                LogTextbox.ScrollToCaret();
            }
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

        private void SaveButton_Click(object sender, EventArgs e)
        {
            saveFileDialog1.InitialDirectory = @".\";

            DialogResult result = saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK) // Test result.
            {
                string file = saveFileDialog1.FileName;
                try
                {
                    WriteStockToFile(file);
                }
                catch (IOException)
                {
                }
            }

        }

        private void WriteStockToFile(string filePath)
        {
            if (!System.IO.File.Exists(@filePath))
            {
                var myFile = System.IO.File.Create(@filePath);
                myFile.Close();
            }

            System.IO.File.WriteAllText(@filePath, stockTextbox.Text, Encoding.UTF8);
        }

        //############### PART 2 #################
        List<string> symbolList = new List<string>();
        Dictionary<string, int> retryDict = new Dictionary<string, int>();
        readonly object historicalDictLock = new object();
        public Dictionary<DateTime, Dictionary<string, Historical>> historicalDict = new Dictionary<DateTime, Dictionary<string, Historical>>();

        private void button1_Click(object sender, EventArgs e)
        {
            symbolList.Clear();
            historicalDict.Clear();
            currStockNum = 0;

            StringReader strReader = new StringReader(stockTextbox.Text);
            string str;

            for (;;)
            {
                str = strReader.ReadLine();
                if (str != null && str != "")
                    symbolList.Add(str);
                else
                    break;
            }
            totalStockNum = symbolList.Count;
            LogTextbox.Text += "Beginning to Extract Historical Prices for " + totalStockNum + " symbols.\n";
            LogTextbox.SelectionStart = LogTextbox.Text.Length;
            LogTextbox.ScrollToCaret();
            Task.Factory.StartNew(() => { ReadStockFromStockQueue2(); }); //ReadStockFromStockQueue2();
        }

        private void ReadStockFromStockQueue2()
        {
            //timer2.Enabled = true;
            foreach (string symbol in symbolList)
            {
                //Task.Factory.StartNew(() => { loadHistoricalPrice(symbol); });
                loadHistoricalPrice(symbol);
            }

            showResults();
            MessageBox.Show("Extract Completed!");
        }

        private void loadHistoricalPrice(string symbol)
        {
            //Invoke((MethodInvoker)delegate
            //{
            //    LogTextbox.Text += "Extracting " + symbol + "...\n";
            //    LogTextbox.SelectionStart = LogTextbox.Text.Length;
            //    LogTextbox.ScrollToCaret();
            //});
            try
            {
                Console.WriteLine("Extracting " + symbol + "....");
                WebProcessor webProcessor = new WebProcessor(this);

                string html = webProcessor.GetGeneratedHTML("http://www.shareinvestor.com/prices/historical_price.html#/?type=historical_price_by_stock&counter=" + symbol + ".SI&historical_view=daily");
                if (!html.Contains("No data available in table") && html != "")
                {
                    string table = getBetween2(html, "<table class=\"sic_table dataTable no-footer\" id=\"sic_historicalPriceTable\" role=\"grid\" cellspacing=\"1\">", "</table>");

                    if (table == "EMPTY")
                    {
                        Console.WriteLine(symbol + ": table = EMPTY");
                        return;
                    }

                    table = table.Substring(table.IndexOf("<tr") + 3);
                    table = table.Substring(table.IndexOf("<tr"));
                    table = table.Replace(System.Environment.NewLine, "");
                    table = table.Replace("  ", "");
                    table = table.Replace("\n", "");
                    table = table.Replace("class=\"sic_highlight\"", "");
                    table = table.Replace(" class=\"sic_up\"", "");
                    table = table.Replace(" class=\"sic_down\"", "");
                    table = table.Replace(" class=\"odd\" role=\"row\"", "");
                    table = table.Replace(" class=\"even\" role=\"row\"", "");
                    table = table.Replace(" >", ">");
                    string[] rows = table.Split(new string[] { "</tr><tr>" }, StringSplitOptions.None);

                    if (rows.Length < 0)
                    {
                        Console.WriteLine(symbol + ": Invalid row length = " + rows.Length);
                        return;
                    }

                    foreach (string row in rows)
                    {
                        string[] cells = row.Split(new string[] { "</td><td>" }, StringSplitOptions.None);
                        for (int i = 0; i < cells.Length; i++)
                        {
                            cells[i] = cells[i].Replace("<td>", "");
                            cells[i] = cells[i].Replace("</td>", "");
                            cells[i] = cells[i].Replace("<tr>", "");
                            cells[i] = cells[i].Replace("</tr></tbody>", "");
                        }

                        if (cells.Length < 13)
                        {
                            Console.WriteLine(symbol + ": Invalid cells length = " + cells.Length);
                            return;
                        }

                        Historical historical = new Historical();
                        historical.symbol = symbol;
                        DateTime dt;
                        DateTime.TryParseExact(cells[0], "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                        historical.date = dt;

                        historical.open = double.Parse(cells[1].Replace("-", "0").Replace(",", ""));
                        historical.high = double.Parse(cells[2].Replace("-", "0").Replace(",", ""));
                        historical.low = double.Parse(cells[3].Replace("-", "0").Replace(",", ""));
                        historical.close = double.Parse(cells[4].Replace("-", "0").Replace(",", ""));
                        historical.vwap = double.Parse(cells[5].Replace("-", "0").Replace(",", ""));
                        if (cells[6] == "-")
                            cells[6] = "0";
                        historical.chg = double.Parse(cells[6].Replace(",", "").Replace("+", ""));
                        if (cells[7] == "-")
                            cells[7] = "0";
                        historical.percentChg = double.Parse(cells[7].Replace(",", "").Replace("+", "").Replace("%", "").Replace("NaN", "0").Replace("Inf", "0"));
                        historical.volume = long.Parse(cells[8].Replace("-", "0").Replace(",", ""));
                        historical.adjustedClose = double.Parse(cells[9].Replace("-", "0").Replace(",", ""));
                        historical.adjustedVwap = double.Parse(cells[10].Replace("-", "0").Replace(",", ""));
                        historical.shortVolume = long.Parse(cells[11].Replace("-", "0").Replace(",", ""));
                        historical.shortValue = double.Parse(cells[12].Replace("-", "0").Replace(",", ""));
                        historical.shortPercentage = (double)historical.shortVolume / (double)historical.volume * 100;
                        historical.avgShortPrice = historical.shortValue / (double)historical.shortVolume;
                        lock (historicalDictLock)
                        {
                            if (historicalDict.ContainsKey(historical.date))
                            {
                                if (!historicalDict[historical.date].ContainsKey(symbol))
                                {
                                    historicalDict[historical.date].Add(symbol, historical);
                                }
                            }
                            else
                            {
                                Dictionary<string, Historical> subDict = new Dictionary<string, Historical>();
                                subDict.Add(symbol, historical);
                                historicalDict.Add(historical.date, subDict);
                            }
                        }
                    }
                    Console.WriteLine("Extract " + symbol + " Success! (" + (currStockNum + 1) + "/" + totalStockNum + ")");
                    
                }
                else
                {
                    Console.WriteLine("Extract " + symbol + " Fail! (" + (currStockNum + 1) + "/" + totalStockNum + ")");
                }

                currStockNum++;

                //Invoke((MethodInvoker)delegate
                //{
                //    LogTextbox.Text += "Extract Success! (" + currStockNum + "/" + totalStockNum + ")\n";
                //    LogTextbox.SelectionStart = LogTextbox.Text.Length;
                //    LogTextbox.ScrollToCaret();
                //});
                

                //if (currStockNum == totalStockNum)
                //{
                //    MessageBox.Show("Extract Completed!");
                //    showResults();
                //}
            }
            catch(Exception ex)
            {
                Console.WriteLine("loadHistoricalPrice() Error: " + ex.Message);
                showResults();
            }
        }

        public void showResults()
        {
            //lock (historicalDictLock)
            //{
            //    var strList = new List<string>();
            //    string str = ",";
            //    foreach (string symbol in symbolList)
            //    {
            //        str += symbol + ",";
            //    }
            //    strList.Add(str);

            //    foreach (DateTime dateTime in historicalDict.Keys)
            //    {
            //        string str2 = dateTime.ToString() + ",";
            //        foreach (string symbol in historicalDict[dateTime].Keys)
            //        {

            //                dataGridView1.Rows[row].Cells[symbol].Value = historicalDict[dateTime][symbol].adjustedClose;
            //        }
            //    }

            //    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\GenkCapital\Desktop\ScanResult.txt"))
            //    {
            //        foreach (string line in lines)
            //        {
            //            file.WriteLine(line);
            //        }
            //    }
            //}


            Invoke((MethodInvoker)delegate
            {
                //lock (historicalDictLock)
                //{
                ResultsDisplay resultDisplay = new ResultsDisplay(historicalDict, "adjustedClose", symbolList);
                resultDisplay.Show();
                //}
            });
        }

        // enable HTML5 (assuming we're running IE10+)
        static void SetFeatureBrowserEmulation()
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime)
                return;
            var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                appName, 10000, Microsoft.Win32.RegistryValueKind.DWord);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            errorStockTextBox.Text = "";
            StockList.Clear();
            stockAddressQueue.Clear();
            currStockNum = 0;
            if (stockAddressQueue.Count == 0)
            {
                StringReader strReader = new StringReader(stockTextbox.Text);
                string str;

                for (; ; )
                {
                    str = strReader.ReadLine();
                    if (str != null && str != "")
                        stockAddressQueue.Enqueue(str);
                    else
                        break;
                }

                totalStockNum = stockAddressQueue.Count;
            }

            ReadAnnualReportFromStockQueue();
        }

        private void ReadAnnualReportFromStockQueue()
        {
            if (stockAddressQueue.Count > 0)
            {
                extractingSymbol = stockAddressQueue.Dequeue();
                webBrowser1.Navigate("http://www.shareinvestor.com/fundamental/annual_reports.html?counter=" + extractingSymbol + ".SI&period=fy&cols=10");
            }
            else
            {
                LogTextbox.Text += "Annual Report Extraction Completed! \n";
                LogTextbox.SelectionStart = LogTextbox.Text.Length;
                LogTextbox.ScrollToCaret();
            }
        }

        private void stockTextbox_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public class WebProcessor
    {
        private string GeneratedSource { get; set; }
        private string URL { get; set; }
        private StockPriceExtractor mainForm;
        public WebProcessor(StockPriceExtractor callingForm)
        {
            mainForm = callingForm;
            GeneratedSource = "";
            URL = "";
        }

        public string GetGeneratedHTML(string url)
        {
            URL = url;

            Thread t = new Thread(new ThreadStart(WebBrowserThread));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join(TimeSpan.FromSeconds(20));

            return GeneratedSource;
        }

        private void WebBrowserThread()
        {
            try
            {
                WebBrowser wb = new WebBrowser();
                wb.Navigate(URL);

                //wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(wb_DocumentCompleted);
                int i = 0;

                Console.WriteLine("While Loop");
                //while (wb.ReadyState != WebBrowserReadyState.Complete && i < 200000)
                while (wb.Document == null || wb.Document.Body == null || wb.Document.Body.InnerHtml == null || !wb.Document.Body.InnerHtml.Contains("Notes:"))
                {
                    Application.DoEvents();
                    //i++;
                }
                Console.WriteLine("Done");
                //Added this line, because the final HTML takes a while to show up
                GeneratedSource = wb.Document.Body.InnerHtml;

                wb.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine("WebBrowserThread() Error: " + e.Message);
                mainForm.showResults();
            }
        }

        private void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser wb = (WebBrowser)sender;
            GeneratedSource = wb.Document.Body.InnerHtml;
        }
    } 

    public class StockInfo
    {
        public string symbol = "";
        public string stockName = "";
        public double lastPrice = 0;
        public string dateUpdate = "";
        public List<double> netEarnings = new List<double>();
        public List<double> revenue = new List<double>();
        public List<double> shareHolderEquity = new List<double>();
        public List<double> ltLiabilities = new List<double>();
        public List<double> eps = new List<double>();
        public List<double> cash = new List<double>();
        public List<double> margin = new List<double>();
        public List<double> roc = new List<double>();
        public double debtToEquity = 0;


        public double[] avgRoc = new double[4];
        public double[] equityGrowth = new double[4];
        public double[] epsGrowth = new double[4];
        public double[] revenueGrowth = new double[4];
        public double[] cashGrowth = new double[4];

        public double rule1Growth = 0;
        public double rule1PE = 0;
        public double epsIn5Yrs = 0;
        public double futurePriceIn5yrs = 0;
        public double stickerPrice = 0;
        public double priceToPurchase = 0;
        public double priceToPurchaseOverLast = 0;

        public int score = 0;

        //Jinjian's Indicators
        public List<double> freeCashFlow = new List<double>(); 
        public double profitMargin = 0;
        public double priceToNTA = 0;
        public double interestCoverage = 0;
        public double noOfShares = 0;
        public double shortTermDebt = 0;
        public double longTermDebt = 0;
        public double dividendYieldExclSpecial = 0;
        public double priceToCashMinusLongTermDebt = 0;
        public double priceToCashMinusTotalDebt = 0;
        public double jjscore = 0;
    }

    public class Historical
    {
        public DateTime date = new DateTime(0);
        public string symbol="";
        public long volume=0, shortVolume=0;
        public double open = 0, high = 0, low = 0, close = 0, vwap = 0, chg = 0, percentChg = 0, adjustedVwap = 0, adjustedClose = 0, shortValue = 0, avgShortPrice = 0, shortPercentage = 0;
    }

    public class CookieAwareWebClient2 : WebClient
    {
        public CookieAwareWebClient2()
            : this(new CookieContainer())
        { }
        public CookieAwareWebClient2(CookieContainer c)
        {
            this.CookieContainer = c;
        }
        public CookieContainer CookieContainer { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            var castRequest = request as HttpWebRequest;
            if (castRequest != null)
            {
                castRequest.CookieContainer = this.CookieContainer;
            }

            return request;
        }
    }

}

