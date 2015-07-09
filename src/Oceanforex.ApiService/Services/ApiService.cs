using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Oceanforex.ApiService.Services
{
    public class ApiService : Interface.IApiService
    {
        public string SendMail(string subject, string body, string from, string fromname, string to, string toname, string smtp, string fromuser, string frompwd)
        {
            MainForm.Emails.Enqueue(new Models.EmailModel(subject, body, from, fromname, to, toname, smtp, fromuser, frompwd));
            MainForm.Instance.WriteLogs(string.Format("接收到发送邮件命令，以放入邮件队列：from:{0}->to:{1}", from, to));
            //Task.Factory.StartNew(() =>
            //{
            //    MailMessage mailMessage = new MailMessage(new MailAddress(from, fromname), new MailAddress(to, toname));
            //    mailMessage.Subject = subject;
            //    mailMessage.Body = body;
            //    mailMessage.IsBodyHtml = true;
            //    //发送html标签
            //    //AlternateView view = AlternateView.CreateAlternateViewFromString(txtContent.Text.Trim() + "<img src=\"cid:myimg\" alt=\"图片\"/>", Encoding.UTF8, "text/html");


            //    ////发送图片
            //    //LinkedResource resource = new LinkedResource(@"E:\Photo\背景02\01.jpg");
            //    //resource.ContentId = "myimg";
            //    //view.LinkedResources.Add(resource);//将图片添加到资源集合中
            //    ////发送附件
            //    //Attachment ment = new Attachment(@"E:\Photo\背景02\01.jpg");
            //    //mailMessage.Attachments.Add(ment);


            //    //mailMessage.AlternateViews.Add(view);
            //    SmtpClient client = new SmtpClient(smtp);
            //    client.Credentials = new NetworkCredential(fromuser, frompwd);
            //    client.Send(mailMessage);

            //});

            return "OK";
        }
    }
}
