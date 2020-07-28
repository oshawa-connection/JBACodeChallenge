using System;
using System.IO;
using System.Linq;
using JBACodeChallenge.Models;

namespace JBACodeChallenge
{
    public class PreFileFormatReader
    {
        private int[] monthNumbers = new int[12] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        public int numberOfLinesPerBlock; // difference between yearRange values.
        public int numberOfHeaderLines; // The number of lines at the top of the file dedicated to the header. Assume fixed.
        public int[] yearRange; // An array of 2 integers representing the year range from the file header, e.g. 1992, 2002
        private int[] yearsArray; //An array representing all years between the two years specified in yearRange.
        private DateTime[,] dateTimeArray; // A 2d array representing the date associatted with each value in the data block.


        public PreFileFormatReader()
        {
            numberOfHeaderLines = 5; 
            yearRange = new int[2]; 
        }


        /// <summary>
        /// Parses the important information from the header of the .pre
        /// file to allow for reading of the rest of the file.
        /// </summary>
        /// <param name="headerText">A string array of the header text.</param>
        public void parseHeader(string[] headerText)
        {
            int numberOfReadHeaderLines = headerText.Length;
            if (numberOfReadHeaderLines != this.numberOfHeaderLines)
            {
                throw new Exception($"Incorrect number of header lines read. " +
                    $"Expected {this.numberOfHeaderLines} but got {numberOfReadHeaderLines}");
            }
            // I HATE regex. I'm guessing LINQ has string manipulation methods.

            string headerLine = headerText[numberOfHeaderLines - 1];

            string[] headerInfos = headerLine.Split("[");

            string yearHeaderInfo = headerInfos[2];
            yearHeaderInfo = yearHeaderInfo.Trim(' ');
            yearHeaderInfo = yearHeaderInfo.Trim(']');
            yearHeaderInfo = yearHeaderInfo.Substring("Years=".Length);
            string[] stringYears = yearHeaderInfo.Split("-");

            if (stringYears.Length != 2)
            {
                throw new Exception($"Incorrect number of years in header. " +
                    $"Expected 2 but got {stringYears.Length}");
            }

            this.yearRange[0] = int.Parse(stringYears[0]);
            this.yearRange[1] = int.Parse(stringYears[1]);

            this.yearsArray = this.generateArrayOfYears(this.yearRange);
            createMonthsAndYearsArray(this.yearsArray);

            //In case years are provided non-chronologicaly first.
            this.numberOfLinesPerBlock = Math.Abs((this.yearRange[1] - this.yearRange[0])) + 1; //inclusive

            Console.WriteLine($"Reading {this.numberOfLinesPerBlock} lines per block.");

        }


        /// <summary>
        /// Each "block" of data has info on the grid-ref above it,
        /// e.g Grid-ref=   1, 148
        /// This data has to be inserted into the database.
        /// </summary>
        /// <param name="blockHeader">E.g. Grid-ref=   1, 148</param>
        /// <returns>Integer array of grid references</returns>
        public int[] parsePreBlockHeader(string blockHeader)
        {
            blockHeader = blockHeader.Substring("Grid-ref= ".Length);
            blockHeader = blockHeader.Replace(" ", ""); //Remove all spaces
            string[] splitGridRefStrings = blockHeader.Split(',');

            if (splitGridRefStrings.Length != 2)
            {
                throw new Exception($"Incorrect number of years in block header. " +
                    $"Expected 2 but got {splitGridRefStrings.Length}");
            }

            int[] gridRefs = new int[2];

            gridRefs[0] = int.Parse(splitGridRefStrings[0]);
            gridRefs[1] = int.Parse(splitGridRefStrings[1]);

            return gridRefs;
        }


