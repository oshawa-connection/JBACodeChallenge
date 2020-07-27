using System;
using System.IO;
using System.Threading.Tasks;
using JBACodeChallenge.Models;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JBACodeInterview.Utilities;

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

            string databasePath = args[0];
            string preFilePath = args[1];

            fileValidityChecker.checkDatabaseFileValidity(databasePath);
            fileValidityChecker.checkPreFileValidity(preFilePath);

            // Build dependent classes
            PreFileFormatReader preFileFormatReader = new PreFileFormatReader();

            List<Task> taskList = new List<Task>();
            int numberOfBlocksWritten = 0;

            string blockHeader = String.Empty;
            string[] blockData;
            string[] headerText = new string[preFileFormatReader.numberOfHeaderLines];


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
                    
                    
                    // Read in header info
                    for (int i = 0; i < preFileFormatReader.numberOfHeaderLines; i++)
                    {
                        headerText[i] = streamReader.ReadLine();
                    }

                    // parse header info
                    preFileFormatReader.parseHeader(headerText);

                    blockData = new string[preFileFormatReader.numberOfLinesPerBlock];

                    //Read block headInfo
                    while ((blockHeader = streamReader.ReadLine()) != null)
                    {
                        int[] gridRefs = preFileFormatReader.parsePreBlockHeader(blockHeader); //returns array of length 2.
                        for (int i = 0; i < preFileFormatReader.numberOfLinesPerBlock; i++)
                        {
                            blockData[i] = streamReader.ReadLine();
                        }

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
                    //transaction.Rollback();
                    // Cancel all tasks.
                    throw exception; // rethrow
                }

                db.SaveChanges();

                transaction.Commit();

            }

            Console.WriteLine($"Sucessfully read {preFilePath} into {databasePath}. Press enter to exit program.");
            Console.ReadLine();

        }
    }
}