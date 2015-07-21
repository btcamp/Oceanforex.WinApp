using Newtonsoft.Json;
using MT4Account.Robot.Core.Models;
using MT4Account.Robot.PumpingService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MT4Account.Robot.Core;

namespace MT4Account.Robot.Core.Helpers
{
    public class TradingHelper
    {
        #region 字段
        static log4net.ILog _log = log4net.LogManager.GetLogger(typeof(TradingHelper));
        private static List<PriceModel> mon_lowPrice = new List<PriceModel>();
        //监听账户的状态  未插单 已插单
        private static Dictionary<string, int> dictIsAdd = new Dictionary<string, int>();
        private static List<PriceModel> mon_heightPrice = new List<PriceModel>();
        private static List<int> listMonitorOrders = new List<int>();
        private static int chaosAccount = Convert.ToInt32(ConfigurationManager.AppSettings["chaosAccount"]);
        private static bool mon_isAddAll = false;


        private static object _lock = new object();

        private static Action<string> writeLogs;


        private static MainForm mainForm;


        #endregion

        #region 属性
        /// <summary>
        /// 主窗体
        /// </summary>
        public static MainForm MainForm
        {
            get { return TradingHelper.mainForm; }
            set { TradingHelper.mainForm = value; }
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        public static Action<string> WriteLogs
        {
            get { return TradingHelper.writeLogs; }
            set { TradingHelper.writeLogs = value; }
        }
        public static bool Mon_isAddAll
        {
            get { return TradingHelper.mon_isAddAll; }
            set { TradingHelper.mon_isAddAll = value; }
        }

        public static List<PriceModel> Mon_lowPrice
        {
            get { return TradingHelper.mon_lowPrice; }
            private set { TradingHelper.mon_lowPrice = value; }
        }


        public static List<PriceModel> Mon_heightPrice
        {
            get { return TradingHelper.mon_heightPrice; }
            private set { TradingHelper.mon_heightPrice = value; }
        }


        public static List<int> ListMonitorOrders
        {
            get { return TradingHelper.listMonitorOrders; }
            private set { TradingHelper.listMonitorOrders = value; }
        }


        public static Dictionary<string, int> DictIsAdd
        {
            get { return TradingHelper.dictIsAdd; }
            private set { TradingHelper.dictIsAdd = value; }
        }
        #endregion

        private static double upprice = 0;

        public static PriceModel heightprice = new PriceModel();

        public static PriceModel lowprice = new PriceModel();

        public static bool StartPriceProcess(List<TxtParamClass> listMonitors, double average = 0, bool isShowLog = false)
        {
            lock (_lock)
            {
                bool isOpen = false;
                //PriceModel lastprice = new PriceModel();
                //lastprice.Price = average;
                //lastprice.Time = DateTime.Now;
                if (lowprice.Price == 0 || heightprice.Price == 0)
                {
                    upprice = average;
                    heightprice.Price = average;
                    heightprice.Time = DateTime.UtcNow;
                    lowprice.Price = average;
                    lowprice.Time = DateTime.UtcNow;
                    return isOpen;
                }
                if (listMonitors.Count == 0)
                {
                    return isOpen;
                }
                //更新最高价和最低价
                if (heightprice.Price < average)
                {
                    heightprice.Price = average;
                    heightprice.Time = DateTime.UtcNow;
                }
                if (lowprice.Price > average)
                {
                    lowprice.Price = average;
                    lowprice.Time = DateTime.UtcNow;
                }
                decimal spread = Math.Abs((decimal)heightprice.Price - (decimal)lowprice.Price);
                if (isShowLog)
                {
                    WriteLogs(string.Format("高价:{0} 低价:{1} 价差:{2} 高价时间：{3} 低价时间：{4}", heightprice.Price, lowprice.Price, spread, heightprice.Time.ToString(), lowprice.Time.ToString()));
                }
                _log.Info(string.Format("价差：Abs({0}-{1})={2}", heightprice.Price, lowprice.Price, spread));
                foreach (TxtParamClass item in listMonitors)
                {
                    try
                    {
                        if (spread > (decimal)(item.MonRate + 20))
                        {
                            WriteLogs("价差异常；价差为：" + spread);
                            continue;
                        }
                        if (spread > (decimal)item.MonRate && item.Action != TxtParamActionEnum.IsTraded)
                        {
                            int _cmd = 0;
                            double openPrice = 0, closePrice = 0;
                            DateTime openTime = DateTime.Now, closeTime = DateTime.Now;
                            //高价在前面
                            if (lowprice.Time < heightprice.Time)
                            {
                                _cmd = 0; ///BUY
                                openPrice = lowprice.Price;
                                openTime = lowprice.Time;

                                closePrice = heightprice.Price;
                                closeTime = heightprice.Time;
                            }
                            else
                            {
                                _cmd = 1; //SELL
                                openPrice = heightprice.Price;
                                openTime = heightprice.Time;

                                closePrice = lowprice.Price;
                                closeTime = lowprice.Time;
                            }
                            if (item.MonLots < 0)
                            {
                                _cmd = _cmd == 1 ? 0 : 1;
                            }
                            TradeTransInfoSE trade = new TradeTransInfoSE
                            {
                                Cmd = _cmd,
                                OrderBy = item.Login,
                                Symbol = "XAUUSD",
                                Volume = Math.Abs((int)((item.MonLots * item.Ratio) * 100)),
                                StopLoss = 0,
                                TakeProfit = 0,
                                Price = openPrice
                            };

                            while (true)
                            {
                                //开仓
                                MT4OperResult result = CustomService.Service.TradeTranscationOpen(trade);
                                if (result.ErrorCode != -1)
                                {
                                    while (true)
                                    {
                                        //平仓
                                        MT4OperResult closeResult = CustomService.Service.TradeTranscationClose(result.ReturnValue, closePrice);
                                        if (closeResult.ErrorCode != -1)
                                        {
                                            while (true)
                                            {
                                                //修改
                                                MT4OperResult modifyResult = CustomService.Service.AdmTradeRecordModifyTime(result.ReturnValue, openTime, closeTime);
                                                if (modifyResult.ErrorCode != -1)
                                                {
                                                    break;//跳出修改循环
                                                }
                                                else
                                                {
                                                    WriteLogs(string.Format("修改失败：login：{0} message：{1}：{2}", item.Login, modifyResult.ErrorDescription, JsonConvert.SerializeObject(trade)));
                                                }
                                            }
                                            break;//跳出平仓循环
                                        }
                                        else
                                        {
                                            WriteLogs(string.Format("平仓失败：login：{0} message：{1}：{2}", item.Login, closeResult.ErrorDescription, JsonConvert.SerializeObject(trade)));
                                        }
                                    }
                                    //整个交易完成
                                    WriteLogs(string.Format("{0}：方向：{1} lots：{2} volume：{3}", item.Login, _cmd == 1 ? "sell" : "buy", Math.Abs(item.MonLots), trade.Volume));
                                    item.Action = TxtParamActionEnum.IsTraded;//标记当前用户已经交易了
                                    break;//跳出开仓循环
                                }
                                else
                                {
                                    WriteLogs(string.Format("开仓失败：login：{0} message：{1}：{2}", item.Login, result.ErrorDescription, JsonConvert.SerializeObject(trade)));
                                }
                                Thread.Sleep(100);
                            }
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
                    }
                    catch (Exception ex)
                    {
                        writeLogs(ex.ToString());
                    }
                }
                if (listMonitors.Where(e => e.Action == TxtParamActionEnum.IsTraded).Count() == listMonitors.Count)
                {
                    heightprice.Price = 0;
                    lowprice.Price = 0;
                    isOpen = true;
                }
                return isOpen;
            }
        }

        public static void PriceProcess(List<TxtParamClass> listMonitors, double average = 0)
        {
            if (average == 0)
            {
                return;
            }
            if (listMonitors.Count == 0)
            {
                return;
            }
            //StringBuilder sb = new StringBuilder();
            lock (_lock)
            {
                //string result = "";

                #region Mon

                //SetLb(string.Format("当前价格:{0}，最高价：{1}，最低价：{2}，价差：{3}", average, mon_heightPrice.Price, mon_lowPrice.Price, Spread));

                foreach (TxtParamClass tpc in listMonitors)
                {
                    PriceModel heightPrice = mon_heightPrice.Where(p => p.Login == tpc.Login).FirstOrDefault();
                    PriceModel lowPrice = mon_lowPrice.Where(p => p.Login == tpc.Login).FirstOrDefault();
                    if (heightPrice == null || lowPrice == null)
                    {
                        _log.Info(JsonConvert.SerializeObject(mon_heightPrice) + ":" + tpc.Login);
                        _log.Info(JsonConvert.SerializeObject(mon_lowPrice) + ":" + tpc.Login);

                    }
                    if (DateTime.Now.Ticks >= tpc.MonStart.Ticks && DateTime.Now.Ticks <= tpc.MonEnd.Ticks)
                    {
                        if (heightPrice.Price == 0)
                        {
                            var _heightPrice = mon_heightPrice.Where(p => p.Login == tpc.Login).FirstOrDefault();
                            if (_heightPrice != null)
                            {
                                _heightPrice.Price = average;
                                _heightPrice.Time = DateTime.Now;
                            }
                            //for (int i = 0; i < mon_heightPrice.Count; i++)
                            //{
                            //    if (mon_heightPrice[i].Login == tpc.Login)
                            //    {
                            //        mon_heightPrice[i].Price = average;
                            //        mon_heightPrice[i].Time = DateTime.Now;
                            //        break;
                            //    }
                            //}
                            //mon_heightPrice.Where(p => p.Login == tpc.Login).First().Price = average;
                            //mon_heightPrice.Where(p => p.Login == tpc.Login).First().Time = DateTime.Now;
                        }

                        if (lowPrice.Price == 0)
                        {
                            var _lowPrice = mon_lowPrice.Where(p => p.Login == tpc.Login).FirstOrDefault();
                            if (_lowPrice != null)
                            {
                                _lowPrice.Price = average;
                                _lowPrice.Time = DateTime.Now; ;

                            }
                            //for (int i = 0; i < mon_lowPrice.Count; i++)
                            //{
                            //    if (mon_lowPrice[i].Login == tpc.Login)
                            //    {
                            //        mon_lowPrice[i].Price = average;
                            //        mon_lowPrice[i].Time = DateTime.Now;
                            //        break;
                            //    }
                            //}
                            //mon_lowPrice.Where(p => p.Login == tpc.Login).First().Price = average;
                            //mon_lowPrice.Where(p => p.Login == tpc.Login).First().Time = DateTime.Now;
                        }
                        if (lowPrice.Price > average)
                        {
                            //WriteLogs(string.Format("变更最低价为：{0}", average));
                            var _lowPrice = mon_lowPrice.Where(p => p.Login == tpc.Login).FirstOrDefault();
                            if (_lowPrice != null)
                            {
                                _lowPrice.Price = average;
                                _lowPrice.Time = DateTime.Now; ;
                            }
                            //for (int i = 0; i < mon_lowPrice.Count; i++)
                            //{
                            //    if (mon_lowPrice[i].Login == tpc.Login)
                            //    {
                            //        mon_lowPrice[i].Price = average;
                            //        mon_lowPrice[i].Time = DateTime.Now;
                            //        break;
                            //    }
                            //}
                            //mon_lowPrice.Where(p => p.Login == tpc.Login).First().Price = average;
                            //mon_lowPrice.Where(p => p.Login == tpc.Login).First().Time = DateTime.Now;
                        }
                        if (heightPrice.Price < average)
                        {
                            //WriteLogs(string.Format("变更最高价为：{0}", average));
                            var _heightPrice = mon_heightPrice.Where(p => p.Login == tpc.Login).FirstOrDefault();
                            if (_heightPrice != null)
                            {
                                _heightPrice.Price = average;
                                _heightPrice.Time = DateTime.Now;
                            }
                            //for (int i = 0; i < mon_heightPrice.Count; i++)
                            //{
                            //    if (mon_heightPrice[i].Login == tpc.Login)
                            //    {
                            //        mon_heightPrice[i].Price = average;
                            //        mon_heightPrice[i].Time = DateTime.Now;
                            //        break;
                            //    }
                            //}
                            //mon_heightPrice.Where(p => p.Login == tpc.Login).First().Price = average;
                            //mon_heightPrice.Where(p => p.Login == tpc.Login).First().Time = DateTime.Now;

                        }
                    }
                    heightPrice = mon_heightPrice.Where(p => p.Login == tpc.Login).FirstOrDefault();
                    lowPrice = mon_lowPrice.Where(p => p.Login == tpc.Login).FirstOrDefault();
                    if (heightPrice == null || lowPrice == null)
                    {
                        _log.FatalFormat("heightPrice：lowPrice两对象为空；loginid:{0}", tpc.Login);
                        continue;
                    }
                    double Spread = heightPrice.Price - lowPrice.Price;

                    //if (DictIsAdd[tpc.Name] == 1 && tpc.ExpectedProfit > 0)
                    //{
                    //    AdjustOrders(tpc.Login, tpc.MonStart, tpc.MonEnd, tpc.ExpectedProfit);
                    //    DictIsAdd[tpc.Name] = 2;
                    //}

                    if (lowPrice.Price == 0 || heightPrice.Price == 0 || mon_isAddAll == true)
                    {
                    }
                    else if (Spread <= tpc.MonRate || DateTime.Now.Ticks <= tpc.MonStart.Ticks || DateTime.Now.Ticks >= tpc.MonEnd.Ticks || DictIsAdd[tpc.Name] >= 1)
                    {
                        //WriteLogs(string.Format("本期段价差{0}，少于{1}", heightPrice.Price - lowPrice.Price, mon_rate));

                    }
                    else
                    {

                        if (tpc.ExpectedProfit > 0)
                        {
                            //TradeRecordsManaged[] trm = MT4Wrapper.GetInstance().GetTradesRecordHistory(tpc.Login, tpc.StartTime, tpc.EndTime);
                            TradeRecordSE[] trm = CustomService.Service.GetTradesRecordHistory(tpc.Login, tpc.MonStart, tpc.MonEnd);
                            double historyProfit = tpc.FinishedProfit;
                            if (trm != null)
                            {
                                historyProfit = trm.Sum(p => p.Profit);
                            }
                            tpc.MonLots = Convert.ToInt32((tpc.ExpectedProfit - historyProfit) / (heightPrice.Price - lowPrice.Price));
                        }

                        WriteLogs(string.Format("准备插入单，最高{0}，最低{1}，价差{2}，时间{3}", heightPrice.Price, lowPrice.Price, heightPrice.Price - lowPrice.Price, DateTime.Now));
                        int _cmd = 0;
                        double openPrice = 0;
                        double closePrice = 0;

                        DateTime openTime = DateTime.Now;
                        DateTime closeTime = DateTime.Now;


                        if (lowPrice.Time < heightPrice.Time)
                        {
                            _cmd = 0; ///BUY
                            openPrice = lowPrice.Price;
                            openTime = lowPrice.Time;

                            closePrice = heightPrice.Price;
                            closeTime = heightPrice.Time;
                        }
                        else
                        {
                            _cmd = 1; //SELL
                            openPrice = heightPrice.Price;
                            openTime = heightPrice.Time;

                            closePrice = lowPrice.Price;
                            closeTime = lowPrice.Time;
                        }


                        double monLots = tpc.MonLots * tpc.Ratio;
                        if (monLots < 0)
                        {
                            if (_cmd == 1)
                            {
                                _cmd = 0;
                            }
                            else if (_cmd == 0)
                            {
                                _cmd = 1;
                            }
                            monLots = 0 - monLots;
                        }
                        DictIsAdd[tpc.Name] = 1;
                        Random random = new Random((int)DateTime.Now.Ticks);
                        closeTime = closeTime.AddSeconds(random.Next(-5, 5));
                        InsertRecord(new TradeTransInfoSE
                        {
                            Cmd = _cmd,
                            OrderBy = tpc.Login,
                            Symbol = "XAUUSD",
                            Volume = (int)(monLots * 100),
                            StopLoss = 0,
                            TakeProfit = 0,
                            Price = openPrice
                        }, closePrice, openTime, closeTime, false);

                        WriteLogs(string.Format("为{0}插入{1}手", tpc.Login, tpc.MonLots));

                        //计算总盈利
                        //TradeRecordsManaged[] trm0 = MT4Wrapper.GetInstance().GetTradesRecordHistory(tpc.Login, tpc.StartTime, tpc.EndTime);

                        //if (trm0 != null)
                        //{
                        //    double profits = trm0.Sum(p => p.profit);
                        //    mainForm.SetParamsDgv("colFinishedProfit", profits, tpc.Login);
                        //}

                    }

                }
                if (DictIsAdd.All(p => p.Value >= 1))
                    mon_isAddAll = true;
                #endregion

                //不在时间范围内 请求master的逻辑 删除

            }

        }


        public static int InsertRecord(TradeTransInfoSE trade, double closePrice, DateTime openTime, DateTime closeTime, bool needChaos = true, bool insertOnly = false)
        {
            int order = -1;
            try
            {
                //MT4Wrapper wrapper = MT4Wrapper.GetInstance();
                if (needChaos)
                {
                    InsertChaosRecord();
                }
                WriteLogs(string.Format("=====准备插入订单{0}手======", trade.Volume));
            }
            catch (Exception ex)
            {
                WriteLogs(string.Format("捕捉到系统异常{0}", ex));
                _log.Error(ex);
                return order;
            }

            while (true)
            {
                try
                {
                    MT4OperResult result = CustomService.Service.TradeTranscationOpen(trade);
                    if (result.ErrorCode == -1)
                    {
                        WriteLogs("wcf开仓异常：" + result.ErrorDescription);
                    }
                    order = result.ReturnValue;
                }
                catch (Exception ee)
                {
                    WriteLogs(string.Format("捕捉到MT4异常：Message：{0}，Source：{1} order:{2} ", ee.Message, ee.Source, order));
                }
                if (order > 0)
                    break;

                Thread.Sleep(3000);
            }
            if (insertOnly)
            {
                WriteLogs("只开仓，不做其他操作，开仓ID：" + order);
                return order;
            }

            bool isClose = false;
            while (!isClose)
            {
                try
                {

                    MT4OperResult result = CustomService.Service.TradeTranscationClose(order, closePrice);
                    if (result.ErrorCode != -1)
                    {
                        isClose = true;
                    }
                }
                catch (Exception ee)
                {
                    WriteLogs(string.Format("捕捉到MT4异常：Message：{0}，Source：{1} order:{2} 准备重试", ee.Message, ee.Source, order));

                }
            }

            bool isModify = false;
            while (!isModify)
            {

                try
                {

                    MT4OperResult result = CustomService.Service.AdmTradeRecordModifyTime(order, openTime, closeTime);
                    if (result.ErrorCode != -1)
                    {
                        WriteLogs(string.Format("=====插入订单{0}手成功，单号：{1},openTime:{2},closeTime:{3}=====", trade.Volume, order, openTime, closeTime));
                        isModify = true;
                    }
                    else
                    {
                        WriteLogs(string.Format("=====插入订单失败，修改订单时间失败 {0}=====", result.ErrorDescription));
                    }
                }
                catch (Exception ee)
                {
                    _log.Error(ee);
                    WriteLogs(string.Format("修改订单捕捉到MT4异常：Message：{0}，Source：{1} order:{2} 准备重试；openTime:{3},closeTime:{4}", ee.Message, ee.Source, order, openTime, closeTime));
                    if (order <= 0)
                    {
                        WriteLogs(string.Format("Order异常: {0}", order));
                        break;
                    }
                    Thread.Sleep(2000);
                    continue;
                }
            }
            return order;

        }

        static void InsertChaosRecord()
        {
            if (chaosAccount == 0)
            {
                Thread.Sleep(500);
                return;
            }
            Random r = new Random((int)DateTime.Now.Ticks);
            int max = r.Next(100, 500);
            List<TradeOpenModel> list = new List<TradeOpenModel>();
            for (int i = 0; i < r.Next(100, 500); i++)
            {
                var model = new TradeOpenModel();
                model.Cmd = 0;
                model.OrderBy = chaosAccount;
                model.Symbol = "XAUUSD";
                model.Volume = 1 * 100;
                model.StopLoss = 0;
                model.TakeProfit = 0;
                model.Price = 100;
                list.Add(model);
            }
            TradeOpenRsponseModel[] responses = CustomService.Service.TradeOpenAll(list.ToArray());
            foreach (var item in responses)
            {
                WriteLogs(JsonConvert.SerializeObject(item));
            }
            Thread.Sleep(1000);
        }
    }
}
