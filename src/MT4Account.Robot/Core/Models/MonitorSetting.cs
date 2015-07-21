using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT4Account.Robot.Core.Models
{
    public class MonitorSetting
    {
        //public DateTime DtInputStart { get; set; }
        //public DateTime dtInputEnd { get; set; }
        //public DateTime DtInputFrom { get; set; }
        //public DateTime DtInputTo { get; set; }
        public DateTime DateTimePriceMonStartTime { get; set; }
        public DateTime DateTimePickerMonEndTime { get; set; }

        public double TextBoxMonLots { get; set; }
        public double TextBoxMonRate { get; set; }
        //public string LbAdjustFrom { get; set; }
        //public string LbAdjustTo { get; set; }
        //public bool ChkCycle { get; set; }
    }
}
