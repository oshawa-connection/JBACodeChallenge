using System;
using System.IO;
using System.Threading.Tasks;
using JBACodeChallenge.Models;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

            string databasePath = args[0];
            string preFilePath = args[1];

            // What if Years=1991-2000 changes? Array size.

            // Build dependent classes
            PreFileFormatReader preFileFormatReader = new PreFileFormatReader();

            List<Task> taskList = new List<Task>();
            int numberOfBlocksWritten = 0;

            string blockHeader = String.Empty;
            string[] blockData;
            string[] headerText = new string[preFileFormatReader.numberOfHeaderLines];
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            using (var db = new JBADatabaseContext())
            {
                db.Database.EnsureCreated();
            }

            
            

            using (var db = new JBADatabaseContext())
            using (var transaction = db.Database.BeginTransaction())
            using (StreamReader streamReader = new StreamReader(preFilePath))
            {
                try
                {
                    //db.Database.EnsureCreated(); //creates if not found.
                    Console.WriteLine("Database created");
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
                    //transaction.Rollback();
                    // Cancel all tasks.
                    throw exception; // rethrow
                }

                db.SaveChanges();

                transaction.Commit();

            }

            Console.WriteLine($"Sucessfully read {preFilePath} into {databasePath}. Press enter to exit.");
            watch.Stop();
            Console.WriteLine($"Execution time: {watch.ElapsedMilliseconds} ms");
            Console.ReadLine();

        }
    }
}