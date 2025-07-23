using LibraryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Persistence.Data
{
    public static class SeedData
    {
        public static void Initialize(LibraryDbContext context)
        {
            Console.WriteLine("🌱 SEED DATA DEBUG - Initialize called");
            
            // Ensure database is created
            Console.WriteLine("🌱 SEED DATA DEBUG - Ensuring database is created");
            context.Database.EnsureCreated();

            // Check if we already have data
            Console.WriteLine("🌱 SEED DATA DEBUG - Checking if data already exists");
            var authorCount = context.Authors.Count();
            var publisherCount = context.Publishers.Count();
            
            Console.WriteLine($"🌱 SEED DATA DEBUG - Current Authors: {authorCount}, Publishers: {publisherCount}");
            
            if (context.Authors.Any() || context.Publishers.Any())
            {
                Console.WriteLine("🌱 SEED DATA DEBUG - Database already has data, skipping seed");
                return; // Database has been seeded
            }

            // Add Authors
            var authors = new[]
            {
                new Author("Orhan Pamuk") { Biography = "Nobel ödüllü Türk yazar", Nationality = "Turkish" },
                new Author("Yaşar Kemal") { Biography = "Ünlü Türk roman yazarı", Nationality = "Turkish" },
                new Author("Nazım Hikmet") { Biography = "Türk şair ve oyun yazarı", Nationality = "Turkish" },
                new Author("Sabahattin Ali") { Biography = "Türk yazar ve öğretmen", Nationality = "Turkish" },
                new Author("Ahmet Hamdi Tanpınar") { Biography = "Türk yazar ve edebiyat tarihçisi", Nationality = "Turkish" }
            };

            context.Authors.AddRange(authors);

            // Add Publishers
            var publishers = new[]
            {
                new Publisher("YKY") { Address = "İstanbul", Website = "https://yky.com.tr", ContactEmail = "info@yky.com.tr" },
                new Publisher("İletişim Yayınları") { Address = "İstanbul", Website = "https://iletisim.com.tr", ContactEmail = "info@iletisim.com.tr" },
                new Publisher("Can Yayınları") { Address = "İstanbul", Website = "https://can.com.tr", ContactEmail = "info@can.com.tr" },
                new Publisher("Doğan Kitap") { Address = "İstanbul", Website = "https://dogankitap.com.tr", ContactEmail = "info@dogankitap.com.tr" },
                new Publisher("Türkiye İş Bankası Kültür Yayınları") { Address = "İstanbul", Website = "https://iskultur.com.tr", ContactEmail = "info@iskultur.com.tr" }
            };

            context.Publishers.AddRange(publishers);

            // Save changes
            Console.WriteLine("🌱 SEED DATA DEBUG - Saving seed data to database");
            try
            {
                var result = context.SaveChanges();
                Console.WriteLine($"🌱 SEED DATA DEBUG - Seed data saved successfully. Records affected: {result}");
                
                // Verify data was saved
                var finalAuthorCount = context.Authors.Count();
                var finalPublisherCount = context.Publishers.Count();
                Console.WriteLine($"🌱 SEED DATA DEBUG - Final counts - Authors: {finalAuthorCount}, Publishers: {finalPublisherCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🌱 SEED DATA DEBUG - EXCEPTION saving seed data: {ex.Message}");
                Console.WriteLine($"🌱 SEED DATA DEBUG - EXCEPTION Type: {ex.GetType().Name}");
                Console.WriteLine($"🌱 SEED DATA DEBUG - EXCEPTION StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 