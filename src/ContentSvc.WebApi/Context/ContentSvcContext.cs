using ContentSvc.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentSvc.WebApi.Context
{
    public class ContentSvcContext : DbContext
    {
        public ContentSvcContext(DbContextOptions options) : base(options)
        {
            Database.SetCommandTimeout(200);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // foreach (var type in modelBuilder.Model.GetEntityTypes())
            // {
            //     type.SetTableName(type.DisplayName());
            // }

            modelBuilder.Entity<MinioUser>()
                .HasOne(u => u.Service)
                .WithMany(s => s.MinioUsers)
                .HasForeignKey(u => u.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApiKey>()
                .HasIndex(k => k.Key)
                .IsUnique(true);

            modelBuilder.Entity<ApiKey>()
                .HasOne(k => k.MinioUser)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(k => k.MinioUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Service> Services { get; set; }

        public DbSet<MinioUser> MinioUsers { get; set; }

        public DbSet<ApiKey> ApiKeys { get; set; }
    }
}
