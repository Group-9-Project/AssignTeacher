using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using SchoolTimetable.Models;

namespace SchoolTimetable.ViewModels
{
    // ── GENERAL & TIMETABLE VIEWMODELS ──────────────────────────────────────────

    public class DashboardViewModel
    {
        public int TeacherCount { get; set; }
        public int SubjectCount { get; set; }
        public int ClassCount { get; set; }
        public int TimetableSlotCount { get; set; }
        public int PendingSubstitutionsCount { get; set; }
        public List<TimetableSlot> TodaySlots { get; set; }
        public List<Substitution> RecentSubstitutions { get; set; }
        public List<Teacher> TeachersWithTimetable { get; set; }

        public DashboardViewModel()
        {
            TodaySlots = new List<TimetableSlot>();
            RecentSubstitutions = new List<Substitution>();
            TeachersWithTimetable = new List<Teacher>();
        }
    }

    public class StudentAccountViewModel
    {
        public int StudentId { get; set; }

        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class TimetableViewModel
    {
        public Teacher Teacher { get; set; }
        public SchoolClass Class { get; set; }
        public List<TimetableSlot> Slots { get; set; }
        public int[] Days => new[] { 1, 2, 3, 4, 5 };
        public int[] Periods => new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        public string[] DayNames => new[] { "", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        public TimetableSlot GetSlot(int day, int period)
        {
            return Slots?.Find(s => s.DayOfWeek == day && s.Period == period);
        }

        public string PeriodTime(int period)
        {
            switch (period)
            {
                case 1: return "07:30–08:15";
                case 2: return "08:20–09:05";
                case 3: return "09:10–09:55";
                case 4: return "10:15–11:00";
                case 5: return "11:05–11:50";
                case 6: return "12:30–13:15";
                case 7: return "13:20–14:05";
                case 8: return "14:10–14:55";
                default: return "";
            }
        }

        public TimetableViewModel() { Slots = new List<TimetableSlot>(); }
    }

    // ── LIBRARY SPECIFIC VIEWMODELS ─────────────────────────────────────────────

    public class LibrarianDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public int BlockedStudents { get; set; }
        public List<Borrowing> RecentBorrowings { get; set; } = new List<Borrowing>();
        public List<Borrowing> OverdueList { get; set; } = new List<Borrowing>();
    }

    public class BookListViewModel
    {
        public List<Book> Books { get; set; }
        public string Search { get; set; }
        public string Genre { get; set; }
        public int? Grade { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
    }

    public class BookFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(150)]
        public string Author { get; set; }

        [StringLength(20)]
        public string ISBN { get; set; }

        [StringLength(80)]
        public string Genre { get; set; }

        [StringLength(120)]
        public string Publisher { get; set; }

        [Display(Name = "Publication Year")]
        public int? PublicationYear { get; set; }

        [Required, Range(1, 9999, ErrorMessage = "Total copies must be at least 1")]
        public int TotalCopies { get; set; }

        [StringLength(600)]
        public string Description { get; set; }

        public int? MinGrade { get; set; }
        public int? MaxGrade { get; set; }
    }

    public class StudentProfileViewModel
    {
        public AppUser Student { get; set; }
        public List<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
        public decimal TotalFines { get; set; }
        public int ActiveCount { get; set; }
        public int OverdueCount { get; set; }
    }

    public class IssueBookViewModel
    {
        [Required]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Please select a student.")]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

        public string Notes { get; set; }

        public Book Book { get; set; }
        public List<AppUser> Students { get; set; } = new List<AppUser>();
        public int? ReservationId { get; set; }
    }

    public class ReserveBookViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string Author { get; set; }

