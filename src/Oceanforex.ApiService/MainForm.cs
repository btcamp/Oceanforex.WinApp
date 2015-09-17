using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.ServiceModel;
using System.IO;

namespace Oceanforex.ApiService
{
    public partial class MainForm : Form
    {
        private static ConcurrentQueue<Models.EmailModel> emails = new ConcurrentQueue<Models.EmailModel>();

        private MT4Service.PumpServiceClient client = new MT4Service.PumpServiceClient("BasicHttpBinding_IPumpService");
        public static ConcurrentQueue<Models.EmailModel> Emails
        {
            get { return MainForm.emails; }
            private set { MainForm.emails = value; }
        }

        private static MainForm instance;

        public static MainForm Instance
        {
            get { return MainForm.instance; }
            private set { MainForm.instance = value; }
        }

        public MainForm()
        {
            InitializeComponent();

        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            instance = this;
            StartSendEmail();
            StartApiService();
            StartClearLog();
            StartSendOrdertoEmail();

        }

        private void StartClearLog()
        {
            Task.Factory.StartNew(() =>
            {
                WriteLogs("日志清除线程开启");
                while (true)
                {
                    try
                    {
                        File.WriteAllText("log/" + DateTime.Now.ToString("yyyyMMdd") + ".log", txtMsg.Text);
                        Thread.Sleep(24 * 3600 * 1000);
                        this.BeginInvoke((MethodInvoker)(() =>
                        {
                            txtMsg.Clear();
                            txtMsg.ClearUndo();
                        }));
                    }
                    catch (Exception ex)
                    {
                        WriteLogs(ex.ToString());
                    }
                }
            });
        }

