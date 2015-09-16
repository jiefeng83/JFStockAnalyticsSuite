using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockFundamentalStudy
{
    public partial class ResultsDisplay : Form
    {
        Dictionary<DateTime, Dictionary<string, Historical>> historicalDict;
        List<string> symbolList;
        string column;

        public ResultsDisplay(Dictionary<DateTime, Dictionary<string, Historical>> historicalDict, string column, List<string> symbolList)
        {
            this.column = column;
            this.historicalDict = historicalDict;
            this.symbolList = symbolList;
            InitializeComponent();
        }

        private void ResultsDisplay_Load(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Columns.Add("Date", "Date");

                foreach (string symbol in symbolList)
                {
                    dataGridView1.Columns.Add(symbol, symbol);
                }
                int firstRow = dataGridView1.Rows.Add();
                foreach (string symbol in symbolList)
                {
                    dataGridView1.Rows[firstRow].Cells[symbol].Value = symbol;
                }
                foreach (DateTime dateTime in historicalDict.Keys)
                {
                    int row = dataGridView1.Rows.Add();
                    foreach (string symbol in historicalDict[dateTime].Keys)
                    {
                        dataGridView1.Rows[row].Cells["Date"].Value = historicalDict[dateTime][symbol].date.ToString("dd-MM-yyyy");
                        if (column == "volume")
                            dataGridView1.Rows[row].Cells[symbol].Value = historicalDict[dateTime][symbol].volume;
                        else if (column == "adjustedClose")
                            dataGridView1.Rows[row].Cells[symbol].Value = historicalDict[dateTime][symbol].adjustedClose;
                        else if (column == "shortVolume")
                            dataGridView1.Rows[row].Cells[symbol].Value = historicalDict[dateTime][symbol].shortVolume;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


    }
}