        [Display(Name = "Collection Deadline")]
        public DateTime CollectionDeadline { get; set; } = DateTime.Now.AddDays(2);
        public bool IsStudentBlocked { get; set; }
    }

    public class BlockStudentViewModel
    {
        [Required]
        public int StudentId { get; set; }

        [Required, StringLength(400), Display(Name = "Block Reason")]
        public string BlockReason { get; set; }
    }

    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; }
        public string StudentNumber { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockReason { get; set; }
        public decimal OutstandingFines { get; set; }
        public List<TimetableSlot> ClassTimetable { get; set; } = new List<TimetableSlot>();
        public List<Borrowing> MyBorrowings { get; set; }
        public List<Book> RecentBooks { get; set; }
    }

    // ── FORM & ASSIGNMENT VIEWMODELS ───────────────────────────────────────────

    public class TimetableSlotFormViewModel
    {
        public int Id { get; set; }
        [Required, Display(Name = "Teacher")]
        public int TeacherId { get; set; }
        [Required, Display(Name = "Subject")]
        public int SubjectId { get; set; }
        [Required, Display(Name = "Class")]
        public int ClassId { get; set; }
        [Required, Display(Name = "Day")]
        public int DayOfWeek { get; set; }
        [Required, Range(1, 8), Display(Name = "Period")]
        public int Period { get; set; }
        public string Room { get; set; }
        public string Notes { get; set; }

        public List<Teacher> Teachers { get; set; } = new List<Teacher>();
        public List<Subject> Subjects { get; set; } = new List<Subject>();
        public List<SchoolClass> Classes { get; set; } = new List<SchoolClass>();
    }

    public class SubstitutionFormViewModel
    {
        public int Id { get; set; }
        [Required] public int TimetableSlotId { get; set; }
        [Required] public int SubstituteTeacherId { get; set; }
        public DateTime SubstitutionDate { get; set; }
        public string Reason { get; set; }

        public TimetableSlot Slot { get; set; }
        public List<Teacher> AvailableTeachers { get; set; } = new List<Teacher>();
        public List<TimetableSlot> AllSlots { get; set; } = new List<TimetableSlot>();
    }

    public class TeacherAssignmentViewModel
    {
        public Teacher Teacher { get; set; }
        public List<Subject> AllSubjects { get; set; } = new List<Subject>();
        public List<int> AssignedSubjectIds { get; set; } = new List<int>();
    }

    public class AllocationViewModel
    {
        public int ApplicationId { get; set; }
        public string FullName { get; set; }
        public string Grade { get; set; }
        public string StudentNumber { get; set; }
        public double Score { get; set; }
        public int CorrectCount { get; set; }
        public string Email { get; set; }
        public bool IsAllocated { get; set; }
        public string AllocatedClassName { get; set; }
    }

    // ── ONLINE CLASS VIEWMODELS ────────────────────────────────────────────────

    public class CreateClassViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [MaxLength(100)]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Scheduled Date & Time")]
        public DateTime ScheduledAt { get; set; }

        [Range(15, 480, ErrorMessage = "Duration must be between 15 and 480 minutes")]
        [Display(Name = "Duration (minutes)")]
        public int DurationMinutes { get; set; }
        public IEnumerable<SelectListItem> SubjectList { get; set; }

        [Range(1, 500)]
        [Display(Name = "Max Students")]
        public int MaxStudents { get; set; }

        [Display(Name = "Join Password (optional)")]
        public string JoinPassword { get; set; }
    }

    public class ClassDetailsViewModel
    {
        public OnlineClass Class { get; set; }
        public List<ClassAttendance> Attendances { get; set; }
        public bool IsTeacher { get; set; }
        public bool HasJoined { get; set; }
        public int CurrentUser { get; set; }
    }

    public class OnlineClassDashboardViewModel
    {
        public List<OnlineClass> LiveClasses { get; set; }
        public List<OnlineClass> UpcomingClasses { get; set; }
        public List<OnlineClass> CompletedClasses { get; set; }
        public bool IsTeacher { get; set; }
        public AppUser CurrentUser { get; set; }
        public int TotalAttendees { get; set; }
    }

    public class AttendanceJsonItem
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string JoinedAt { get; set; }
        public string LeftAt { get; set; }
        public bool IsActive { get; set; }
    }
}