using Newtonsoft.Json;
using MT4Account.Robot.Core;
using MT4Account.Robot.Core.Helpers;
using MT4Account.Robot.Core.Models;
using MT4Account.Robot.Core.UserControls;
using MT4Account.Robot.PumpingService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MT4Account.Robot
{
    public partial class MainForm : Form
    {
        private static readonly string getDataUrl = ConfigurationManager.AppSettings.Get("GetDataUrl");
        private static readonly string saveDataUrl = ConfigurationManager.AppSettings.Get("SaveDataUrl");
        private static readonly string monitorSettingPath = ConfigurationManager.AppSettings["setting"].ToString();
        private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(MainForm));
        Loading loading = null;

        private static List<string> symbols = new List<string>();

        private List<TxtParamClass> list = null;

        private TradeRecordSE[] copyRecords = null;
        private int copyAccount = 0;
        /// <summary>
        /// 用户组
        /// </summary>
        List<string> groups = new List<string>();

        internal double bid = 0;
        internal double ask = 0;
        ///用来处理价格线程安全队列
        private ConcurrentQueue<double> priceList = new ConcurrentQueue<double>();
        /// <summary>
        /// 设置监听参数队列
        /// </summary>
        ConcurrentQueue<MonitorSetting> queueSetting = new ConcurrentQueue<MonitorSetting>();
        private bool isMon = false;
        /// <summary>
        /// 监听的订单
        /// </summary>
        private List<int> listMonitorOrders = new List<int>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void toolImportFinoData_Click(object sender, EventArgs e)
        {
        }
        void loading_Hided()
        {
            foreach (Control item in this.Controls)
            {
                item.Enabled = true;
            }
        }

        void loading_Showed()
        {
            foreach (Control item in this.Controls)
            {
                item.Enabled = false;
            }
        }


        private void toolFileImportData_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                loading.ShowLoading();
                BindData4File(ofd.FileName);
            }
        }
        private void toolOpenAll_Click(object sender, EventArgs e)
        {
            List<TxtParamClass> chkAccounts = new List<TxtParamClass>();
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                if (Convert.ToBoolean(row.Cells["chkCol"].Value))
                {
                    chkAccounts.Add(list.Where(a => a.Login == int.Parse(row.Cells["LoginId"].Value.ToString())).FirstOrDefault());
                }
            }
            OpenForm form = new OpenForm(symbols, chkAccounts);
            form.ShowDialog();
        }

        private void toolModifyOrder_Click(object sender, EventArgs e)
        {
            int account = 0;
            string name = string.Empty;
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value))
                {
                    account = int.Parse(row.Cells["LoginId"].Value.ToString());
                    name = row.Cells["ColName"].Value.ToString();
                    break;
                }
            }
            Core.Forms.ModifyOrder modify = new Core.Forms.ModifyOrder(account, name);
            modify.ShowDialog();
        }


        private async void MainForm_Load(object sender, EventArgs e)
        {
            dataGridViewData.AutoGenerateColumns = false;
            //创建加载框
            loading = new Loading();
            loading.Showed += loading_Showed;
            loading.Hided += loading_Hided;
            this.Controls.Add(loading);
            //加载所有产品
            PumpingService.SymbolInfoSE[] array = await Core.CustomService.Service.GetAllSymbolInfoAsync();
            symbols.AddRange(array.Select(ee => ee.Symbol));

            StartSymbolUpdate("GOLD");
            TradingHelper.MainForm = this;
            TradingHelper.WriteLogs = WriteLogs;
        }

        private void dataGridViewData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex > -1)
            {
                dataGridViewData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = !Convert.ToBoolean(dataGridViewData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            }
        }
        #region Core
        private List<TradeCloseModel> ConvertToCloseModel(List<TradeRecordSE> records)
        {
            List<TradeCloseModel> closes = new List<TradeCloseModel>();
            foreach (var record in records)
            {
                closes.Add(new TradeCloseModel() { Account = record.Login, OrderTicket = record.OrderId, Price = record.ClosePrice });
            }
            return closes;
        }

        private void BindData4File(string fileName)
        {
            loading.ShowLoading();
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                try
                {
                    string[] array = File.ReadAllLines(fileName);
                    if (list == null)
                    {
                        list = new List<TxtParamClass>();
                    }

                    foreach (string str in array)
                    {
                        UserRecordSE user = CustomService.Service.GetUserRecordsByLogin(int.Parse(str));
                        int count = 0;
                        while (string.IsNullOrEmpty(user.Name))
                        {
                            count++;
                            if (count>5)
                            {
                                break;
                            }
                            user = CustomService.Service.GetUserRecordsByLogin(int.Parse(str));
                        }
                        if (user.Group == "oceanforex")
                        {
                            var model = new TxtParamClass()
                            {
                                Login = user.Login,
                                Name = user.Name,
                                MemberId = user.Login,
                                Amount = user.Balance,
                                Ratio = 1,
                                StartTime = user.Regdate,
                                Groups = new List<string>() { user.Group }
                            };
                            list.Add(model);
                        }
                    }

                    this.BeginInvoke((MethodInvoker)(() =>
                    {
                        Display(list);
                        loading.HideLoading();
                    }));
                }
                catch (Exception ee)
                {
                    _log.Error(ee);
                }
            });
        }

        private void BindData4Tools()
        {
            loading.ShowLoading();
            ThreadPool.QueueUserWorkItem((obj) =>
            {


                try
                {
                    WebClient webClient = new WebClient();
                    webClient.Proxy = null;
                    webClient.Encoding = Encoding.UTF8;

                    //string result = webClient.DownloadString(getDataUrl);
                    webClient.DownloadStringAsync(new Uri(getDataUrl));
                    webClient.DownloadStringCompleted += (o, e) =>
                    {
                        File.WriteAllText("log.txt", e.Result);
                        list = JsonConvert.DeserializeObject<List<TxtParamClass>>(e.Result);
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            Display(list);
                            loading.HideLoading();
                        }));
                    };

                }
                catch (Exception ee)
                {
                    _log.Error(ee);
                }
            });
        }

        private void Display(List<TxtParamClass> listParams)
        {
            this.BeginInvoke((MethodInvoker)(() =>
            {
                dataGridViewData.Rows.Clear();
                if (listParams != null)
                {
                    for (int i = 0; i < listParams.Count(); i++)
                    {
                        dataGridViewData.Rows.Add();
                        dataGridViewData.Rows[i].Tag = listParams[i].MemberId;
                        dataGridViewData.Rows[i].Cells["ColName"].Value = listParams[i].Name;
                        dataGridViewData.Rows[i].Cells["LoginId"].Value = listParams[i].Login.ToString();
                        dataGridViewData.Rows[i].Cells["StartDateTime"].Value = listParams[i].StartTime;
                        dataGridViewData.Rows[i].Cells["EndDateTime"].Value = listParams[i].EndTime;
                        dataGridViewData.Rows[i].Cells["StartAmount"].Value = listParams[i].Amount;
                        dataGridViewData.Rows[i].Cells["Ratio"].Value = listParams[i].Ratio;
                        dataGridViewData.Rows[i].Cells["Group"].Value = listParams[i].Groups.FirstOrDefault();
                        if (listParams[i].Groups.Count > 0)
                        {
                            groups.AddRange(listParams[i].Groups.ToArray());

                        }
                    }
                    //显示用户所有组
                    var g = groups.Distinct();
                    toolcmboxgroup.Items.Clear();
                    toolcmboxgroup.Items.AddRange(g.ToArray());
                    foreach (DataGridViewRow row in dataGridViewData.Rows)
                    {
                        ((DataGridViewComboBoxCell)row.Cells["Group"]).Items.Clear();
                        foreach (var item in groups.Distinct())
                        {
                            ((DataGridViewComboBoxCell)row.Cells["Group"]).Items.Add(item);
                        }
                    }
                    switch (this.WindowState)
                    {
                        case FormWindowState.Maximized:
                            this.WindowState = FormWindowState.Normal;
                            this.WindowState = FormWindowState.Maximized;
                            break;
                        case FormWindowState.Minimized:
                            break;
                        case FormWindowState.Normal:
                            this.WindowState = FormWindowState.Maximized;
                            break;
                        default:
                            break;
                    }
                    Refresh();
                }
            }));

        }

        /// <summary>
        /// 更新Symbol报价
        /// </summary>
        /// <param name="symbol"></param>
        private void StartSymbolUpdate(string symbol)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        SymbolInfoSE model = Core.CustomService.Service.GetSymbolInfo(symbol);
                        if (model == null)
                        {
                            continue;
                        }
                        this.bid = model.Bid;
                        this.ask = model.Ask;
                        if (priceList.Count > 10)
                        {
                            priceList = new ConcurrentQueue<double>();
                        }
                        if (isMon == true)
                        {
                            //priceList.Enqueue((bid + ask) / 2);
                            if (bid > 1000 && ((decimal)bid - (decimal)ask) <= (decimal)0.5)
                            {
                                priceList.Enqueue(bid);
                            }
                        }
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            this.Text = string.Format("symbol:{0} bid:{1} ask:{2} update:{3} queue:{4}", symbol, bid.ToString(), ask.ToString(), DateTime.Now, priceList.Count);
                        }));
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                    }
                }
            });
        }

        private List<TxtParamClass> GetParamsFromDgv(bool flg)
        {
            List<TxtParamClass> result = new List<TxtParamClass>();
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                int loginId = 0;

                if (int.TryParse(row.Cells["LoginId"].Value.ToString(), out loginId)
                    && Convert.ToBoolean(row.Cells["chkCol"].Value) == flg)
                {
                    result.Add(list.Find(ele => ele.Login == loginId));
                }
            }
            return result;
        }

        int count = 1;
        private void WriteLogs(string logStr)
        {
            ThreadPool.QueueUserWorkItem(p =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    count++;
                    txtLogs.AppendText(DateTime.Now.ToLongTimeString() + " ： " + logStr + "\n");
                    if (count >= 5000)
                    {
                        File.AppendAllText(Path.Combine("log", DateTime.Now.ToString("yyyyMMdd") + ".log"), txtLogs.Text);
                        txtLogs.Clear();
                        txtLogs.ClearUndo();
                        count = 1;
                    }
                });
            });
        }
        private List<TxtParamClass> Search(string account, string name)
        {
            var result = list.AsEnumerable();
            if (!string.IsNullOrEmpty(account))
            {
                result = result.Where(e => e.Login.ToString().IndexOf(account) >= 0);
            }
            if (!string.IsNullOrEmpty(name))
            {
                result = result.Where(e => e.Name.IndexOf(name) >= 0);
            }
            return result.ToList();
        }

        private void MonPrice(List<TxtParamClass> listMonitors)
        {
            WriteLogs("开启价格处理线程");
            isMon = true;
            ThreadPool.QueueUserWorkItem(p =>
            {
                MonitorSetting setting = null;
                while (isMon && queueSetting.TryDequeue(out setting))
                {
                    RemoveQueue(setting);
                    TradingHelper.heightprice.Price = 0;
                    TradingHelper.lowprice.Price = 0;
                    bool flg = true;
                    while (DateTime.Now < setting.DateTimePriceMonStartTime)
                    {
                        if (flg)
                        {
                            WriteLogs("等待时间...." + DateTime.Now.ToString() + "->" + setting.DateTimePriceMonStartTime.ToString());
                            flg = false;
                        }
                        Thread.Sleep(1000);
                    }

                    for (int i = 0; i < listMonitors.Count; i++)
                    {
                        foreach (DataGridViewRow row in dataGridViewData.Rows)
                        {
                            if (row.Tag != null && row.Tag.ToString() == listMonitors[i].MemberId.ToString())
                            {
                                if (row.Cells["colMonLots"].Value == null || row.Cells["colMonRate"].Value == null)
                                {
                                    MessageBox.Show("请认真检查Monlots 和 MonRate是否设置值！");
                                }
                                else
                                {
                                    string monlots = row.Cells["colMonLots"].Value.ToString();
                                    string monrates = row.Cells["colMonRate"].Value.ToString();
                                    if (monlots.IndexOf(",") >= 0 && monrates.IndexOf(",") >= 0)
                                    {
                                        int lotsindex = monlots.IndexOf(","), rateindex = monrates.IndexOf(",");
                                        string lots = monlots.Substring(0, lotsindex);
                                        string rate = monrates.Substring(0, rateindex);
                                        row.Cells["colMonLots"].Value = monlots.Remove(0, lotsindex + 1);
                                        row.Cells["colMonRate"].Value = monrates.Remove(0, rateindex + 1);
                                        listMonitors[i].MonLots = double.Parse(lots);
                                        listMonitors[i].MonRate = double.Parse(rate);
                                    }
                                    else
                                    {
                                        listMonitors[i].MonLots = double.Parse(monlots);
                                        listMonitors[i].MonRate = double.Parse(monrates);
                                        row.Cells["colMonLots"].Value = string.Empty;
                                        row.Cells["colMonRate"].Value = string.Empty;
                                    }
                                }
                            }
                        }
                        listMonitors[i].Action = TxtParamActionEnum.Monitor;//重置当前已经交易的用户
                    }
                    while (DateTime.Now > setting.DateTimePriceMonStartTime
                        && DateTime.Now < setting.DateTimePickerMonEndTime)
                    {
                        try
                        {
                            double price = 0;
                            bool isopen = false;
                            while (priceList.TryDequeue(out price))
                            {
                                for (int i = 0; i < listMonitors.Count; i++)
                                {
                                    listMonitors[i].MonEnd = setting.DateTimePickerMonEndTime;
                                    listMonitors[i].MonStart = setting.DateTimePriceMonStartTime;
                                }
                                isopen = TradingHelper.StartPriceProcess(listMonitors, price, chkShowLog.Checked);
                                if (isopen)
                                {
                                    break;
                                }
                                Thread.Sleep(5);
                            }
                            if (isopen)
                            {
                                //queueSetting.Enqueue(setting);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex);
                            WriteLogs("处理报价出错：" + ex.ToString());
                        }
                        Thread.Sleep(50);
                    }
                }
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    toolStartMonitor.Text = "开启监听";
                    priceList = new ConcurrentQueue<double>();
                    isMon = false;
                    File.AppendAllText(Path.Combine("log", DateTime.Now.ToString("yyyyMMdd") + ".log"), txtLogs.Text);
                    txtLogs.Clear();
                    txtLogs.ClearUndo();
                }));
                WriteLogs("所有参数设置已经完成处理");
            });
        }

        private void DisplayQueue(MonitorSetting setting)
        {
            int index = dataGridViewQueue.Rows.Add();
            dataGridViewQueue.Rows[index].Cells["colDataTimePriceMonStartTime"].Value = setting.DateTimePriceMonStartTime;
            dataGridViewQueue.Rows[index].Cells["colDateTimePickerMonEndTime"].Value = setting.DateTimePickerMonEndTime;
            dataGridViewQueue.Rows[index].Cells["colTextBoxMonLots"].Value = setting.TextBoxMonLots;
            dataGridViewQueue.Rows[index].Cells["colTextBoxMonRate"].Value = setting.TextBoxMonRate;
        }
        private void RemoveQueue(MonitorSetting setting)
        {
            this.BeginInvoke((MethodInvoker)(() =>
            {
                foreach (DataGridViewRow row in dataGridViewQueue.Rows)
                {
                    if (Convert.ToDateTime(row.Cells["colDataTimePriceMonStartTime"].Value) == setting.DateTimePriceMonStartTime
                        && Convert.ToDateTime(row.Cells["colDateTimePickerMonEndTime"].Value) == setting.DateTimePickerMonEndTime
                        && Convert.ToInt32(row.Cells["colTextBoxMonLots"].Value) == setting.TextBoxMonLots
                        && Convert.ToDouble(row.Cells["colTextBoxMonRate"].Value) == setting.TextBoxMonRate)
                    {
                        dataGridViewQueue.Rows.Remove(row);
                        break;
                    }
                }
            }));
        }

        private void SaveDate(List<TxtParamClass> list)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.Proxy = null;
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                string str = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                string postStr = string.Format("content={0}", str);
                byte[] postData = Encoding.UTF8.GetBytes(postStr);
                byte[] responseData = webClient.UploadData(saveDataUrl, postData);
                string responseStr = Encoding.UTF8.GetString(responseData);

            }
            catch (Exception ee) { WriteLogs(ee.ToString()); }
        }

        private TxtParamClass RowConvertToModel(DataGridViewRow row, TxtParamClass model)
        {
            model.Name = row.Cells["ColName"].Value == null ? string.Empty : row.Cells["ColName"].Value.ToString();
            model.Login = row.Cells["LoginId"].Value == null ? 0 : Convert.ToInt32(row.Cells["LoginId"].Value);
            model.StartTime = row.Cells["StartDateTime"].Value == null ? DateTime.Now : Convert.ToDateTime(row.Cells["StartDateTime"].Value);
            model.EndTime = row.Cells["EndDateTime"].Value == null ? DateTime.Now : Convert.ToDateTime(row.Cells["EndDateTime"].Value);
            model.Amount = row.Cells["StartAmount"].Value == null ? 0 : Convert.ToDouble(row.Cells["StartAmount"].Value);
            model.Ratio = row.Cells["Ratio"].Value == null ? 1 : Convert.ToDouble(row.Cells["Ratio"].Value);
            model.Groups = row.Cells["Group"].Value == null ? new List<string>() : row.Cells["Group"].Value.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return model;
        }
        #endregion

        private void btnRest_Click(object sender, EventArgs e)
        {
            dateTimePriceMonStartTime.Value = DateTime.Now;
            dateTimePickerMonEndTime.Value = DateTime.Now;
            textBoxMonLots.Text = string.Empty;
            textBoxMonRate.Text = string.Empty;
            queueSetting = new ConcurrentQueue<MonitorSetting>();
            dataGridViewQueue.Rows.Clear();
        }

        /// <summary>
        /// 复制订单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void toolCopy_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value))
                {
                    copyAccount = int.Parse(row.Cells["LoginId"].Value.ToString());
                    break;
                }
            }
            if (copyAccount == 0)
            {
                MessageBox.Show("请选择要复制的账户！");
            }
            else
            {
                loading.ShowLoading();
                copyRecords = await CustomService.Service.GetTradesRecordHistoryAsync(copyAccount, DateTime.Now.AddYears(-20), DateTime.Now.AddYears(1));
                loading.HideLoading();
                if (copyRecords.Length > 0)
                {
                    MessageBox.Show("复制成功！总订单数：" + copyRecords.Length);
                }
                else
                {
                    MessageBox.Show("复制失败，请核对是否有订单！总订单数：" + copyRecords.Length);
                }
            }
        }

        /// <summary>
        /// 粘贴到账户订单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void toolPaset_Click(object sender, EventArgs e)
        {

            int pasetAccount = 0, account = 0;
            if (string.IsNullOrEmpty(txtAccount.Text.Trim()) || !int.TryParse(txtAccount.Text.Trim(), out account))
            {
                MessageBox.Show("请填写正确插单用户，避免复制的订单号不重复！");
                txtAccount.Focus();
                return;
            }
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value))
                {
                    pasetAccount = int.Parse(row.Cells["LoginId"].Value.ToString());
                    break;
                }
            }
            if (copyAccount == 0)
            {
                MessageBox.Show("请先复制在进行选择账户粘贴！");
            }
            else if (copyAccount == pasetAccount)
            {
                MessageBox.Show("不能对同一个账户进行复制粘贴！");
            }
            else if (copyRecords == null || copyRecords.Length == 0)
            {
                MessageBox.Show("复制的记录为空，无需进行粘贴！");
            }
            else
            {
                //粘贴操作
                //删掉该账号上所有的历史记录
                loading.ShowLoading();
                MT4OperResult result = await CustomService.Service.TradeDeleteAllHistoryAsync(pasetAccount);

                if (result.ErrorCode != -1)
                {
                    foreach (TradeRecordSE record in copyRecords)
                    {
                        if (record.Cmd == 6)
                        {
                            MT4OperResult depositResut = await CustomService.Service.TradeTranscationBalanceAsync(pasetAccount, record.Profit);

                            if (depositResut.ErrorCode != -1)
                            {
                                _log.InfoFormat("成功入金1条 从{0}账户的{1}订单 复制到 {2}账户的{3}订单", copyAccount, record.OrderId, pasetAccount, depositResut.ErrorCode);
                            }
                            else
                            {
                                _log.InfoFormat("入金失败1条 从{0}账户的{1}订单 复制到 {2}账户的{3}订单", copyAccount, record.OrderId, pasetAccount, depositResut.ErrorCode);
                            }
                            continue;
                        }
                        TradeTransInfoSE trade = new TradeTransInfoSE();
                        trade.Symbol = record.Symbol;
                        trade.Cmd = record.Cmd;
                        trade.StopLoss = record.StopLoss;
                        trade.TakeProfit = record.TakeProfit;
                        trade.OrderBy = pasetAccount;
                        trade.Volume = record.Volume;
                        trade.Order = 0;
                        trade.IeDeviation = 0;
                        trade.Expiration = DateTime.Now;
                        trade.Price = record.OpenPrice;
                        MT4OperResult openResult = await CustomService.Service.TradeTranscationOpenAsync(trade);

                        if (openResult.ErrorCode != -1)
                        {
                            //插入无用单
                            trade.Volume = 1 * 100;
                            trade.Symbol = record.Symbol;
                            trade.OrderBy = account;
                            MT4OperResult noresult = await CustomService.Service.TradeTranscationOpenAsync(trade);
                            //平掉无用单
                            await CustomService.Service.TradeTranscationCloseAsync(noresult.ReturnValue, record.ClosePrice);
                            MT4OperResult closeResult = await CustomService.Service.TradeTranscationCloseAsync(openResult.ReturnValue, record.ClosePrice);
                            if (closeResult.ErrorCode != -1)
                            {
                                //修改订单
                                MT4OperResult modifyResult = await CustomService.Service.AdmTradeRecordModifyTimeAndPriceAsync(openResult.ReturnValue, record.OpenTime, record.OpenPrice, record.CloseTime, record.ClosePrice, record.Profit);
                                if (modifyResult.ErrorCode != -1)
                                {
                                    _log.InfoFormat("成功复制1条 从{0}账户的{1}订单 复制到 {2}账户的{3}订单", copyAccount, record.OrderId, pasetAccount, openResult.ReturnValue);
                                }
                                else
                                {
                                    _log.FatalFormat("复制1条失败，修改订单时出错 从{0}账户的{1}订单 复制到 {2}账户的{3}订单", copyAccount, record.OrderId, pasetAccount, openResult.ReturnValue);
                                }
                            }
                            else
                            {
                                _log.FatalFormat("复制1条失败，平仓时出错 从{0}账户的{1}订单 复制到 {2}账户的{3}订单", copyAccount, record.OrderId, pasetAccount, openResult.ReturnValue);
                            }
                        }
                        else
                        {
                            _log.FatalFormat("复制1条失败，开仓时出错 从{0}账户的{1}订单", copyAccount, record.OrderId);
                        }
                    }

                }
                else
                {
                    _log.Fatal(JsonConvert.SerializeObject(result));
                }
                copyAccount = 0;
                pasetAccount = 0;
                copyRecords = null;
                loading.HideLoading();
            }
        }

        /// <summary>
        /// 批量平仓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void toolCloseAll_Click(object sender, EventArgs e)
        {
            List<TradeCloseModel> closes = new List<TradeCloseModel>();
            loading.ShowLoading();
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                if (Convert.ToBoolean(row.Cells["chkCol"].Value))
                {
                    int loginId = int.Parse(row.Cells["LoginId"].Value.ToString());
                    PumpingService.TradeRecordSE[] trades = await CustomService.Service.GetOpenTradeByLoginAsync(loginId);
                    if (trades.Length <= 0 || trades == null)
                    {
                        continue;
                    }
                    closes.AddRange(ConvertToCloseModel(trades.ToList()));
                }
            }
            if (closes.Count <= 0)
            {
                MessageBox.Show("没有要平仓的订单！");
                loading.HideLoading();
                return;
            }
            TradeCloseResponseModel[] responseModels = await CustomService.Service.TradeCloseAllAsync(closes.ToArray());
            loading.HideLoading();
            StringBuilder sbMessage = new StringBuilder();
            foreach (TradeCloseResponseModel response in responseModels)
            {
                if (response.ErrorCode == -1)
                {
                    _log.Fatal(JsonConvert.SerializeObject(response));
                    sbMessage.AppendFormat("Account :{0} Message:{1}", response.CloseModel.Account, response.ErrorDescription);
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
                MessageBox.Show("批量平仓成功！");
            }
            else
            {
                MessageBox.Show("平仓失败账户：\r\b" + sbMessage.ToString());
            }
        }

        private void toolcmboxgroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                if (toolcmboxgroup.SelectedItem != null)
                {
                    string group = toolcmboxgroup.SelectedItem.ToString();
                    object cellGroup = row.Cells["Group"].Value;
                    if (cellGroup != null)
                    {
                        if (cellGroup.ToString().Contains(","))
                        {
                            string[] array = cellGroup.ToString().Split(',');
                            if (array.Where(s => s == group).FirstOrDefault() != null)
                            {
                                row.Cells["chkCol"].Value = true;
                            }
                            else
                            {
                                row.Cells["chkCol"].Value = false;
                            }
                        }
                        else if (cellGroup.ToString() == group)
                        {
                            row.Cells["chkCol"].Value = true;
                        }
                        else
                        {
                            row.Cells["chkCol"].Value = false;
                        }
                    }

                }
            }
        }

        private void toolTxtAccount_TextChanged(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                loading.ShowLoading();
                var result = Search(toolTxtAccount.Text, tooltxtName.Text);
                Display(result);
            }).ContinueWith(task =>
            {
                loading.HideLoading();
            });
        }

        private void tooltxtName_TextChanged(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                loading.ShowLoading();
                var result = Search(toolTxtAccount.Text, tooltxtName.Text);
                Display(result);
            }).ContinueWith(task =>
            {
                loading.HideLoading();
            });

        }


        private void toolStartMonitor_Click(object sender, EventArgs e)
        {
            loading.ShowLoading();
            if (toolStartMonitor.Text == "开启监听")
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        List<TxtParamClass> array = list.Clone();
                        List<TxtParamClass> listMonitor = new List<TxtParamClass>();
                        foreach (DataGridViewRow row in dataGridViewData.Rows)
                        {
                            if (Convert.ToBoolean(row.Cells["chkCol"].Value))
                            {
                                if (Convert.ToDouble(row.Cells["Ratio"].Value) == 0)
                                {
                                    MessageBox.Show("请检查盈利系数是否设置正确，有0的盈利系数");
                                    loading.HideLoading();
                                    return;
                                }
                                int loginid = 0;
                                if (int.TryParse(row.Cells["LoginId"].Value.ToString(), out loginid))
                                {
                                    //检查余额是否过低
                                    double amount = 0;
                                    double.TryParse(string.IsNullOrEmpty(row.Cells["StartAmount"].Value.ToString()) ? "0" : row.Cells["StartAmount"].Value.ToString(), out amount);
                                    if (amount <= 2000)
                                    {
                                        WriteLogs(string.Format("{0}：用户余额过低，余额为：{1}", loginid, amount));
                                        continue;
                                    }
                                    //MarginLevelSE margin = Core.CustomService.Service.GetMarginLevel(loginid);
                                    //if (margin == null)
                                    //{
                                    //    WriteLogs("MT4不存在该用户：" + loginid);
                                    //    continue;
                                    //}
                                    //if (margin != null && margin.Balance < 2000)
                                    //{
                                    //    WriteLogs(string.Format("{0}：用户余额过低，余额为：{1}", loginid, margin.Balance));
                                    //    continue;
                                    //}

                                }

                                int memberId = 0;
                                int.TryParse(row.Tag.ToString(), out memberId);
                                if (memberId == 0)
                                {
                                    WriteLogs("不存在该用户：" + loginid);
                                    continue;
                                }
                                var element = array.Find(ele => ele.MemberId == memberId);
                                element = RowConvertToModel(row, element);
                                listMonitor.Add(element);
                            }
                        }
                        WriteLogs("监听集合长度：" + listMonitor.Count);
                        if (listMonitor.Count > 0)
                        {
                            WriteLogs("准备监听MT4");
                            MonPrice(listMonitor);
                            toolStartMonitor.Text = "停止监听";
                            loading.HideLoading();
                            WriteLogs("成功开启监听");
                        }
                        else
                        {
                            loading.HideLoading();
                            MessageBox.Show("请选择要监听的用户");
                        }
                    }
                    catch (FaultException<ExceptionDetail> ex)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("Message:" + ex.Detail.Message);
                        sb.AppendLine();
                        sb.Append("Type:" + ex.Detail.Type);
                        sb.AppendLine();
                        sb.Append("StackTrace:" + ex.Detail.StackTrace);
                        sb.AppendLine();
                        sb.Append("HelpLink:" + ex.Detail.HelpLink);
                        sb.AppendLine();
                        WriteLogs(sb.ToString());
                        MessageBox.Show(ex.Detail.Message);
                    }
                }).ContinueWith(task =>
                {
                    loading.HideLoading();
                });
            }
            else
            {
                isMon = false;
                toolStartMonitor.Text = "开启监听";
                loading.HideLoading();
                WriteLogs("停止监听成功");
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                bool ischek = false;
                foreach (DataGridViewRow row in dataGridViewData.Rows)
                {
                    if (Convert.ToBoolean(row.Cells["chkCol"].Value))
                    {
                        ischek = true;
                    }
                }
                if (!ischek)
                {
                    MessageBox.Show("请先选择要监听的用户");
                    return;
                }
                double lots;
                double rate;
                if (!double.TryParse(textBoxMonLots.Text, out lots))
                {

                    MessageBox.Show("输入Monlots有误 int！");
                    return;
                }
                if (!double.TryParse(textBoxMonRate.Text, out rate))
                {
                    MessageBox.Show("输入MonRate有误 double！");
                    return;
                }
                if (lots >= 100 || lots <= -50)
                {
                    MessageBox.Show("您设置的手数过大，请重新设置");
                    return;
                }
                loading.ShowLoading();
                MonitorSetting setting = new MonitorSetting();
                setting.DateTimePriceMonStartTime = dateTimePriceMonStartTime.Value;
                setting.DateTimePickerMonEndTime = dateTimePickerMonEndTime.Value;
                setting.TextBoxMonLots = lots;
                setting.TextBoxMonRate = rate;
                foreach (DataGridViewRow row in dataGridViewData.Rows)
                {
                    if (Convert.ToBoolean(row.Cells["chkCol"].Value))
                    {
                        row.Cells["colMonLots"].Value = (row.Cells["colMonLots"].Value == null ? string.Empty : row.Cells["colMonLots"].Value.ToString() + ",") + lots;
                        row.Cells["colMonRate"].Value = (row.Cells["colMonRate"].Value == null ? string.Empty : row.Cells["colMonRate"].Value.ToString() + ",") + rate;
                    }
                }
                queueSetting.Enqueue(setting);
                DisplayQueue(setting);
                loading.HideLoading();
                WriteLogs("设置成功添加到队列");
            }
            catch (Exception ex)
            {
                loading.HideLoading();
                _log.Error(ex);

            }
        }
        private void toolClearBtn_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                if (Convert.ToBoolean(row.Cells["chkCol"].Value))
                {
                    row.Cells["chkCol"].Value = false;
                }
            }
        }

        private void toolInvertSelection_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                row.Cells["chkCol"].Value = !Convert.ToBoolean(row.Cells["chkCol"].Value);
            }
        }

        private void toolBtnSave_Click(object sender, EventArgs e)
        {
            List<TxtParamClass> result = new List<TxtParamClass>();
            foreach (DataGridViewRow row in dataGridViewData.Rows)
            {
                int memberId = 0;
                int.TryParse(row.Tag.ToString(), out memberId);
                if (memberId == 0)
                {
                    continue;
                }
                var element = list.Find(ele => ele.MemberId == memberId);
                element = RowConvertToModel(row, element);
                result.Add(element);
            }
            Task.Factory.StartNew(() =>
            {
                loading.ShowLoading();
                SaveDate(result);
            }).ContinueWith(task =>
            {
                loading.HideLoading();
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    MessageBox.Show("成功同步到fino平台");
                }));
            });
        }

        private void ToolStripMenuItemGetBanlance_Click(object sender, EventArgs e)
        {
            loading.ShowLoading();
            Task.Factory.StartNew(() =>
            {

                foreach (DataGridViewRow row in dataGridViewData.Rows)
                {
                    object obj = row.Cells["LoginId"].Value;
                    int loginid = 0;
                    if (obj != null && int.TryParse(obj.ToString(), out loginid))
                    {
                        Task.Factory.StartNew(() =>
                        {
                            MarginLevelSE model = CustomService.Service.GetMarginLevel(loginid);
                            if (model != null)
                            {
                                this.BeginInvoke((MethodInvoker)(() =>
                                {

                                    row.Cells["StartAmount"].Value = model.Balance.ToString("F2");
                                }));
                            }
                        }, TaskCreationOptions.AttachedToParent);
                    }
                }
            }).ContinueWith(task =>
            {
                loading.HideLoading();
            });
        }


    }
}
