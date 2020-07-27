using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace JBACodeChallenge.Models
{
    public class JBADatabaseContext : DbContext
    {
        private string databaseConnectionString;
        public JBADatabaseContext(string databaseConnectionString)
        {
            this.databaseConnectionString = databaseConnectionString;
        }
        public DbSet<RainfallModel> RainfallMeasurementModels { get; set; }

        /// <summary>
        /// Used if you want to change and encrypt the SQLite database.
        /// </summary>
        /// <returns></returns>
        private string BuildConnectionString()
        {
            SqliteConnectionStringBuilder sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder();
            sqliteConnectionStringBuilder.DataSource = this.databaseConnectionString;
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
            options.UseSqlite(connectionString);
        }
    }
}
