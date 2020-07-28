using System;
using System.IO;
using System.Threading.Tasks;
using JBACodeChallenge.Models;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JBACodeInterview.Utilities;
using Microsoft.EntityFrameworkCore;

namespace JBACodeChallenge
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new Exception($"Incorrect number of arguements. " +
                    $"Expected {2} but got {args.Length}");
            }

            FileValidityChecker fileValidityChecker = new FileValidityChecker();

            string databasePath = args[0]; // The path to the database file.
            string preFilePath = args[1]; //The path to the .pre data file.

            fileValidityChecker.checkDatabaseFileValidity(databasePath);
            fileValidityChecker.checkPreFileValidity(preFilePath);

            // Build dependent classes
            PreFileFormatReader preFileFormatReader = new PreFileFormatReader();

            List<Task> taskList = new List<Task>();
            int numberOfBlocksWritten = 0;

            // .pre files have a header with a fixed number of lines.
            string[] headerText = new string[preFileFormatReader.numberOfHeaderLines];

            // .pre files are comprised of two parts: the block header and the block.
            // The "block" is a two dimensional array of integers. 
            // The blockHeader contains information unique to the succeding block.
            string blockHeader = String.Empty; // e.g. grid-ref = 1, 138
            string[] blockData; // This stores the unparsed block data.
            

            // Creates the database if it doesn't exist.
            using (var db = new JBADatabaseContext(databasePath))
            {
                db.Database.EnsureCreated();
            }

            
            using (var db = new JBADatabaseContext(databasePath))
            using (var transaction = db.Database.BeginTransaction())
            using (StreamReader streamReader = new StreamReader(preFilePath))
            {
                try
                {
                    // Read in file header info
                    for (int i = 0; i < preFileFormatReader.numberOfHeaderLines; i++)
                    {
                        headerText[i] = streamReader.ReadLine();
                    }

                    // parse file header info and prepare the preFileFormatReader for further parsing.
                    preFileFormatReader.parseHeader(headerText);
                    // pre-allocate blockData array.
                    blockData = new string[preFileFormatReader.numberOfLinesPerBlock];

                    //Read block headInfo
                    while ((blockHeader = streamReader.ReadLine()) != null)
                    {
                        // Parse the block header and get the grid refs.
                        int[] gridRefs = preFileFormatReader.parsePreBlockHeader(blockHeader); //returns array of length 2.
                        for (int i = 0; i < preFileFormatReader.numberOfLinesPerBlock; i++)
                        {
                            blockData[i] = streamReader.ReadLine();
                        }

                        //Parse the string array into the structure that the database expects.
                        RainfallModel[,] rainfallModels = preFileFormatReader.parsePreDataBlock(gridRefs, blockData);

                        //For every rainfallModel record in the array, stage it for loading into the database 
                        foreach (var rainfallMeasurementModel in rainfallModels)
                        {
                            db.RainfallMeasurementModels.Add(rainfallMeasurementModel);
                        }

                        numberOfBlocksWritten++;
                        
                    }

                }
                catch (Exception exception)
                {
                    Console.WriteLine("An exception occurred. No transactions will be written.");
                    Console.WriteLine($"Exception: {exception.Message}");
                    //transaction.Rollback(); // not needed
                    throw exception; // rethrow
                }

                db.SaveChanges();
                Console.WriteLine($"Sucessfully parsed {numberOfBlocksWritten} blocks. Committing transaction.");
                transaction.Commit();
                db.Database.CloseConnection(); 
            }

            Console.WriteLine($"Sucessfully read {preFilePath} into {databasePath}. Press enter to exit program.");
            Console.ReadLine();

        }
    }
}