using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Oceanforex.ApiService.Services.Interface
{
    [ServiceContract]
    public interface IApiService
    {
        [OperationContract]
        string SendMail(string subject, string body, string form, string fromname, string to, string toname, string smtp, string formuser, string frompwd);
    }
}
