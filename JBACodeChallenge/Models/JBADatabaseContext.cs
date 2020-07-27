using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace JBACodeChallenge.Models
{
    public class JBADatabaseContext : DbContext
    {
        public DbSet<RainfallModel> RainfallMeasurementModels { get; set; }

                /// <summary>
        /// Checks if database filepath is valid.
        /// </summary>
        /// <returns></returns>
        private bool IsValid(string connectionString)
        {
            return true;
        }

        /// <summary>
        /// Used if you want to change and encrypt the SQLite database.
        /// </summary>
        /// <returns></returns>
        private static string BuildConnectionString()
        {
            SqliteConnectionStringBuilder sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder();
            sqliteConnectionStringBuilder.DataSource = @"C:\Users\James\source\repos\JBACodeChallenge\JBACodeChallenge\files\JBATestDatabase.db";
            Console.WriteLine(sqliteConnectionStringBuilder.ToString());
            return sqliteConnectionStringBuilder.ToString();

        }
        /// <summary>
        /// Or change so that we can provide our own connection string.
        /// </summary>
        /// <param name="options"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string connectionString = BuildConnectionString();
            if (!this.IsValid(connectionString)) throw new Exception("Database path not valid");
            options.UseSqlite(connectionString);
        }
    }
}
