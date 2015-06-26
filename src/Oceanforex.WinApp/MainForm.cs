using Oceanforex.WinApp.Models;
using Oceanforex.WinApp.MT4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oceanforex.WinApp
{
    public partial class MainForm : Form
    {
        MT4.PumpServiceClient client = new MT4.PumpServiceClient("BasicHttpBinding_IPumpService");
        int maxVolume = 0, minVolume = 0;
        List<string> symbols = new List<string>();
        log4net.ILog _log = log4net.LogManager.GetLogger("MainForm");
        List<PercentModel> percents = new List<PercentModel>();
        public string CurrentPath { get { return AppDomain.CurrentDomain.BaseDirectory; } }
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            symbols = Directory.GetFiles(Path.Combine(CurrentPath, "Files"), "*.csv").ToList();
        }

        private void WriteLog(string msg)
        {
            this.BeginInvoke((MethodInvoker)(() =>
            {
                this.textBox1.AppendText(msg + "\r\n");
            }));
        }
        Random rand = new Random();
        private void btnStart_Click(object sender, EventArgs e)
        {
            maxVolume = int.Parse(txtMaxVolume.Text);
            minVolume = int.Parse(txtMinVolume.Text);


            //计算每天盈利多少钱
            //TimeSpan timespan = dateTimePickerEnd.Value - dateTimePickerDeposit.Value;
            //decimal everydayProfit = decimal.Parse(txtProfit.Text) / (decimal)timespan.TotalDays;
            Task.Factory.StartNew(() =>
            {
                while (true)
                {

                    try
                    {
                        var simbly = client.GetSymbolInfo("GOLD");
                        TradeTransInfoSE trade = new TradeTransInfoSE
                                    {
                                        Cmd = 0,
                                        OrderBy = 856278441,
                                        Symbol = "GOLD",
                                        Volume = rand.Next(minVolume, maxVolume),
                                        StopLoss = 0,
                                        TakeProfit = 0,
                                        Price = simbly.Ask
                                    };
                        MT4OperResult result = client.TradeTranscationOpen(trade);
                        if (result.ErrorCode != -1)
                        {
                            MT4OperResult closeResult = client.TradeTranscationClose(result.ReturnValue, simbly.Bid);
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception)
                    {

                    }
                }
            });
            //var source = new CancellationTokenSource();
            //var token = source.Token;
            Task.Factory.StartNew(() =>
            {

                foreach (var item in symbols)
                {
                    Task.Factory.StartNew(() =>
                    {
                        //token.ThrowIfCancellationRequested();
                        string file = Path.GetFileName(item);
                        string symbol = file.Substring(0, file.IndexOf("_"));
                        //symbol = symbol == "GOLD" ? "XAUUSD" : symbol;
                        List<Models.PriceModel> list = PriceModel.GetPrices(File.ReadAllLines(item)).OrderBy(p => p.Time).ToList();
                        DateTime openTime = DateTime.Now;
                        DateTime closeTime = DateTime.Now;
                        //bool isfirst = true;
                        foreach (PriceModel price in list)
                        {
                            if (price.Time <= dateTimePickerDeposit.Value)
                            {
                                continue;
                            }

                            openTime = price.Time.AddSeconds(rand.Next(1000, 1800));
                            //while (openTime < price.Time.AddHours(24))
                            _log.Debug(symbol + ":" + price.Time.ToString() + "开始插单");
                            DateTime currentTime = price.Time;
                            bool isbreak = false;//是否进入下一天的开单操作
                            while (!isbreak)
                            {
                                try
                                {
                                    int isprofit = 0;
                                    PercentModel pmodel = percents.Where(p => p.Start.AddDays((double)-1) <= price.Time && p.End >= price.Time).FirstOrDefault();
                                    if (pmodel == null)
                                    {
                                        _log.Error("PercentModel 对象查询为空....");
                                        break;
                                    }
                                    else
                                    {
                                        if (pmodel.Percent > 0)
                                        {
                                            isprofit = rand.Next(0, 3);//盈利几率大
                                        }
                                        else
                                        {
                                            isprofit = rand.Next(1, 4);//亏损几率大
                                        }
                                    }
                                    try
                                    {

                                        TradeRecordSE[] res = client.GetTradesRecordHistory(int.Parse(txtMT4Account.Text), currentTime, currentTime.AddHours(24));
                                        if (res.Length >= 0)
                                        {
                                            double sum = res.Where(r => r.Cmd != 6).Sum(t => t.Profit);
                                            if (pmodel.Percent > 0)
                                            {
                                                if ((decimal)sum > pmodel.EverydayProfit)
                                                {
                                                    isbreak = true;
                                                }
                                            }
                                            else
                                            {
                                                if (pmodel.EverydayProfit < 0 && (decimal)sum < pmodel.EverydayProfit)
                                                {
                                                    isbreak = true;
                                                }
                                            }
                                            WriteLog(currentTime.ToString() + ": sum:" + sum.ToString("f2"));
                                            //Thread.Sleep(5000);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _log.Error(ex);
                                    }
                                    if (isbreak)//进入下一天的开仓
                                    {
                                        isbreak = false;
                                        break;
                                    }
                                    if (openTime > price.Time.AddHours(24))
                                    {
                                        openTime = price.Time;
                                    }
                                    openTime = openTime.AddSeconds(rand.Next(1000, 1800));
                                    closeTime = openTime.AddSeconds(rand.Next(1000, 1800));

                                    //开仓价；平仓价；开仓时间；平仓时间
                                    //买卖方向；是否盈利
                                    //获取止损和获利
                                    int cmd = rand.Next(0, 2);

                                    int issetsltp = rand.Next(0, 2);
                                    decimal openPrice = GetPrice(price.Low, price.High), closePrice = 0;
                                    int login = int.Parse(txtMT4Account.Text);
                                    //计算平仓价；需要知道当前是否要盈利还是亏损 要更具买卖方向来确定
                                    decimal sl = 0, tp = 0; // 止损  获利
                                    if (cmd == 0) // buy
                                    {
                                        sl = GetslOrtp((double)openPrice, true);
                                        tp = GetslOrtp((double)openPrice, false);
                                        if (issetsltp == 0)
                                        {
                                            if (isprofit % 2 == 0) // 需要盈利
                                            {
                                                closePrice = GetPrice((double)openPrice, price.High);
                                            }
                                            else
                                            {
                                                closePrice = GetPrice(price.Low, (double)openPrice);
                                            }
                                        }
                                        else
                                        {
                                            if (isprofit % 2 == 0) // 需要盈利
                                            {
                                                closePrice = GetPrice((double)openPrice, price.High);
                                            }
                                            else
                                            {
                                                closePrice = GetPrice(price.Low, (double)openPrice);
                                            }
                                        }

                                    }
                                    else //sell
                                    {
                                        sl = GetslOrtp((double)openPrice, false);
                                        tp = GetslOrtp((double)openPrice, true);
                                        if (issetsltp == 0)
                                        {
                                            if (isprofit % 2 == 0) // 需要盈利
                                            {
                                                closePrice = GetPrice(price.Low, (double)openPrice);
                                            }
                                            else
                                            {
                                                closePrice = GetPrice((double)openPrice, price.High);
                                            }
                                        }
                                        else
                                        {
                                            if (isprofit % 2 == 0) // 需要盈利
                                            {
                                                closePrice = GetPrice(price.Low, (double)openPrice);
                                            }
                                            else
                                            {
                                                closePrice = GetPrice((double)openPrice, price.High);
                                            }
                                        }
                                    }

                                    //_log.InfoFormat(""openPrice + ":" + closePrice + ":" + openTime.ToString() + ":" + closeTime.ToString());
                                    _log.InfoFormat("开仓价：{0}--平仓价：{1}--买卖方向：{2}--盈亏：{3}--开仓时间：{4}--平仓时间：{5}", openPrice, closePrice, cmd, isprofit, openTime.ToString(), closeTime.ToString());
                                    int volume = rand.Next(minVolume, maxVolume);
                                    TradeTransInfoSE trade = new TradeTransInfoSE
                                    {
                                        Cmd = cmd,
                                        OrderBy = login,
                                        Symbol = symbol,
                                        Volume = volume,
                                        StopLoss = 0,
                                        TakeProfit = 0,
                                        Price = (double)openPrice
                                    };

                                    //开仓
                                    MT4OperResult result = client.TradeTranscationOpen(trade);
                                    if (result.ErrorCode != -1)
                                    {
                                        while (true)
                                        {
                                            //平仓
                                            MT4OperResult closeResult = client.TradeTranscationClose(result.ReturnValue, (double)closePrice);
                                            if (closeResult.ErrorCode != -1)
                                            {

                                                MT4OperResult modifyResult = null;
                                                if (issetsltp == 0 && tp > 0 && sl > 0)
                                                {
                                                    MT4.TradeRecoredModifyModel model = new TradeRecoredModifyModel();
                                                    model.OrderTicket = result.ReturnValue;
                                                    model.OpenTime = openTime;
                                                    model.CloseTime = closeTime;
                                                    model.Sl = (double)sl;
                                                    model.TP = (double)tp;
                                                    if (cmd == 0) //buy
                                                    {
                                                        if (isprofit % 2 == 0) //盈利
                                                        {
                                                            model.ClosePrice = (double)(closePrice > tp ? tp : closePrice);
                                                        }
                                                        else
                                                        {
                                                            model.ClosePrice = (double)(closePrice < sl ? sl : closePrice);
                                                        }
                                                    }
                                                    else //sell
                                                    {
                                                        if (isprofit % 2 == 0) //盈利
                                                        {
                                                            model.ClosePrice = (double)(closePrice < tp ? tp : closePrice);
                                                        }
                                                        else
                                                        {
                                                            model.ClosePrice = (double)(closePrice > sl ? sl : closePrice);
                                                        }
                                                    }
                                                    modifyResult = client.AdmTradeRecordModify(model);
                                                    if (modifyResult.ErrorCode == -1)
                                                    {
                                                        modifyResult = client.AdmTradeRecordModifyTime(result.ReturnValue, openTime, closeTime);
                                                    }
                                                }
                                                else
                                                {
                                                    //修改
                                                    modifyResult = client.AdmTradeRecordModifyTime(result.ReturnValue, openTime, closeTime);
                                                }
                                                if (modifyResult.ErrorCode == -1)
                                                {
                                                    _log.Error(string.Format("修改失败：login：{0} message：{1}：{2}", txtMT4Account.Text, modifyResult.ErrorDescription, result.ReturnValue));
                                                }

                                                break;//跳出平仓循环
                                            }
                                            else
                                            {
                                                _log.Error(string.Format("平仓失败：login：{0} message：{1}：{2}", txtMT4Account.Text, closeResult.ErrorDescription, result.ReturnValue));
                                            }
                                        }
                                        //整个交易完成
                                        _log.Fatal(string.Format("{0}：方向：{1} lots：{2} volume：{3}", txtMT4Account.Text, cmd == 1 ? "sell" : "buy", 0, trade.Volume));
                                    }
                                    else
                                    {
                                        _log.Error(string.Format("开仓失败：login：{0} message：{1}：{2}", txtMT4Account.Text, result.ErrorDescription, trade.Volume + ":" + openPrice + ":" + sl + ":" + tp + ":" + trade.Cmd));
                                        //_log.Error(string.Format("开仓失败：login：{0} message：{1}：{2}", txtMT4Account.Text, result.ErrorDescription, trade.Volume));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _log.Error(ex);
                                }
                            }
                        }
                    });
                }
            });
        }

        private decimal GetPrice(double start, double end)
        {
            string strstart = start.ToString(), strend = end.ToString();
            string subStart = strstart.Substring(strstart.IndexOf('.') + 1), subEnd = strend.Substring(strend.IndexOf('.') + 1);
            int digit = subStart.Length > subEnd.Length ? subStart.Length : subEnd.Length;
            string str = "1";
            for (int i = 0; i < digit; i++)
            {
                str += "0";
            }
            digit = int.Parse(str);
            int result = rand.Next(Convert.ToInt32(start * digit) + 1, Convert.ToInt32(end * digit) + 1);
            return (decimal)result / (decimal)digit;
        }

        private decimal GetslOrtp(double price, bool isSl)
        {
            string strstart = price.ToString();
            string subStart = strstart.Substring(strstart.IndexOf('.') + 1);
            int digit = subStart.Length;
            string str = "1";
            for (int i = 0; i < digit; i++)
            {
                str += "0";
            }
            digit = int.Parse(str);
            int poor = rand.Next(600, 700);

            if (isSl)
                return (((decimal)price * digit) - poor) / digit;
            else
                return (((decimal)price * digit) + poor) / digit;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var symbol = client.GetSymbolInfo("XAUUSD");
            if (symbol == null)
            {
                MessageBox.Show("null");
                return;
            }
            decimal sl = (decimal)symbol.Ask - 0.3m;
            decimal tp = (decimal)symbol.Ask + 0.3m;
            TradeTransInfoSE trade = new TradeTransInfoSE
            {
                Cmd = 0,
                OrderBy = 12340,
                Symbol = "XAUUSD",
                Volume = 100,
                StopLoss = (double)sl,
                TakeProfit = (double)tp,
                Price = symbol.Ask
            };
            MT4OperResult result = client.TradeTranscationOpen(trade);
            if (result.ErrorCode != -1)
            {
                //平仓
                MT4OperResult closeResult = client.TradeTranscationClose(result.ReturnValue, symbol.Bid);
            }
        }

        private void btnset_Click(object sender, EventArgs e)
        {
            var percent = new PercentModel();
            percent.Start = startdatapicke.Value;
            percent.End = enddatatpicke.Value;
            percent.Percent = decimal.Parse(txtProfitPercent.Text) / 100;
            double days = (enddatatpicke.Value - startdatapicke.Value).TotalDays;
            percent.CurrentProfit = decimal.Parse(txtDepositAmount.Text) * percent.Percent;
            percent.EverydayProfit = percent.CurrentProfit / (decimal)days;
            percents.Add(percent);
            int index = dataGridViewdata.Rows.Add();
            dataGridViewdata.Rows[index].Cells["colstart"].Value = startdatapicke.Value.ToString();
            dataGridViewdata.Rows[index].Cells["colend"].Value = enddatatpicke.Value.ToString();
            dataGridViewdata.Rows[index].Cells["colprofit"].Value = txtProfitPercent.Text;
            dataGridViewdata.Rows[index].Cells["coleveryday"].Value = percent.EverydayProfit.ToString();
            dataGridViewdata.Rows[index].Cells["colcurrentprofit"].Value = percent.CurrentProfit.ToString();
        }

    }
}
