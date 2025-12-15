using FoodAppAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodAppAPI.Data
{
    public class foodAppContext : DbContext
    {
        public foodAppContext (DbContextOptions<foodAppContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Item> Items { get; set; } = null!;

        public DbSet<Category> Categories { get; set; } = null!;

        // ---------------- Fluent API ----------------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Item -> WasteLog cascade
            modelBuilder.Entity<WasteLog>()
                .HasOne(w => w.Item)
                .WithMany(i => i.WasteLogs)
                .HasForeignKey(w => w.ItemId)
                .OnDelete(DeleteBehavior.Cascade); // delete WasteLogs when Item is deleted

            // User -> WasteLog restricted to avoid multiple cascade paths
            modelBuilder.Entity<WasteLog>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict); // prevent cascade delete
                                                    // Categories → Users: disable cascade delete
            modelBuilder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Category)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // NO cascade

        }



    }
}