        private void StartApiService()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ServiceHost host = new ServiceHost(typeof(ApiService.Services.ApiService));
                    host.Opened += (o, e) => { WriteLogs("成功启动ApiService...."); };
                    host.Open();
                }
                catch (Exception ex)
                {
                    WriteLogs(ex.ToString());
                }
            });
        }

        private void StartSendEmail()
        {
            Task.Factory.StartNew(() =>
            {
                WriteLogs("发送邮件线程已启动....");
                while (true)
                {
                    try
                    {
                        Models.EmailModel email = null;
                        if (emails.TryDequeue(out email))
                        {
                            MailMessage mailMessage = new MailMessage(new MailAddress(email.from, email.fromname), new MailAddress(email.to, email.toname));
                            mailMessage.Subject = email.subject;
                            mailMessage.Body = email.body;
                            mailMessage.IsBodyHtml = true;

                            SmtpClient client = new SmtpClient(email.smtp);
                            client.Credentials = new NetworkCredential(email.fromuser, email.frompwd);
                            client.Send(mailMessage);
                            WriteLogs(string.Format("成功发送邮件：from:{0}->to:{1}", email.from, email.to));
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLogs(ex.ToString());
                    }
                    Thread.Sleep(30000);
                }

            });

        }

        private void StartSendOrdertoEmail()
        {
            Task.Factory.StartNew(() =>
            {
                WriteLogs("给用户发送订单邮件已成功启动");
                DateTime dt = DateTime.Now;
                while (true)
                {
                    
                    if (DateTime.Now > dt)
                    {
                        string[] array = File.ReadAllLines("account.log");
                        try
                        {
                            foreach (string item in array)
                            {
                                int login = 0;
                                if (int.TryParse(item, out login))
                                {
                                    MT4Service.UserRecordSE accout = client.GetUserRecordsByLogin(login);
                                    int i = 1;
                                    while (string.IsNullOrEmpty(accout.Name))
                                    {
                                        accout = client.GetUserRecordsByLogin(login);
                                        if (i > 5)
                                        {
                                            break;
                                        }
                                        i++;
                                    }
                                    if (!string.IsNullOrEmpty(accout.Name))
                                    {
                                        MT4Service.TradeRecordSE[] records = client.GetOpenTrade(login, "oceanforex");
                                        StringBuilder sbtrading = new StringBuilder();
                                        int index = 0;
                                        foreach (MT4Service.TradeRecordSE record in records)
                                        {
                                            string style = index % 2 != 0 ? "style='background-color:#ccc'" : string.Empty;
                                            MT4Service.SymbolInfoSE symbol = client.GetSymbolInfo(record.Symbol);
                                            sbtrading.AppendFormat(@"<tr {14}>
                                                                                <td>{0}</td>
                                                                                <td>{1}</td>
                                                                                <td>{2}</td>
                                                                                <td>{3}</td>
                                                                                <td>{4}</td>
                                                                                <td>{5}</td>
                                                                                <td>{6}</td>
                                                                                <td>{7}</td>
                                                                                <td>{8}</td>
                                                                                <td>{9}</td>
                                                                                <td>{10}</td>
                                                                                <td>{11}</td>
                                                                                <td>{12}</td>
                                                                                <td>{13}</td>
                                                                            </tr>", record.OrderId,
                                                                                  record.OpenTime.ToString("yyyy.MM.dd HH:mm:ss"),
                                                                                  record.Cmd == 0 ? "buy" : "sell", (record.Volume / 100).ToString("f2"),
                                                                                  record.Symbol,
                                                                                  record.OpenPrice,
                                                                                  record.StopLoss,
                                                                                  record.TakeProfit,
                                                                                  "&nbsp;",
                                                                                  record.Cmd == 0 ? symbol.Bid : symbol.Ask,
                                                                                  record.Commission,
                                                                                  record.Taxes,
                                                                                  record.Storage,
                                                                                  record.Profit, style);
                                            index++;
                                        }
                                        double sumCom = records.Sum(e => e.Commission);
                                        double sumTaxes = records.Sum(e => e.Taxes);
                                        double sumStorage = records.Sum(e => e.Storage);
                                        double sumProfit = records.Sum(e => e.Profit);
                                        sbtrading.AppendFormat(@"<tr style='text-align:right;\'>
                                                                                <td colspan='10'>&nbsp;</td>
                                                                                <td class='mspt'>{0}</td>
                                                                                <td class='mspt'>{1}</td>
                                                                                <td class='mspt'>{2}</td>
                                                                                <td class='mspt'>{3}</td>
                                                                            </tr>", sumCom.ToString("f2"),
                                                                                  sumTaxes.ToString("f2"),
                                                                                  sumStorage.ToString("f2"),
                                                                                  sumProfit.ToString("f2"));
                                        sbtrading.AppendFormat(@"<tr>
                                                                                <td colspan='10'>&nbsp;</td>
                                                                                <td colspan='2' align='right'><b>Floating P/L:</b></td>
                                                                                <td colspan='2' align='right' title='Commission + Swap + Profit' class='mspt'><b>{0}</b></td>
                                                                            </tr>", (sumCom + sumStorage + sumProfit).ToString("N2"));
                                        //sbtrading.AppendFormat("<tr><td style='text-align:right;padding:5px' colspan='9'>Total：{0}</td></tr>", records.Sum(e => e.Profit).ToString("f2"));


                                        MT4Service.TradeRecordSE[] closerecords = client.GetTradesRecordHistory(login, DateTime.Now.AddMonths(-1), DateTime.Now.AddDays(1));
                                        StringBuilder sbtraded = new StringBuilder();
                                        int indexed = 0;
                                        closerecords = closerecords.Where(e => e.Cmd != 6).OrderByDescending(e => e.CloseTime).Take(20).ToArray();
                                        foreach (MT4Service.TradeRecordSE record in closerecords)
                                        {
                                            string style = indexed % 2 != 0 ? "style='background-color:#E0E0E0'" : string.Empty;
                                            sbtraded.AppendFormat(@"<tr {14}>
                                                                                <td>{0}</td>
                                                                                <td>{1}</td>
                                                                                <td>{2}</td>
                                                                                <td>{3}</td>
                                                                                <td>{4}</td>
                                                                                <td>{5}</td>
                                                                                <td>{6}</td>
                                                                                <td>{7}</td>
                                                                                <td>{8}</td>
                                                                                <td>{9}</td>
                                                                                <td>{10}</td>
                                                                                <td>{11}</td>
                                                                                <td>{12}</td>
                                                                                <td>{13}</td>
                                                                            </tr>", record.OrderId,
                                                                                  record.OpenTime.ToString("yyyy.MM.dd HH:mm:ss"),
                                                                                  record.Cmd == 0 ? "buy" : "sell", (record.Volume / 100).ToString("f2"),
                                                                                  record.Symbol,
                                                                                  record.OpenPrice,
                                                                                  record.StopLoss,
                                                                                  record.TakeProfit,
                                                                                  record.CloseTime.ToString("yyyy.MM.dd HH:mm:ss"),
                                                                                  record.ClosePrice,
                                                                                  record.Commission,
                                                                                  record.Taxes,
                                                                                  record.Storage,
                                                                                  record.Profit,
                                                                                  style);
                                            indexed++;
                                        }
                                        sumCom = closerecords.Sum(e => e.Commission);
                                        sumTaxes = closerecords.Sum(e => e.Taxes);
                                        sumStorage = closerecords.Sum(e => e.Storage);
                                        sumProfit = closerecords.Sum(e => e.Profit);
                                        sbtraded.AppendFormat(@"<tr style='text-align:right;\'>
                                                                                <td colspan='10'>&nbsp;</td>
                                                                                <td class='mspt'>{0}</td>
                                                                                <td class='mspt'>{1}</td>
                                                                                <td class='mspt'>{2}</td>
                                                                                <td class='mspt'>{3}</td>
                                                                            </tr>", sumCom.ToString("f2"),
                                                                                  sumTaxes.ToString("f2"),
                                                                                  sumStorage.ToString("f2"),
                                                                                  sumProfit.ToString("f2"));
                                        sbtraded.AppendFormat(@"<tr>
                                                                                <td colspan='10'>&nbsp;</td>
                                                                                <td colspan='2' align='right'><b>Closed P/L:</b></td>
                                                                                <td colspan='2' align='right' title='Commission + Swap + Profit' class='mspt'><b>{0}</b></td>
                                                                            </tr>", (sumCom + sumStorage + sumProfit).ToString("N2"));
                                        //sbtraded.AppendFormat("<tr><td style='text-align:right;padding:5px' colspan='9'>Total：{0}</td></tr>", closerecords.Sum(e => e.Profit).ToString("f2"));
                                        string content = File.ReadAllText("emailtpl.html").Replace("{name}", accout.Name).
                                                                                           Replace("{account}", accout.Login.ToString()).
                                                                                           Replace("{currency}", accout.Country).
                                                                                           Replace("{leverage}", string.Format("1:{0}", accout.Leverage)).
                                                                                           Replace("{time}", DateTime.Now.ToString()).
                                                                                           Replace("{openrecords}", sbtrading.ToString()).
                                                                                           Replace("{closerecords}", sbtraded.ToString());
                                        MainForm.Emails.Enqueue(new Models.EmailModel("交易记录告知", content, "service@oceanforex.com", "OceanForex Customer Service", accout.Email, accout.Name, "smtp.exmail.qq.com", "service@oceanforex.com", "ocean58"));
                                    }
                                }
                            }
                            dt = DateTime.Parse(string.Format("{0}-{1}-{2} 00:00:00", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1));

                        }
                        catch (Exception ex)
                        {
                            WriteLogs(ex.ToString());
                        }
                    }
                    Thread.Sleep(1000 * 60 * 5);
                }
            });
        }

        public void WriteLogs(string msg)
        {
            this.BeginInvoke((MethodInvoker)(() =>
            {
                this.txtMsg.AppendText(string.Format("{0}:{1}\r\n", DateTime.Now.ToString(), msg));
            }));
        }


    }
}
