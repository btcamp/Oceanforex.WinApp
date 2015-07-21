using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT4Account.Robot.Core.Models
{
    public class PriceModel
    {
        public DateTime Time { get; set; }
        public double Price { get; set; }
        public int Login { get; set; }
    }
}
