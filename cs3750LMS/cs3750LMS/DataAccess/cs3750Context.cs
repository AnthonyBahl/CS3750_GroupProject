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
        public virtual DbSet<Link> Links { get; set; }
        public virtual DbSet<State> States { get; set; }

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

                entity.Property(e => e.Address1)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("address1");

                entity.Property(e => e.Address2)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("address2");

                entity.Property(e => e.City)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("city");

                entity.Property(e => e.State)
                    .HasColumnName("state");

                entity.Property(e => e.Zip)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("zip");

                entity.Property(e => e.Phone)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("phone");

                entity.Property(e => e.ProfileImage)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("image");

                entity.Property(e => e.LinkedIn)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("LinkedIn");

                entity.Property(e => e.Github)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("Github");

                entity.Property(e => e.Twitter)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("Twitter");
            });

            modelBuilder.Entity<Link>(entity =>
            {
                entity.Property(e => e.LinkID).HasColumnName("linkID");
                entity.Property(e => e.UserID).HasColumnName("userID");
                entity.Property(e => e.Contents)
                    .HasMaxLength(256)
                    .IsUnicode(false)
                    .HasColumnName("contents");

            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.Property(e => e.StateID).HasColumnName("stateID");
                entity.Property(e => e.StateCode)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("stateCode");
                entity.Property(e => e.StateName)
                    .HasMaxLength(128)
                    .HasColumnName("stateName");
            });



                OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
