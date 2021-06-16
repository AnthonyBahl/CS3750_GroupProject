using System;
using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
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
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Enrollment> Enrollments { get; set; }

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

                entity.Property(e => e.Bio)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("Bio");
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

            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(e => e.CourseID).HasColumnName("CourseID");



                entity.Property(e => e.InstructorID).HasColumnName("InstructorID");

                entity.Property(e => e.Department).HasColumnName("DeptID");

                entity.Property(e => e.ClassNumber)
                .HasMaxLength(30)
                .HasColumnName("ClassNumber");

                entity.Property(e => e.ClassTitle)
                .HasMaxLength(60)
                .HasColumnName("ClassTitle");

                entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("Description");

                entity.Property(e => e.Location)
                .HasMaxLength(100)
                .HasColumnName("Location");

                entity.Property(e => e.Credits).HasColumnName("Credits");

                entity.Property(e => e.Capacity).HasColumnName("Capacity");

                entity.Property(e => e.MeetDays)
                .HasMaxLength(10)
                .HasColumnName("MeetDays");

                entity.Property(e => e.StartTime).HasColumnName("StartTime");
                entity.Property(e => e.EndTime).HasColumnName("EndTime");
                entity.Property(e => e.EndTime).HasColumnName("Color");
            });

            modelBuilder.Entity<Department>(entity => {
                entity.Property(e => e.DeptID).HasColumnName("DeptID");

                entity.Property(e => e.DeptName)
                .HasMaxLength(30)
                .HasColumnName("DeptName");


                entity.Property(e => e.DeptCode)
                .HasMaxLength(30)
                .HasColumnName("DeptCode");
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.Property(e => e.enrollmentID).HasColumnName("enrollmentID");

                entity.Property(e => e.studentID).HasColumnName("studentID");

                entity.Property(e => e.courseID).HasColumnName("courseID");
            });

            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.Property(e => e.AssignmentID).HasColumnName("AssignmentID");

                entity.Property(e => e.CourseID).HasColumnName("CourseID");

                entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("Title");

                entity.Property(e => e.Description)
                .HasMaxLength(256)
                .HasColumnName("Description");

                entity.Property(e => e.MaxPoints).HasColumnName("MaxPoints");

                entity.Property(e => e.DueDate).HasColumnName("DueDate");

                entity.Property(e => e.SubmissionType).HasColumnName("SubmissionType");
            });

            modelBuilder.Entity<Submission>(entity =>
            {
                entity.Property(e => e.SubmissionID).HasColumnName("SubmissionID");

                entity.Property(e => e.AssignmentID).HasColumnName("AssignmentID");

                entity.Property(e => e.StudentID).HasColumnName("StudentID");

                entity.Property(e => e.SubmissionDate).HasColumnName("SubmissionDate");

                entity.Property(e => e.SubmissionType).HasColumnName("SubmissionType");

                entity.Property(e => e.Grade).HasColumnName("Grade");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
