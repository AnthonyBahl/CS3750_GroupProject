using System;
using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace cs3750LMS.Models
{
    public partial class cs3750Context : DbContext
    {
        public cs3750Context()
        {
        }

        public cs3750Context(DbContextOptions<cs3750Context> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserCache> UserCache { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).HasColumnName("userID");

                entity.Property(e => e.AccountType).HasColumnName("accountType");

                entity.Property(e => e.Birthday)
                    .HasColumnType("date")
                    .HasColumnName("birthday");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("firstName");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("lastName");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false)
                    .HasColumnName("password");
            });

            modelBuilder.Entity<UserCache>(entity =>
                {
                    entity.Property(e => e.CacheId)
                    .IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false)
                    .HasColumnName("id");

                    entity.Property(e => e.UserEmail)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("userEmail");

                    entity.Property(e => e.CacheFirstName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("FirstName");

                    entity.Property(e => e.CacheLastName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("LastName");

                    entity.Property(e => e.ExpiresAtTime)
                    .IsRequired()
                    .HasColumnType("DateTimeOffset")
                    .HasColumnName("ExpiresAtTime");

                    entity.Property(e => e.SlidingExpirationInSeconds)
                    .HasColumnType("Int64")
                    .HasColumnName("SlidingExpirationInSeconds");

                    entity.Property(e => e.AbsoluteExpiration)
                    .HasColumnType("DateTimeOffset")
                    .HasColumnName("AbsoluteExpiration");

                });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
