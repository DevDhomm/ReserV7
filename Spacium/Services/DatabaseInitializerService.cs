using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Spacium.Data;
using Spacium.Models;
using System.IO;
using System.Reflection;

namespace Spacium.Services
{
    public class DatabaseInitializerService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _dbPath;

        public DatabaseInitializerService(ApplicationDbContext context)
        {
            _context = context;
            // recherche du chemin d'accès à la base de données dans le dossier AppData pour éviter les problèmes de permissions
            _dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Spacium",
                "app.db"
            );
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Check if database file exists
                bool databaseExists = File.Exists(_dbPath);
                Directory.CreateDirectory(Path.GetDirectoryName(_dbPath));

                if (!databaseExists)
                {
                    // Database doesn't exist, create and initialize it
                    System.Diagnostics.Debug.WriteLine("Database not found. Creating new database...");

                    // First, ensure all migrations are applied (creates schema)
                    await _context.Database.MigrateAsync();

                    // Then initialize with SQL script data
                    await InitializeFromSqlScriptAsync();

                    System.Diagnostics.Debug.WriteLine("Database successfully created with migrations and data.");
                }
                else
                {
                    // Database exists, ensure it's accessible and migrations are up to date
                    await _context.Database.CanConnectAsync();
                    await _context.Database.MigrateAsync();
                    System.Diagnostics.Debug.WriteLine("Database already exists. Migrations applied.");
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
                // Load SQL script from embedded resources (only for compatibility, data seeding is now handled by EF Core)
                var assembly = Assembly.GetExecutingAssembly();
                const string sqlScriptResourceName = "Spacium.Assets.initialize_database.sql";

                using (var stream = assembly.GetManifestResourceStream(sqlScriptResourceName))
                {
                    if (stream == null)
                    {
                        // Script not found, but we can still seed using EF Core
                        System.Diagnostics.Debug.WriteLine("SQL script resource not found. Seeding data using EF Core...");
                        await SeedDataAsync();
                        return;
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
                                        try
                                        {
                                            await command.ExecuteNonQueryAsync();
                                        }
                                        catch (Exception ex)
                                        {
                                            // Ignore errors for already existing tables/views
                                            System.Diagnostics.Debug.WriteLine($"SQL statement skipped (likely already exists): {ex.Message}");
                                        }
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

                // Refresh EF Core context to load data
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");

                System.Diagnostics.Debug.WriteLine("Database successfully initialized from SQL script.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing database from SQL script: {ex.Message}");
                throw;
            }
        }

        private async Task SeedDataAsync()
        {
            try
            {
                // Check if data already exists
                if (await _context.Users.CountAsync() > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Database already seeded. Skipping data insertion.");
                    return;
                }

                // Seed Users
                var users = new List<User>
                {
                    new User { Id = 1, Username = "user1", Email = "user1@ecole.fr", Password = "user1", Role = "User", Nom = "Utilisateur 1", DateCreation = DateTime.UtcNow },
                    new User { Id = 2, Username = "gestionnaire", Email = "gestionnaire@ecole.fr", Password = "gestionnaire", Role = "Gestionnaire", Nom = "Gestionnaire", DateCreation = DateTime.UtcNow }
                };
                await _context.Users.AddRangeAsync(users);
                await _context.SaveChangesAsync();

                // Seed Salles
                var salles = new List<Salle>
                {
                    new Salle { Id = 1, Nom = "Salle 0-1", Description = "Salle de cours standard", Capacite = 40, Type = "Salle de cours", Etage = 0, Disponibilite = true, DateCreation = DateTime.UtcNow },
                    new Salle { Id = 2, Nom = "Salle 0-3", Description = "Salle de cours standard", Capacite = 70, Type = "Salle de cours", Etage = 0, Disponibilite = true, DateCreation = DateTime.UtcNow },
                    new Salle { Id = 3, Nom = "Salle 0-4", Description = "Salle de cours standard", Capacite = 50, Type = "Salle de cours", Etage = 0, Disponibilite = true, DateCreation = DateTime.UtcNow },
                    new Salle { Id = 4, Nom = "Laboratoire informatique", Description = "Laboratoire informatique", Capacite = 25, Type = "Laboratoire", Etage = 0, Disponibilite = true, DateCreation = DateTime.UtcNow },
                    new Salle { Id = 5, Nom = "Salle de langue", Description = "Salle de cours", Capacite = 25, Type = "Salle de cours", Etage = 0, Disponibilite = true, DateCreation = DateTime.UtcNow },
                    new Salle { Id = 6, Nom = "Salle 1-3", Description = "Salle de cours", Capacite = 80, Type = "Salle de cours", Etage = 1, Disponibilite = true, DateCreation = DateTime.UtcNow },
                    new Salle { Id = 7, Nom = "Grande salle", Description = "Salle de cours et reunion", Capacite = 200, Type = "Salle de réunion et de cours", Etage = 1, Disponibilite = true, DateCreation = DateTime.UtcNow },
                    new Salle { Id = 8, Nom = "Salle 1-4", Description = "Espace salle de cours", Capacite = 20, Type = "Salle de travail collaboratif", Etage = 1, Disponibilite = true, DateCreation = DateTime.UtcNow },
                    new Salle { Id = 9, Nom = "Salle 2-6", Description = "Salle de cours standard", Capacite = 70, Type = "Salle de cours", Etage = 2, Disponibilite = true, DateCreation = DateTime.UtcNow },
                    new Salle { Id = 10, Nom = "Salle 2-5", Description = "Salle de cours standard", Capacite = 70, Type = "Salle de cours", Etage = 0, Disponibilite = true, DateCreation = DateTime.UtcNow }
                };
                await _context.Salles.AddRangeAsync(salles);
                await _context.SaveChangesAsync();

                // Seed Creneaux
                var creneaux = new List<Creneau>
                {
                    new Creneau { Id = 1, Debut = TimeSpan.Parse("08:00:00"), Fin = TimeSpan.Parse("10:00:00"), DateCreation = DateTime.UtcNow },
                    new Creneau { Id = 2, Debut = TimeSpan.Parse("10:00:00"), Fin = TimeSpan.Parse("12:00:00"), DateCreation = DateTime.UtcNow },
                    new Creneau { Id = 3, Debut = TimeSpan.Parse("12:00:00"), Fin = TimeSpan.Parse("14:00:00"), DateCreation = DateTime.UtcNow },
                    new Creneau { Id = 4, Debut = TimeSpan.Parse("14:00:00"), Fin = TimeSpan.Parse("16:00:00"), DateCreation = DateTime.UtcNow },
                    new Creneau { Id = 5, Debut = TimeSpan.Parse("16:00:00"), Fin = TimeSpan.Parse("18:00:00"), DateCreation = DateTime.UtcNow }
                };
                await _context.Creneaux.AddRangeAsync(creneaux);
                await _context.SaveChangesAsync();

                // Seed Equipements
                var equipements = new List<Equipement>
                {
                    new Equipement { Id = 1, Nom = "Vidéoprojecteur HD", Description = "Projecteur haute définition", Type = "Vidéoprojecteur", EstFonctionnel = true, SalleId = 1, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 2, Nom = "Tableau blanc interactif", Description = "Écran tactile 65 pouces", Type = "Tableau interactif", EstFonctionnel = true, SalleId = 1, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 3, Nom = "Caméra HD", Description = "Logitech 4K pour visioconférence", Type = "Visioconférence", EstFonctionnel = true, SalleId = 2, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 4, Nom = "Micro sans fil", Description = "Système de microphone sans fil", Type = "Audio", EstFonctionnel = true, SalleId = 2, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 5, Nom = "Écran tactile 55p", Description = "Écran tactile interactif", Type = "Affichage", EstFonctionnel = true, SalleId = 3, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 6, Nom = "Ordinateurs (10)", Description = "10 postes de travail", Type = "Informatique", EstFonctionnel = true, SalleId = 4, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 7, Nom = "Microscopes (8)", Description = "8 microscopes optiques", Type = "Équipement scientifique", EstFonctionnel = true, SalleId = 4, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 8, Nom = "Microscopes (8)", Description = "8 microscopes optiques", Type = "Équipement scientifique", EstFonctionnel = true, SalleId = 5, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 9, Nom = "Vidéoprojecteur HD", Description = "Projecteur haute définition", Type = "Vidéoprojecteur", EstFonctionnel = true, SalleId = 6, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 10, Nom = "Tableau blanc", Description = "Tableau blanc traditionnel", Type = "Tableau", EstFonctionnel = true, SalleId = 7, DateCreation = DateTime.UtcNow },
                    new Equipement { Id = 11, Nom = "Caméra HD", Description = "Caméra pour enregistrement", Type = "Enregistrement", EstFonctionnel = true, SalleId = 8, DateCreation = DateTime.UtcNow }
                };
                await _context.Equipements.AddRangeAsync(equipements);
                await _context.SaveChangesAsync();

                // Seed Reservations
                var reservations = new List<Reservation>
                {
                    new Reservation { Id = 1, DateReservation = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), Motif = "Cours de Programmation Avancée", Statut = "Confirmée", UserId = 1, SalleId = 1, CreneauId = 1, DateDebut = "2026-02-12", DateFin = "2026-02-12", HeureDebut = "08:00", HeureFin = "10:00" },
                    new Reservation { Id = 2, DateReservation = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), Motif = "TP Chimie Organique", Statut = "Confirmée", UserId = 1, SalleId = 4, CreneauId = 2, DateDebut = "2026-02-12", DateFin = "2026-02-12", HeureDebut = "10:00", HeureFin = "12:00" },
                    new Reservation { Id = 3, DateReservation = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), Motif = "Réunion d'équipe pédagogique", Statut = "Confirmée", UserId = 1, SalleId = 7, CreneauId = 3, DateDebut = "2026-02-12", DateFin = "2026-02-12", HeureDebut = "12:00", HeureFin = "14:00" }
                };
                await _context.Reservations.AddRangeAsync(reservations);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("Database successfully seeded with EF Core.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }
    }
}

