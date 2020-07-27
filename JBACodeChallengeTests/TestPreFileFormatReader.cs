﻿using System;
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

            CollectionAssert.AreEqual(expectedYearRange, preFileFormatReader.yearRange,"Year range read incorrectly");
            Assert.AreEqual(expectednumberOfLinesPerBlock, preFileFormatReader.numberOfLinesPerBlock,"Number of lines (years) read incorrectly");
            
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
        /// Tests that 
        /// Mainly to make sure that future changes to this method do not change
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





        }

        /// <summary>
        /// The example data file provided
        /// however, it should also work if a header with a shorter year range (rows)
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
            Assert.AreEqual(expectedNumberOfLinesPerBlock, preFileFormatReader.numberOfLinesPerBlock);


            preFileFormatReader.parsePreDataBlock(gridRefs, blockDataShort);
            

        }
    }
}