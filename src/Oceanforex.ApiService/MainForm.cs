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

namespace Oceanforex.ApiService
{
    public partial class MainForm : Form
    {
        private static ConcurrentQueue<Models.EmailModel> emails = new ConcurrentQueue<Models.EmailModel>();

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

        public void WriteLogs(string msg)
        {
            this.BeginInvoke((MethodInvoker)(() =>
            {
                this.txtMsg.AppendText(string.Format("{0}:{1}\r\n", DateTime.Now.ToString(), msg));
            }));
        }


    }
}
