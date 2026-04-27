using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SchoolTimetable.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("name=SchoolTimetableContext")
        {
            // Initializer handles the seeding of your initial data
            Database.SetInitializer(new SchoolTimetableInitializer());
        }

        // ── 1. ACADEMIC DBSETS ──────────────────────────────────────────────
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<SchoolClass> Classes { get; set; }
        public DbSet<TeacherSubject> TeacherSubjects { get; set; }
        public DbSet<TimetableSlot> TimetableSlots { get; set; }
        public DbSet<Substitution> Substitutions { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<LearningMaterial> LearningMaterials { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicantMark> ApplicantMarks { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentAccount> StudentAccounts { get; set; }

        // ── 2. LIBRARY & IDENTITY DBSETS ───────────────────────────────────
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<Entrance_test> Entrance_tests { get; set; }

        // ── 3. ONLINE CLASSROOM DBSETS ─────────────────────────────────────
        public DbSet<OnlineClass> OnlineClasses { get; set; }
        public DbSet<ClassAttendance> ClassAttendances { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // PREVENT PLURALIZATION (Prevents the "Table not found" errors)
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // ── ACADEMIC MAPPINGS ──────────────────────────────────────────
            modelBuilder.Entity<TimetableSlot>().HasRequired(ts => ts.Teacher).WithMany(t => t.TimetableSlots).HasForeignKey(ts => ts.TeacherId).WillCascadeOnDelete(false);
            modelBuilder.Entity<TimetableSlot>().HasRequired(ts => ts.Subject).WithMany(s => s.TimetableSlots).HasForeignKey(ts => ts.SubjectId).WillCascadeOnDelete(false);
            modelBuilder.Entity<TimetableSlot>().HasRequired(ts => ts.Class).WithMany(c => c.TimetableSlots).HasForeignKey(ts => ts.ClassId).WillCascadeOnDelete(false);
            modelBuilder.Entity<LearningMaterial>().HasRequired(m => m.Teacher).WithMany(t => t.LearningMaterials).HasForeignKey(m => m.TeacherId).WillCascadeOnDelete(true);

            // ── SUBSTITUTION MAPPINGS ───────────────────────────────────────
            modelBuilder.Entity<Substitution>().HasRequired(s => s.TimetableSlot).WithMany(ts => ts.Substitutions).HasForeignKey(s => s.TimetableSlotId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Substitution>().HasRequired(s => s.OriginalTeacher).WithMany().HasForeignKey(s => s.OriginalTeacherId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Substitution>().HasRequired(s => s.SubstituteTeacher).WithMany(t => t.SubstitutionsAssigned).HasForeignKey(s => s.SubstituteTeacherId).WillCascadeOnDelete(false);

            // ── LIBRARY & IDENTITY MAPPINGS ─────────────────────────────────
            modelBuilder.Entity<Borrowing>().HasRequired(b => b.Student).WithMany(u => u.Borrowings).HasForeignKey(b => b.AppUserId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Borrowing>().HasRequired(b => b.Book).WithMany(bk => bk.Borrowings).HasForeignKey(b => b.BookId).WillCascadeOnDelete(false);
            modelBuilder.Entity<AppUser>().HasOptional(u => u.StudentAccount).WithRequired(sa => sa.AppUser).WillCascadeOnDelete(false);

            // ── ONLINE CLASSROOM MAPPINGS ────────────────────────────────────
            modelBuilder.Entity<OnlineClass>().HasRequired(c => c.Teacher).WithMany().HasForeignKey(c => c.TeacherId).WillCascadeOnDelete(false);
            modelBuilder.Entity<ClassAttendance>().HasRequired(a => a.OnlineClass).WithMany(c => c.Attendances).HasForeignKey(a => a.ClassId).WillCascadeOnDelete(true);
            modelBuilder.Entity<ClassAttendance>().HasRequired(a => a.Student).WithMany().HasForeignKey(a => a.StudentId).WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }

        // ── LOGIC HELPERS ──────────────────────────────────────────────────
        public static AppUser AuthenticateUser(string email, string password)
        {
            using (var db = new ApplicationDbContext())
            {
                var hash = PasswordHelper.Hash(password);
                return db.AppUsers.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash);
            }
        }

        public static void UpdateOverdueStatuses(ApplicationDbContext db)
        {
            var now = DateTime.Now;
            var overdueItems = db.Borrowings.Where(b => b.Status == BorrowStatus.Active && b.DueDate < now).ToList();
            foreach (var b in overdueItems)
            {
                b.Status = BorrowStatus.Overdue;
                var daysLate = (now - b.DueDate).TotalDays;
                b.FineAmount = Math.Ceiling((decimal)daysLate) * 5m;
            }
            if (overdueItems.Any()) db.SaveChanges();
        }

        public System.Data.Entity.DbSet<Setup_Examination_timetable.Models.Setup_exam_time> Setup_exam_time { get; set; }
    }

    // ── INITIALIZER & SEED DATA ────────────────────────────────────────────
    public class SchoolTimetableInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var subjects = new List<Subject>
            {
                new Subject { Name = "Mathematics", Code = "MATH", Color = "#E74C3C", IsActive = true },
                new Subject { Name = "Computer Applications", Code = "CAT", Color = "#34495E", IsActive = true }
            };
            subjects.ForEach(s => context.Subjects.Add(s));

            var users = new List<AppUser>
            {
                new AppUser { FullName = "System Librarian", Email = "librarian@dgss.edu.za", PasswordHash = PasswordHelper.Hash("Librarian@123"), Role = UserRole.Librarian },
                new AppUser { FullName = "Demo Student", Email = "student@dgss.edu.za", PasswordHash = PasswordHelper.Hash("Student@123"), Role = UserRole.Student }
            };
            users.ForEach(u => context.AppUsers.Add(u));

            context.SaveChanges();
            base.Seed(context);
        }
    }

    // ── PASSWORD SECURITY ──────────────────────────────────────────────────
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("LibSalt_" + password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}