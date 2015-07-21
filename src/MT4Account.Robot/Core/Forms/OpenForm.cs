using Newtonsoft.Json;
using MT4Account.Robot.Core.Models;
using MT4Account.Robot.Core.UserControls;
using MT4Account.Robot.PumpingService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MT4Account.Robot
{
    public partial class OpenForm : Form
    {
        private List<string> symbols = null;
        private List<Core.Models.TxtParamClass> accounts = null;
        private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(OpenForm));
        //private Panel panel;
        Loading loading = null;
        public OpenForm(List<string> symbols, List<Core.Models.TxtParamClass> openAccount)
        {
            InitializeComponent();
            if (symbols == null)
            {
                this.symbols = new List<string>();
            }
            else
            {
                this.symbols = symbols;
            }
            if (openAccount == null)
            {
                accounts = new List<Core.Models.TxtParamClass>();
            }
            else
            {
                accounts = openAccount;
            }
        }
        private void OpenForm_Load(object sender, EventArgs e)
        {
            Display(accounts, symbols);
            //创建加载框
            loading = new Loading();
            loading.Showed += loading_Showed;
            loading.Hided += loading_Hided;
            this.Controls.Add(loading);
        }

        void loading_Hided()
        {
            this.groupBox1.Enabled = true;
            groupBox2.Enabled = true;
        }

        void loading_Showed()
        {
            this.groupBox1.Enabled = false;
            groupBox2.Enabled = false;
        }

        private void dataGridViewOpenData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4 && dataGridViewOpenData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "移除")
            {
                dataGridViewOpenData.Rows.RemoveAt(e.RowIndex);
            }
        }


        private async void btnOpen_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtVlome.Text) || comCmd.SelectedItem == null || cmbSymbol.SelectedItem == null)
            {
                MessageBox.Show("请填写完整开仓信息");
            }
            else
            {
                string cmd = comCmd.SelectedItem.ToString().Trim();
                int intcmd = 0, volume = 0;
                double price = 0;

                List<TradeOpenModel> trades = new List<TradeOpenModel>();
                SymbolInfoSE symbol = Core.CustomService.Service.GetSymbolInfo(cmbSymbol.SelectedItem.ToString());
                if (symbol == null)
                {
                    MessageBox.Show("获取Symbol信息失败，请重试！");
                    return;
                }
                //转化买卖方向

                if (!int.TryParse(txtVlome.Text, out volume))
                {
                    MessageBox.Show("请填写正确的手数");
                    return;
                }
                if (cmd == "sell")
                {
                    price = symbol.Bid;
                    intcmd = 1;
                }
                else
                {
                    price = symbol.Ask;
                }

                foreach (DataGridViewRow row in dataGridViewOpenData.Rows)
                {
                    int loginid = Convert.ToInt32(row.Cells["LoginId"].Value);
                    TradeOpenModel trade = new TradeOpenModel();
                    trade.Symbol = cmbSymbol.SelectedItem.ToString().ToUpper();
                    trade.Cmd = intcmd;
                    trade.StopLoss = 0;
                    trade.TakeProfit = 0;
                    trade.OrderBy = loginid;
                    trade.Volume = volume * 100;
                    trade.Order = 0;
                    trade.IeDeviation = 0;
                    trade.Expiration = DateTime.Now;
                    trade.Price = price;
                    trades.Add(trade);
                }
                loading.ShowLoading();
                TradeOpenRsponseModel[] responseModels = await Core.CustomService.Service.TradeOpenAllAsync(trades.ToArray());
                loading.HideLoading();
                StringBuilder sbMessage = new StringBuilder();
                foreach (TradeOpenRsponseModel response in responseModels)
                {
                    if (response.ErrorCode == -1)
                    {
                        _log.Fatal(JsonConvert.SerializeObject(response));
                        sbMessage.AppendFormat("Account :{0} Message:{1}", response.OpenModel.OrderBy, response.ErrorDescription);
                        sbMessage.AppendLine();
                        continue;
                    }
                    else
                    {
                        _log.Info(JsonConvert.SerializeObject(response));
                    }
                }
                if (sbMessage.Length == 0)
                {
                    MessageBox.Show("批量开仓成功！");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("开仓失败账户：\r\b" + sbMessage.ToString());
                }
            }
        }

        private void Display(List<TxtParamClass> listParams, List<string> syms)
        {
            Task.Factory.StartNew(() =>
            {
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    dataGridViewOpenData.Rows.Clear();
                    if (listParams != null)
                    {
                        for (int i = 0; i < listParams.Count(); i++)
                        {
                            dataGridViewOpenData.Rows.Add();
                            dataGridViewOpenData.Rows[i].Cells["ColName"].Value = listParams[i].Name;
                            dataGridViewOpenData.Rows[i].Cells["LoginId"].Value = listParams[i].Login.ToString();
                            dataGridViewOpenData.Rows[i].Cells["StartAmount"].Value = listParams[i].Amount;
                            dataGridViewOpenData.Rows[i].Cells["Ratio"].Value = listParams[i].Ratio;
                        }
                    }
                    if (symbols.Count > 0)
                    {
                        cmbSymbol.Items.AddRange(syms.ToArray());
                    }
                }));

            });


        }
    }
}
