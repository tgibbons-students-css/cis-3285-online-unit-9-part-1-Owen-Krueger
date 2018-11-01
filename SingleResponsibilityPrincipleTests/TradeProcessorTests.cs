using Microsoft.VisualStudio.TestTools.UnitTesting;
using SingleResponsibilityPrinciple;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SingleResponsibilityPrinciple.Tests
{
    [TestClass()]
    public class TradeProcessorTests
    {

        private int CountDbRecords()
        {
            //using (var connection = new System.Data.SqlClient.SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\tradedatabase.mdf;Integrated Security=True;Connect Timeout=30;"))
            string connectSqlServer = "Data Source = athena.css.edu; Initial Catalog = CIS3285; Persist Security Info = True; User ID = tgibbons; Password = Data Source = athena.css.edu; Initial Catalog = CIS3285; Persist Security Info = True; User ID = tgibbons; Password = Saints4CSS";
            using (var connection = new System.Data.SqlClient.SqlConnection(connectSqlServer))
            {
                connection.Open();
                string myScalarQuery = "select count(*) from trade";
                SqlCommand myCommand = new SqlCommand(myScalarQuery, connection);
                int count = (int)myCommand.ExecuteScalar();
                connection.Close();
                return count;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NoTestFile()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("");
            var tradeProcessor = new TradeProcessor();

            //Act
            tradeProcessor.ProcessTrades(tradeStream);

            //Assert

        }

        [TestMethod]
        //[ExpectedException(typeof(ArgumentNullException))]
        public void EmptyTestFile()
        {
            //Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.tradesEmpty.txt");
            var tradeProcessor = new TradeProcessor();

            //Act
            tradeProcessor.ProcessTrades(tradeStream);

            //Assert

        }
        [TestMethod]
        public void Test4tradesInFile()
        {
            // Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.trades4.txt");
            var tradeProcessor = new TradeProcessor();
            int startCount = CountDbRecords();
            // Act
            tradeProcessor.ProcessTrades(tradeStream);
            int endCount = CountDbRecords();
            // Assert
            Assert.AreEqual(endCount - startCount, 4);
        }

        [TestMethod]
        public void TestWronglyFormattedTrades()
        {
            // Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.tradesBadFormats.txt");
            var tradeProcessor = new TradeProcessor();
            int startCount = CountDbRecords();
            // Act
            tradeProcessor.ProcessTrades(tradeStream);
            int endCount = CountDbRecords();
            // Assert
            Assert.AreEqual(endCount - startCount, 0);
        }


        [TestMethod]
        public void TestTradeAmountBounds()
        {
            // Arrange
            var tradeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SingleResponsibilityPrincipleTests.tradesBounds.txt");
            var tradeProcessor = new TradeProcessor();
            int startCount = CountDbRecords();
            // Act
            tradeProcessor.ProcessTrades(tradeStream);
            int endCount = CountDbRecords();
            // Assert
            Assert.AreEqual(endCount - startCount, 2);
        }

    }
}