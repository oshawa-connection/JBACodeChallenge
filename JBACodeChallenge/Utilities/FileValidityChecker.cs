using System;
using System.IO;

namespace JBACodeInterview.Utilities
{
    public class FileValidityChecker
    {

        /// <summary>
        /// Checks the specified file name of the input filepath for existance
        /// and correct file extension.
        /// </summary>
        /// <param name="fileName">The path to the input .pre file, including filename and extension</param>
        public void checkPreFileValidity(string fileName)
        {
            string preFileExtension = ".pre";
            if (!File.Exists(fileName))
            {
                throw new Exception("File not found");
            }
            

            string fileExtension = Path.GetExtension(fileName);

            if (fileExtension != ".pre")
            {
                throw new Exception($"Incorrect file extension {fileExtension}. Expected {preFileExtension}");
            }

        }

        /// <summary>
        /// Checks specified folder of the database exists and if so,
        /// that the file specified has the correct extension.
        /// </summary>
        /// <param name="databasePath">The path to the output database, including filename and extension</param>
        public void checkDatabaseFileValidity(string databasePath)
        {
            string sqliteFileExtension = ".db";

            // We will create the database if it doesn't exist anyway
            if (!Directory.Exists(Path.GetDirectoryName(databasePath)))
            {
                throw new Exception("Incorrect path to database.");
            }

            string databaseFileExtension = Path.GetExtension(databasePath);
            if (Path.GetExtension(databasePath) != sqliteFileExtension)
            {
                throw new Exception($"Incorrect file extension {databaseFileExtension}. Expected {sqliteFileExtension}");
            }
        }
    }
    
}
