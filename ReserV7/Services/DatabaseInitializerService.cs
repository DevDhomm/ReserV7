using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ReserV7.Data;
using System.IO;
using System.Reflection;

namespace ReserV7.Services
{
    public class DatabaseInitializerService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _dbPath;

        public DatabaseInitializerService(ApplicationDbContext context)
        {
            _context = context;
            _dbPath = Path.Combine(AppContext.BaseDirectory, "app.db");
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Check if database file exists
                bool databaseExists = File.Exists(_dbPath);

                if (!databaseExists)
                {
                    // Database doesn't exist, create and initialize it with SQL script
                    System.Diagnostics.Debug.WriteLine("Database not found. Creating new database from SQL script...");
                    await InitializeFromSqlScriptAsync();
                }
                else
                {
                    // Database exists, ensure it's accessible
                    await _context.Database.CanConnectAsync();
                    System.Diagnostics.Debug.WriteLine("Database already exists. Using existing database.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        private async Task InitializeFromSqlScriptAsync()
        {
            try
            {
                // Load SQL script from embedded resources
                var assembly = Assembly.GetExecutingAssembly();
                const string sqlScriptResourceName = "ReserV7.Assets.initialize_database.sql";

                using (var stream = assembly.GetManifestResourceStream(sqlScriptResourceName))
                {
                    if (stream == null)
                    {
                        throw new FileNotFoundException($"SQL script resource not found: {sqlScriptResourceName}");
                    }

                    using (var reader = new StreamReader(stream))
                    {
                        string sqlScript = await reader.ReadToEndAsync();

                        // Execute SQL script against SQLite database
                        using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
                        {
                            await connection.OpenAsync();

                            // Disable foreign key constraints during initialization
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "PRAGMA foreign_keys = OFF;";
                                await command.ExecuteNonQueryAsync();
                            }

                            try
                            {
                                // Split script by semicolons and execute each statement
                                var statements = sqlScript.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                                foreach (var statement in statements)
                                {
                                    var trimmedStatement = statement.Trim();
                                    if (string.IsNullOrWhiteSpace(trimmedStatement))
                                        continue;

                                    using (var command = connection.CreateCommand())
                                    {
                                        command.CommandText = trimmedStatement;
                                        await command.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                            finally
                            {
                                // Re-enable foreign key constraints
                                using (var command = connection.CreateCommand())
                                {
                                    command.CommandText = "PRAGMA foreign_keys = ON;";
                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("Database successfully initialized from SQL script.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing database from SQL script: {ex.Message}");
                throw;
            }
        }
    }
}
