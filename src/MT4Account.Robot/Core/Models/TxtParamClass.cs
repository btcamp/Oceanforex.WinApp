using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT4Account.Robot.Core.Models
{
    public enum TxtParamActionEnum
    {
        Insert = 0, Update = 1, Monitor = 2, IsTraded = 3
    }
    [Serializable]
    public class TxtParamClass
    {
        public TxtParamClass()
        {
            Groups = new List<string>();
        }

        public int MemberId { get; set; }
        public string Name { get; set; }
        public int Login { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Amount { get; set; }
        public double Ratio { get; set; }
        public TxtParamActionEnum Action { get; set; }

        public DateTime MonStart { get; set; }
        public DateTime MonEnd { get; set; }
        public double MonLots { get; set; }
        public double MonRate { get; set; }
        public double ExpectedProfit { get; set; }
        public double FinishedProfit { get; set; }
        public List<string> Groups { get; set; }
    }
}
