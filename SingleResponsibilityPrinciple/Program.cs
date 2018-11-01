using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SingleResponsibilityPrinciple
{
    class Program
    {
        static void Main(string[] args)
        {
            //Url to get trade data from
            string url = "http://faculty.css.edu/tgibbons/trades4.txt";

            var tradeProcessor = new TradeProcessor();
            
            //Process trade data from url
            tradeProcessor.ProcessURLTrades(url);

            Console.ReadKey();
        }
    }
}
