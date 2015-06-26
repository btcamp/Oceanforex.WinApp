using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oceanforex.WinApp.Models
{
    public class PriceModel
    {
        public PriceModel(string close, string open, string hight, string low, string time)
        {
            this.Close = double.Parse(close);
            this.Open = double.Parse(open);
            this.High = double.Parse(hight);
            this.Low = double.Parse(low);
            this.Time = DateTime.Parse(time);
        }
        public double Close { get; set; }

        public double Open { get; set; }
        public double High { get; set; }

        public double Low { get; set; }

        public DateTime Time { get; set; }


        public static List<PriceModel> GetPrices(string[] lines)
        {
            List<PriceModel> result = new List<PriceModel>();
            foreach (var item in lines)
            {
                string[] array = item.Split(';');
                if (array.Length == 5)
                {
                    result.Add(new PriceModel(array[0], array[1], array[2], array[3], array[4]));
                }
            }
            return result;
        }
    }

}
