using MT4Account.Robot.PumpingService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MT4Account.Robot.Core.Forms
{
    public partial class ModifyOrder : Form
    {
        int _account = 0;
        UserControls.Loading loading = null;
        private log4net.ILog _log = log4net.LogManager.GetLogger(typeof(OpenForm));
        /// <summary>
        /// 修改集合
        /// </summary>
        private List<TradeModifyModel> list = new List<TradeModifyModel>();
        public ModifyOrder(int account, string name)
        {
            InitializeComponent();
            this._account = account;
            this.Text = string.Format(this.Text, name);
        }



        private async void ModifyOrder_Load(object sender, EventArgs e)
        {
            dataGridViewModifyData.AutoGenerateColumns = false;
            //创建加载框
            loading = new UserControls.Loading();
            loading.Showed += loading_Showed;
            loading.Hided += loading_Hided;
            this.Controls.Add(loading);
            loading.ShowLoading();

            PumpingService.TradeRecordSE[] trades = await Core.CustomService.Service.GetTradesRecordHistoryAsync(this._account, DateTime.Now.AddYears(-20), DateTime.Now);
            Display(trades.Where(ele => ele.Cmd != 6).ToArray());
            loading.HideLoading();
        }


        private void Display(TradeRecordSE[] trades)
        {
            if (trades.Length > 0)
            {
                foreach (TradeRecordSE trade in trades.OrderByDescending(e => e.CloseTime))
                {
                    int index = dataGridViewModifyData.Rows.Add();
                    dataGridViewModifyData.Rows[index].Cells["Login"].Value = trade.Login;
                    dataGridViewModifyData.Rows[index].Cells["OrderId"].Value = trade.OrderId;
                    dataGridViewModifyData.Rows[index].Cells["OpenTime"].Value = trade.OpenTime;
                    dataGridViewModifyData.Rows[index].Cells["Cmd"].Value = trade.Cmd == 0 ? "buy" : "sell";
                    dataGridViewModifyData.Rows[index].Cells["Volume"].Value = trade.Volume;
                    dataGridViewModifyData.Rows[index].Cells["Symbol"].Value = trade.Symbol;
                    dataGridViewModifyData.Rows[index].Cells["OpenPrice"].Value = trade.OpenPrice;
                    dataGridViewModifyData.Rows[index].Cells["CloseTime"].Value = trade.CloseTime;
                    dataGridViewModifyData.Rows[index].Cells["ClosePrice"].Value = trade.ClosePrice;
                    dataGridViewModifyData.Rows[index].Cells["Profit"].Value = trade.Profit;
                }
            }
        }
        void loading_Hided()
        {
            this.btnSave.Enabled = true;
            this.dataGridViewModifyData.Enabled = true;
        }

        void loading_Showed()
        {
            this.btnSave.Enabled = false;
            this.dataGridViewModifyData.Enabled = false;
        }

        private void dataGridViewModifyData_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {


            try
            {
                int orderTick = int.Parse(dataGridViewModifyData.Rows[e.RowIndex].Cells["OrderId"].Value.ToString());
                TradeModifyModel exist = list.Where(ee => ee.OrderTicket == orderTick).FirstOrDefault();
                if (exist != null)
                {
                    exist.ClosePrice = Convert.ToDouble(dataGridViewModifyData.Rows[e.RowIndex].Cells["ClosePrice"].Value);
                    exist.CloseTime = Convert.ToDateTime(dataGridViewModifyData.Rows[e.RowIndex].Cells["CloseTime"].Value);
                }
                else
                {
                    TradeModifyModel model = new TradeModifyModel();
                    model.ClosePrice = Convert.ToDouble(dataGridViewModifyData.Rows[e.RowIndex].Cells["ClosePrice"].Value);
                    model.CloseTime = Convert.ToDateTime(dataGridViewModifyData.Rows[e.RowIndex].Cells["CloseTime"].Value);
                    model.OrderTicket = orderTick;
                    list.Add(model);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (list.Count > 0)
            {
                loading.ShowLoading();
                TradeModifyResponseModel[] responseModels = await CustomService.Service.TradeModifyAllAsync(list.ToArray());
                loading.HideLoading();
                StringBuilder sbMessage = new StringBuilder();
                foreach (TradeModifyResponseModel response in responseModels)
                {
                    if (response.ErrorCode == -1)
                    {
                        _log.Fatal(JsonConvert.SerializeObject(response));
                        sbMessage.AppendFormat("OrderTicket :{0} Message:{1}", response.ModifyModel.OrderTicket, response.ErrorDescription);
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
                    MessageBox.Show("批量修改成功！");
                }
                else
                {
                    MessageBox.Show("修改失败订单：\r\b" + sbMessage.ToString());
                }
            }
            loading.HideLoading();
            list.Clear();
        }

    }
}
