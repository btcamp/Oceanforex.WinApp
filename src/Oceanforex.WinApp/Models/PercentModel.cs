using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oceanforex.WinApp.Models
{
    public class PercentModel
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Percent { get; set; }

        public decimal EverydayProfit { get; set; }

        public decimal CurrentProfit { get; set; }
    }
}
