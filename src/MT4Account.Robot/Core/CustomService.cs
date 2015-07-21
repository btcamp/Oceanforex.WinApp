using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT4Account.Robot.Core
{
    public class CustomService
    {
        private static readonly PumpingService.IPumpService service = new PumpingService.PumpServiceClient("BasicHttpBinding_IPumpService");
        public static PumpingService.IPumpService Service
        {
            get
            {
                return service;
            }
        }
    }
}
