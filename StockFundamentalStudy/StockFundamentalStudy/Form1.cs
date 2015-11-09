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

namespace StockFundamentalStudy
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

        string FindNetEarnings = " Net Earnings</STRONG> </TH>";
        string FindRevenue = " Revenue</STRONG> </TH>";
        string FindShareHolderEquity = " Shareholders' Equity</STRONG> </TH>";
        string FindLTLiabilities = " Long Term Liabilities</STRONG> </TH>";
        string FindEPS = "(EPS)</SPAN></STRONG> <I>- Historical</I>";
        string FindCash = "Cash And Cash Equivalents At End</STRONG> </TH>";
        string FindMargin = "(Net Earnings/Revenue) </TH>";
        string FindDebtToEquity = "Debt To Equity</SPAN></STRONG><BR> ((Long Term Debt + Short Term Debt)";
        string FindDateUpdate = "Full Year<BR>";
        string FindFreeCashFlow = "Free Cash Flow</STRONG><BR>";
        string FindProfitMargin = "Profit(Earnings) Margin</SPAN></STRONG> [%]<BR> (Net Earnings/Revenue)";
        string FindPriceToNTA = "Price/Adjusted NTA) </TH>\r\n";
        string FindInterestCoverage = "Interest Coverage</STRONG><BR>";
        string FindNoOfShares = "No. Of Ordinary Shares Issued ('000)</STRONG> </TH>";
        string FindShortTermDebt = "Short Term Debt (Include Current Portion of Long Term Debt)</STRONG>";
        string FindLongTermDebt = "Long Term Debt</STRONG>";
        string FindDividendYield = "Dividend \r\n      Yield</SPAN></STRONG> <I>- Adjusted";

        Queue<string> stockAddressQueue = new Queue<string>();
        Regex regex = new Regex(@"^-?\d+(?:\.\d+)?");

        //string contents = "";
        string tempString = "";
        string tempString2 = "";

        DataSet ds = new DataSet("New_DataSet");
        DataTable dt = new DataTable("New_DataTable");
        List<string> stockList = new List<string>();

        Dictionary<string, StockInfo> StockList = new Dictionary<string, StockInfo>();

        public Form1()
        {
            InitializeComponent();
        }

        void Form1_Load(object sender, EventArgs e)
        {
            string userName = "";
            string password = "";
            string hdr = "Authorization: Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + password)) + System.Environment.NewLine;

            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.ScrollBarsEnabled = false;
            webBrowser1.Navigate(String.Format("https://{0}:{1}@www.shareinvestor.com/user/do_login.html?use_https=0", userName, password), null, null, hdr);

            replaceText("Welcome!\nPlease log into Share Investor!");
            Task.Factory.StartNew(InitSetup);
        }

        void Start_Click(object sender, EventArgs e)
        {
            errorStockTextBox.Text = "";
            StockList.Clear();
            stockAddressQueue.Clear();
            currStockNum = 0;
            if (stockAddressQueue.Count == 0)
            {
                StringReader strReader = new StringReader(stockTextbox.Text);
                string str;

                for (;;)
                {
                    str = strReader.ReadLine();
                    if (str != null && str != "")
                        stockAddressQueue.Enqueue(str);
                    else
                        break;
                }

                totalStockNum = stockAddressQueue.Count;
            }

            Task.Factory.StartNew(ReadStockFromStockQueue); //start ReadStockFromStockQueue from another thread
        }

        void InitSetup()
        {
            while (myState != MyState.READY)
            {
                Thread.Sleep(2000);

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
                                //cookies = GetCookies();
                            }
                            break;
                    }
                });
            }
        }

        void ReadStockFromStockQueue()
        {
            if (stockAddressQueue.Count > 0)
            {
                currStockCode = stockAddressQueue.Dequeue();
                int counter = 0;
                bool extractSuccess = false;
                
                currStockNum++;
                printText("Extracting " + currStockCode + "... (" + currStockNum + "/" + totalStockNum + ")");
                webBrowser1.DocumentText = "";
                webBrowser1.Navigate("http://www.shareinvestor.com/fundamental/financials.html?counter=" + currStockCode + ".SI&period=fy&cols=10");
                
                while (!extractSuccess && counter <= 3)
                {
                    Thread.Sleep(2000);

                    Invoke((MethodInvoker)delegate
                    {
                        counter++;
                        webBrowser1.Document.Window.ScrollTo(30, 470);
                        extractSuccess = extractInfo(webBrowser1.DocumentText);
                    });
                }

                if (extractSuccess)
                    printText("Extract Success!");
                else
                {
                    Invoke((MethodInvoker)delegate { errorStockTextBox.Text += currStockCode + "\n"; });
                    printText("Extract Fail!");
                }

                ReadStockFromStockQueue();
            }
            else
                calculation();
        }

        void StopButton_Click(object sender, EventArgs e)
        {
            calculation();
        }

        //########################## EXTRACT METHODS ##########################

        void calculation()
        {
            printText("Doing Calculation...");
            
            Invoke((MethodInvoker)delegate
            {
                StudyResult studyResult = new StudyResult(StockList);
                studyResult.Show();
            });

            printText("Calculation Completed!");
        }

        bool extractInfo(string contents)
        {
            StockInfo stockInfo = new StockInfo();
            double tempDouble = 0;
            bool parseSuccess = false;

            stockInfo.symbol = currStockCode;
            //printText("Extracting: " + currStockCode);

            //Get stockName
            tempString = getBetween2(contents, "<TITLE>", "</TITLE>");
            string[] tempName = tempString.Split('-');
            stockInfo.stockName = Regex.Match(tempName[0], @"[0-9a-zA-Z\s^.]+").Value;
            stockInfo.stockName = stockInfo.stockName.Replace(" ", "");

            //Get symbol
            string tempSymbol = getBetween2(tempName[1], "(", ")").Replace(".SI", "");
            if (tempSymbol != currStockCode)
                return false;

            //Get lastPrice
            tempString = getBetween2(contents, "Last (SGD):", "</TD>");
            tempString = getBetween2(tempString, "<STRONG>", "</STRONG>");

            parseSuccess = double.TryParse(tempString, NumberStyles.Any, CultureInfo.InvariantCulture, out tempDouble);
            if (parseSuccess)
                stockInfo.lastPrice = tempDouble;
            else
                return false;

            //Get dateUpdate
            tempString = getBetween2(contents, FindDateUpdate, "</TH>");
            stockInfo.dateUpdate = tempString;

            stockInfo.netEarnings = extractList(contents, FindNetEarnings);
            stockInfo.revenue = extractList(contents, FindRevenue);
            stockInfo.shareHolderEquity = extractList(contents, FindShareHolderEquity);
            stockInfo.ltLiabilities = extractList(contents, FindLTLiabilities);
            stockInfo.eps = extractList(contents, FindEPS);
            stockInfo.cash = extractList(contents, FindCash);
            stockInfo.margin = extractList(contents, FindMargin);
            stockInfo.debtToEquity = extractDouble(contents, FindDebtToEquity);
            stockInfo.freeCashFlow = extractList(contents, FindFreeCashFlow);
            stockInfo.profitMargin = extractDouble(contents, FindProfitMargin);
            stockInfo.priceToNTA = extractDouble(contents, FindPriceToNTA);
            stockInfo.interestCoverage = extractDouble(contents, FindInterestCoverage);
            stockInfo.noOfShares = extractDouble(contents, FindNoOfShares);
            stockInfo.shortTermDebt = extractDouble0(contents, FindShortTermDebt);
            stockInfo.longTermDebt = extractDouble0(contents, FindLongTermDebt);
            stockInfo.dividendYieldExclSpecial = extractDouble(contents, FindDividendYield);

            if (!StockList.ContainsKey(currStockCode))
                StockList.Add(currStockCode, stockInfo);
            else
                StockList[currStockCode] = stockInfo;

            return true;
        }

        List<double> extractList(string contents, string findString)
        {
            bool parseSuccess = false;
            double tempDouble;
            var tempList = new List<double>();

            tempString = getBetween(contents, findString, "</TR>");

            for (; ; )
            {
                tempString2 = getBetween2(tempString, "<TD>", "</TD>");

                if (tempString2 == "EMPTY")
                    break;

                if (tempString2.Length < 15)
                {
                    parseSuccess = double.TryParse(tempString2, NumberStyles.Any, CultureInfo.InvariantCulture, out tempDouble);
                    if (parseSuccess)
                        tempList.Add(tempDouble);
                    else
                        tempList.Add(-99999999);
                }
                tempString = trimFront(tempString, tempString2);
            }

            return tempList;
        }

        double extractDouble(string contents, string findString)
        {
            bool parseSuccess = false;
            double tempDouble = -99999999;

            tempString = getBetween(contents, findString, "</TR>");

            for (;;)
            {
                tempString2 = getBetween2(tempString, "<TD>", "</TD>");

                if (tempString2 == "EMPTY")
                    break;

                if (tempString2.Length < 15)
                {
                    parseSuccess = double.TryParse(tempString2, NumberStyles.Any, CultureInfo.InvariantCulture, out tempDouble);
                    if (!parseSuccess)
                        tempDouble = -99999999;

                    break;
                }
                tempString = trimFront(tempString, tempString2);
            }

            return tempDouble;
        }

        double extractDouble0(string contents, string findString)
        {
            bool parseSuccess = false;
            double tempDouble = 0;

            tempString = getBetween(contents, findString, "</TR>");

            for (; ; )
            {
                tempString2 = getBetween2(tempString, "<TD>", "</TD>");

                if (tempString2 == "EMPTY")
                    break;

                if (tempString2.Length < 15)
                {
                    parseSuccess = double.TryParse(tempString2, NumberStyles.Any, CultureInfo.InvariantCulture, out tempDouble);
                    if (!parseSuccess)
                        tempDouble = 0;

                    break;
                }
                tempString = trimFront(tempString, tempString2);
            }

            return tempDouble;
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

        void SaveButton_Click(object sender, EventArgs e)
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
                {}
            }
        }

        void WriteStockToFile(string filePath)
        {
            if (!System.IO.File.Exists(@filePath))
            {
                var myFile = System.IO.File.Create(@filePath);
                myFile.Close();
            }

            System.IO.File.WriteAllText(@filePath, stockTextbox.Text, Encoding.UTF8);
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

        void replaceText(string text)
        {
            Invoke((MethodInvoker)delegate
            {
                LogTextbox.Text = text + "\n";
                LogTextbox.SelectionStart = LogTextbox.Text.Length;
                LogTextbox.ScrollToCaret();
            });
        }

        //########################### GET STOCK SYMBOLS ###########################

        void ReadButton_Click(object sender, EventArgs e)
        {
            getStockSymbols();
            //openFileDialog1.InitialDirectory = @".\";
            //DialogResult result = openFileDialog1.ShowDialog();

            //if (result == DialogResult.OK) // Test result.
            //{
            //    string file = openFileDialog1.FileName;
            //    try
            //    {
            //        ReadInstFromFile(file);
            //    }
            //    catch (IOException)
            //    {
            //    }
            //}
        }

        public bool getStockSymbols()
        {
            stockList = new List<string>();
            String[] options = new String[0];
            using (var webClient = new CookieAwareWebClient2())
            {
                var loginAddress = "https://www.shareinvestor.com/user/do_login.html?use_https=0";
                Uri uri = new Uri(loginAddress);
                var loginData = new NameValueCollection
                {
                  { "name", "koh_jiefeng" },
                  { "password", "abcd1234" }
                };
                try
                {
                    //webClient.UploadValues(System.Web.HttpUtility.UrlEncode(loginAddress), loginData);
                    byte[] responseArray = webClient.UploadValues(loginAddress, loginData);
                    //Console.WriteLine("\nResponse received was :\n{0}", Encoding.ASCII.GetString(responseArray));
                    //webClient.UseDefaultCredentials = true;
                    //webClient.Credentials = new NetworkCredential("koh_jiefeng", "abcd1234");
                    //webClient.Login(loginAddress, loginData);
                    Uri uri01 = new Uri("http://www.shareinvestor.com/prices/historical_price.html#/?counter=C29.SI&historical_view=daily&page=-1");
                    //string source = webClient.DownloadString(uri01);
                    //webClient.DownloadFile(uri01, "c:\\temp\\dump.txt" );
                    string source2;
                    webClient.DownloadStringCompleted += (sender, e) =>
                    {
                        source2 = e.Result;
                        //Console.WriteLine(source2);
                    };
                    //string source5 = webClient.DownloadString(uri01);
                    //Console.WriteLine(source5);
                    //webClient.DownloadStringAsync(uri01);
                    webClient.DownloadStringAsync(uri01);
                    //string dropdownlist = Regex.Match(source, @"\<select class=""validate-selection\b[^>]*\>\s*(?<Title>[\s\S]*?)\</select\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                    //dropdownlist = dropdownlist.Replace("  ", "");
                    //dropdownlist = dropdownlist.Replace(System.Environment.NewLine, "");
                    //dropdownlist = dropdownlist.Replace("  ", "");
                    //dropdownlist = dropdownlist.Replace("option value=\"", "");
                    //dropdownlist = dropdownlist.Replace("/option", "");
                    //dropdownlist = dropdownlist.Replace(" selected", "");
                    //dropdownlist = dropdownlist.Replace("\"", "");
                    //dropdownlist = dropdownlist.Replace("\n", "");
                    //dropdownlist = dropdownlist.Replace("<>", "");
                    //dropdownlist = dropdownlist.Replace(".SI", "");
                    //dropdownlist = dropdownlist.Replace("-- Select Counter --", "");
                    //options = dropdownlist.Split(new string[] { "<" }, StringSplitOptions.None);

                    //foreach (string option in options)
                    //{
                    //    if (option == "")
                    //        continue;
                    //    stockTextbox.Text += option.Split('>')[0] + "\n";
                    //    stockList.Add(option.Split('>')[0]);
                    //}

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
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

