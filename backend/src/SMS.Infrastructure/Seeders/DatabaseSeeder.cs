using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;
using SMS.Domain.Enums;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                // Only seed if database is empty
                if (await context.Users.AnyAsync())
                {
                    Console.WriteLine("Database already seeded. Skipping initialization.");
                    return;
                }

                Console.WriteLine("Starting database seeding...");

                // Seed Users only (minimal seed data for testing)
                var users = new List<User>
                {
                    new User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Admin",
                        LastName = "User",
                        Email = "admin@sms.com",
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        Role = UserRole.Admin,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Teacher",
                        LastName = "User",
                        Email = "teacher@sms.com",
                        Username = "teacher",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher@123"),
                        Role = UserRole.Teacher,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = "Accountant",
                        LastName = "User",
                        Email = "accountant@sms.com",
                        Username = "accountant",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Accountant@123"),
                        Role = UserRole.Accountant,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
                
                Console.WriteLine($"✓ Seeded {users.Count} users successfully!");

                // Seed School Configuration
                var school = new School
                {
                    Id = Guid.NewGuid(),
                    Name = "My School",
                    Code = "SCH001",
                    Address = "123 School Street",
                    City = "City",
                    State = "State",
                    PostalCode = "12345",
                    PhoneNumber = "+1234567890",
                    EmailAddress = "school@sms.com",
                    Website = "https://school.com",
                    EstablishedDate = DateTime.UtcNow.AddYears(-10),
                    IsActive = true,
                    PrimaryColor = "#1976D2",
                    SecondaryColor = "#DC004E",
                    AccentColor = "#FF6F00",
                    HeaderText = "Welcome to My School",
                    FooterText = "© 2026 My School. All rights reserved.",
                    DateFormat = "dd/MM/yyyy",
                    CurrencyCode = "INR",
                    CurrencySymbol = "₹",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    UpdatedBy = "system"
                };

                await context.Schools.AddAsync(school);
                await context.SaveChangesAsync();
                
                Console.WriteLine("✓ Seeded school configuration successfully!");
                Console.WriteLine("Database seeding completed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }
    }
}

