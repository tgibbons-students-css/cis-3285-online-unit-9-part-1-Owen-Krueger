using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SingleResponsibilityPrinciple
{
    public class TradeProcessor
    {
        //Read Trade Data from local file
        private IEnumerable<string> ReadFileTradeData(Stream stream)
        {
            var tradeData = new List<string>();
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    tradeData.Add(line);
                }
            }
            return tradeData;
        }

        private IEnumerable<TradeRecord> ParseTrades(IEnumerable<string> tradeData)
        {
            var trades = new List<TradeRecord>();
            var lineCount = 1;
            foreach (var line in tradeData)
            {
                var fields = line.Split(new char[] { ',' });

                if (!ValidateTradeData(fields, lineCount))
                {
                    continue;
                }

                var trade = MapTradeDataToTradeRecord(fields);

                trades.Add(trade);

                lineCount++;
            }

            return trades;
        }

        private bool ValidateTradeData(string[] fields, int currentLine)
        {
            if (fields.Length != 3)
            {
                LogMessage("WARN", " Line {0} malformed. Only {1} field(s) found.", currentLine, fields.Length);
                return false;
            }

            if (fields[0].Length != 6)
            {
                LogMessage("WARN", " Trade currencies on line {0} malformed: '{1}'", currentLine, fields[0]);
                return false;
            }

            int tradeAmount;
            if (!int.TryParse(fields[1], out tradeAmount))
            {
                LogMessage("WARN", " Trade amount on line {0} not a valid integer: '{1}'", currentLine, fields[1]);
                return false;
            }
            else if(tradeAmount < 1000 || tradeAmount > 100000) //If lot size is under 1000 or more than 100000, throw error
            {
                LogMessage("WARN", " Trade amount on line {0} is not between 1,000 and 10,000 units: '{1}'", currentLine, fields[1]);
                return false;
            }

            decimal tradePrice;
            if (!decimal.TryParse(fields[2], out tradePrice))
            {
                LogMessage("WARN"," Trade price on line {0} not a valid decimal: '{1}'", currentLine, fields[2]);
                return false;
            }

            return true;
        }

        private void LogMessage(string msgType, string message, params object[] args)
        {
            Console.WriteLine(msgType+ " :" +message, args);
            using (StreamWriter w = File.AppendText("log.xml")) //Log information to an xml file
            {
                w.WriteLine("<log><type>" + msgType + "</type><message>" + message + "</message>"); //message to write out
            }
        }

        private TradeRecord MapTradeDataToTradeRecord(string[] fields)
        {
            var sourceCurrencyCode = fields[0].Substring(0, 3);
            var destinationCurrencyCode = fields[0].Substring(3, 3);
            var tradeAmount = int.Parse(fields[1]);
            var tradePrice = decimal.Parse(fields[2]);
            const float LotSize = 100000f;
            var trade = new TradeRecord
            {
                SourceCurrency = sourceCurrencyCode,
                DestinationCurrency = destinationCurrencyCode,
                Lots = tradeAmount / LotSize,
                Price = tradePrice
            };

            return trade;
        }

        private void StoreTrades(IEnumerable<TradeRecord> trades)
        {
            LogMessage("INFO", "  Connecting to Database");
            //using (var connection = new System.Data.SqlClient.SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\tradedatabase.mdf;Integrated Security=True;Connect Timeout=30;"))
            //Connection to athena.css.edu database instead of local file
            string connectSqlServer = "Data Source = athena.css.edu; Initial Catalog = CIS3285; Persist Security Info = True; User ID = tgibbons; Password = Data Source = athena.css.edu; Initial Catalog = CIS3285; Persist Security Info = True; User ID = tgibbons; Password = Saints4CSS";
            using (var connection = new System.Data.SqlClient.SqlConnection(connectSqlServer))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var trade in trades)
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = "dbo.insert_trade";
                        command.Parameters.AddWithValue("@sourceCurrency", trade.SourceCurrency);
                        command.Parameters.AddWithValue("@destinationCurrency", trade.DestinationCurrency);
                        command.Parameters.AddWithValue("@lots", trade.Lots);
                        command.Parameters.AddWithValue("@price", trade.Price);

                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                connection.Close();
            }

            LogMessage("INFO", "  {0} trades processed", trades.Count());
        }

        public void ProcessTrades(Stream stream)
        {
            var lines = ReadFileTradeData(stream);
            var trades = ParseTrades(lines);
            StoreTrades(trades);
        }

        //Process Trades from a URL reference
        public void ProcessURLTrades(string url)
        {
            var lines = ReadURLTradeData(url);
            var trades = ParseTrades(lines);
            StoreTrades(trades);
        }

        //Read Trades from a URL reference
        public List<string> ReadURLTradeData(string url)
        {
            var tradeData = new List<string>();
            // create a web client and use it to read the file stored at the given URL
            var client = new WebClient();
            using (var stream = client.OpenRead(url))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    tradeData.Add(line);
                }
            }
            
            return tradeData;
        }


    }
}
