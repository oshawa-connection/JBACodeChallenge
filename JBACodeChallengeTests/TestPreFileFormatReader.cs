using System;
using JBACodeChallenge;
using JBACodeChallenge.Models;
using JBACodeInterview.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestsJBACodeInterview.Utilities
{
    [TestClass]
    public class TestPreFileFormatReader
    {
        public TestPreFileFormatReader()
        {
            
        }


        /// <summary>
        /// Tests that the parse header reads key file information correctly.
        /// </summary>
        [TestMethod]
        public void testReadHeaderInfoCorrectly()
        {
            PreFileFormatReader preFileFormatReader = new PreFileFormatReader();

            string[] testingHeaderStringArray = new string[5]
            {
                "Tyndall Centre grim file created on 22.01.2004 at 17:57 by Dr. Tim Mitchell",
                ".pre = precipitation (mm)",
                "CRU TS 2.1",
                "[Long=-180.00, 180.00] [Lati= -90.00,  90.00] [Grid X,Y= 720, 360]",
                "[Boxes=   67420] [Years=1991-2000] [Multi=    0.1000] [Missing=-999]"
            };

            int[] expectedYearRange = new int[2] { 1991, 2000 };
            int expectednumberOfLinesPerBlock = 10;
            preFileFormatReader.parseHeader(testingHeaderStringArray);

            CollectionAssert.AreEqual(expectedYearRange, preFileFormatReader.yearRange,"Year range read incorrectly from header");
            Assert.AreEqual(expectednumberOfLinesPerBlock, preFileFormatReader.numberOfLinesPerBlock,"Number of lines per block read incorrectly from header");
            
        }

        /// <summary>
        /// If the header does not have the correct number of lines, the parse header should
        /// throw an exception.
        /// </summary>
        [TestMethod]
        public void testThrowsOnIncorrectHeader()
        {
            PreFileFormatReader preFileFormatReader = new PreFileFormatReader();

            string[] testingHeaderStringArrayTooShort = new string[4] {
                "Tyndall Centre grim file created on 22.01.2004 at 17:57 by Dr. Tim Mitchell",
                ".pre = precipitation (mm)",
                "[Long=-180.00, 180.00] [Lati= -90.00,  90.00] [Grid X,Y= 720, 360]",
                "[Boxes=   67420] [Years=1991-2000] [Multi=    0.1000] [Missing=-999]"
            };
            int[] expectedYearRange = new int[2] { 1991, 2000 };
            int expectednumberOfLinesPerBlock = 10;
            Assert.ThrowsException<Exception>(() => preFileFormatReader.parseHeader(testingHeaderStringArrayTooShort),
                "Should throw exception if header string is incorrect length"); 

        }

       
        /// <summary>
        /// Tests that data blocks are parsed correctly.
        /// Mainly to make sure that future code changes to this method do not change
        /// important functionality.
        /// </summary>
        [TestMethod]
        public void testparsePreDataBlock()
        {
            PreFileFormatReader preFileFormatReader = new PreFileFormatReader();

            //Load header info before reading.
            string[] testingHeaderStringArray = new string[5]
            {
                "Tyndall Centre grim file created on 22.01.2004 at 17:57 by Dr. Tim Mitchell",
                ".pre = precipitation (mm)",
                "CRU TS 2.1",
                "[Long=-180.00, 180.00] [Lati= -90.00,  90.00] [Grid X,Y= 720, 360]",
                "[Boxes=   67420] [Years=1991-2000] [Multi=    0.1000] [Missing=-999]"
            };

            preFileFormatReader.parseHeader(testingHeaderStringArray);

            int[] gridRefs = new int[2] { 1, 148 };

            string[] blockData = new string[] {
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             " 3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
            };

            RainfallModel[,] rainfallModels = preFileFormatReader.parsePreDataBlock(gridRefs, blockData);

            Assert.AreEqual(rainfallModels.Length, blockData.Length * 12, $"There should be {blockData.Length * 12} rainfall models but {rainfallModels.Length} were created.");

            //Test an example model
            RainfallModel expectedFirstRainfallModel = new RainfallModel() { guid=Guid.NewGuid(), Value= 3020 , Xref=1, Yref= 148, Date= new DateTime(1991,1,1) };

            RainfallModel actualFirstRainfallModel = rainfallModels[0, 0];

            Assert.AreEqual(expectedFirstRainfallModel.Xref, actualFirstRainfallModel.Xref);
            Assert.AreEqual(expectedFirstRainfallModel.Yref, actualFirstRainfallModel.Yref);
            Assert.AreEqual(expectedFirstRainfallModel.Date, actualFirstRainfallModel.Date);
            Assert.AreEqual(expectedFirstRainfallModel.Value, actualFirstRainfallModel.Value);

            RainfallModel expectedLastRowFirstColumnRainfallModel = new RainfallModel() { guid = Guid.NewGuid(), Value = 3020, Xref = 1, Yref = 148, Date = new DateTime(2000, 1, 1) };

            RainfallModel actualLastRowFirstColumnRainfallModel = rainfallModels[0, 9];
            Assert.AreEqual(expectedLastRowFirstColumnRainfallModel.Xref, actualLastRowFirstColumnRainfallModel.Xref);
            Assert.AreEqual(expectedLastRowFirstColumnRainfallModel.Yref, actualLastRowFirstColumnRainfallModel.Yref);
            Assert.AreEqual(expectedLastRowFirstColumnRainfallModel.Value, actualLastRowFirstColumnRainfallModel.Value);
            Assert.AreEqual(expectedLastRowFirstColumnRainfallModel.Date, actualLastRowFirstColumnRainfallModel.Date);
            


        }

        /// <summary>
        /// Check works for variable year range (rows per block)
        /// E.g. if the years were from 2000-2003, we should read 4 lines (rows) per block
        /// </summary>
        [TestMethod]
        public void testParseShorterYearRangePreDataBlock()
        {
            string[] testingHeaderStringArray = new string[5]
            {
                "Tyndall Centre grim file created on 22.01.2004 at 17:57 by Dr. Tim Mitchell",
                ".pre = precipitation(mm)",
                "CRU TS 2.1",
                "[Long = -180.00, 180.00][Lati = -90.00, 90.00][Grid X, Y = 720, 360]",
                "[Boxes = 67420] [Years=2000-2003] [Multi = 0.1000] [Missing = -999]"
            };

            string[] blockDataShort = new string[]
            {
             "3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             "3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             "3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630",
             "3020 2820 3040 2880 1740 1360  980  990 1410 1770 2580 2630"
            };

            
            int[] gridRefs = new int[2] { 1, 148 };

            PreFileFormatReader preFileFormatReader = new PreFileFormatReader();
            preFileFormatReader.parseHeader(testingHeaderStringArray);

            int expectedNumberOfLinesPerBlock = 4;
            Assert.AreEqual(expectedNumberOfLinesPerBlock, preFileFormatReader.numberOfLinesPerBlock,$"Expected ");


            RainfallModel[,] rainfallModels = preFileFormatReader.parsePreDataBlock(gridRefs, blockDataShort);

            int expectedRainfallModelsLength = 4 * 12; // Just to show how I get the value. 4 rows, 12 months.

            Assert.AreEqual(expectedRainfallModelsLength, rainfallModels.Length, $"There should be {expectedRainfallModelsLength} rainfall models but {rainfallModels.Length} were created.");
            

        }
    }
}