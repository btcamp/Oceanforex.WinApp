using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oceanforex.ApiService.Models
{
    public class EmailModel
    {
        public EmailModel(string subject, string body, string from, string fromname, string to, string toname, string smtp, string fromuser, string frompwd)
        {
            this.subject = subject;
            this.body = body;
            this.from = from;
            this.fromname = fromname;
            this.to = to;
            this.toname = toname;
            this.smtp = smtp;
            this.fromuser = fromuser;
            this.frompwd = frompwd;
        }

        public string subject { get; set; }
        public string body { get; set; }
        public string from { get; set; }
        public string fromname { get; set; }
        public string to { get; set; }

        public string toname { get; set; }
        public string smtp { get; set; }
        public string fromuser { get; set; }
        public string frompwd { get; set; }
    }
}