        /// <summary>
        /// Parse a block of the pre data and uses it to create an array of RainfallModels.
        /// It also uses dateTimeArray to add dates to the Models.
        /// </summary>
        /// <param name="gridRefs">An array of 2 integers representing the grid refs for the block E.g. [1,148]</param>
        /// <param name="blockData">A string array representing the block of data.</param>
        public RainfallModel[,] parsePreDataBlock(int[] gridRefs, string[] blockData)
        {
            // validate inputs
            if (gridRefs.Length != 2)
            {
                throw new Exception($"Incorrect number of gridRefs." +
                    $"Expected 2 but got {gridRefs.Length}");
            }

            if (blockData.Length != this.numberOfLinesPerBlock)
            {
                throw new Exception($"Incorrect number of lines in block." +
                    $"Expected {this.numberOfLinesPerBlock} but got {blockData.Length}");
            }

            int[,] blockDataParsed = new int[12, blockData.Length]; // 12 Months per year
            RainfallModel[,] rainfallMeasurements = new RainfallModel[12, blockData.Length];

            // Parse each block into a two dimensional array.
            // COmbine these two loops
            for (int yearIndex=0; yearIndex< blockData.Length; yearIndex++)
            {
                blockData[yearIndex] = blockData[yearIndex].Trim(' ');

                string[] splitLine = blockData[yearIndex].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                for(int monthIndex=0; monthIndex< splitLine.Length; monthIndex++)
                {
                    blockDataParsed[monthIndex, yearIndex] = int.Parse(splitLine[monthIndex]);
                    rainfallMeasurements[monthIndex, yearIndex] = new RainfallModel()
                    {
                        guid = Guid.NewGuid(),
                        Date = this.dateTimeArray[monthIndex, yearIndex],
                        Xref = gridRefs[0],
                        Yref = gridRefs[1],
                        Value = blockDataParsed[monthIndex, yearIndex]
                    };
                }
            }
            return rainfallMeasurements;
        }


        /// <summary>
        /// Given an array of two integers, generates a sorted array containing all
        /// numbers in the range between the two, inclusive of the final year.
        /// Potentially this could be moved to a another/ static class.
        /// </summary>
        /// <param name="yearRange">The range of years. e.g. [1991, 2000] to represent 1991 to 2000.</param>
        /// <returns>A sorted array of the numbers in the range between the two numbers, inclusive.</returns>
        private int[] generateArrayOfYears(int[] yearRange)
        {
            //In case years are in reverse order, use Absolute value
            int numberOfYears = Math.Abs((yearRange[0] - yearRange[1])) + 1;

            int lowestYear = yearRange.Min();

            int[] yearsArray = new int[numberOfYears];

            for(int yearsArrayIndex=0; yearsArrayIndex < numberOfYears; yearsArrayIndex++)
            {
                yearsArray[yearsArrayIndex] = lowestYear + yearsArrayIndex;
            }

            return yearsArray;
        }

        /// <summary>
        /// Given an array representing years, this function returns a 2d array of months and years. The day field will always be the first day of the month.
        /// This is quite difficult to explain in words so here is a picture
        /// e.g. joinMonthsAndYears([1995,1998]) -> [1995/01/01, 1995/02/01          ...        1995/12/01]
        ///                                                  .              .
        ///                                                  .                 .
        ///                                                                       .
        ///                                         [1998/01/01                 ...  1996/11/01,1996/12/01] 
        /// Potentially this could be moved to a another/ static class.
        /// Beware of American date formatting when it prints
        /// TODO: Give this guy a better name
        /// </summary>
        /// <param name="yearsArray">An array of integers representing years.</param>
        private void createMonthsAndYearsArray(int[] yearsArray)
        {
            int[] monthNumbers = new int[12] { 1, 2, 3, 4, 5 ,6,7,8,9,10,11,12};
            
            var dateEnumerable = from month in monthNumbers
                             from year in yearsArray
                             select new DateTime(year, month, 1);

            //DateTime[] dateTimes = dateEnumerable.ToArray<DateTime>();

            DateTime[,] returnDateTimeArray = new DateTime[monthNumbers.Length, yearsArray.Length];

            for (int yearIndex=0;yearIndex< yearsArray.Length;yearIndex++)
            {
                for(int monthIndex=0;monthIndex<monthNumbers.Length;monthIndex++)
                {
                    returnDateTimeArray[monthIndex, yearIndex] = new DateTime(yearsArray[yearIndex], monthNumbers[monthIndex], 1);
                }
                
            }
            this.dateTimeArray = returnDateTimeArray;
        }
    }
}
