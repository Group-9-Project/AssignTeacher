using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace SchoolTimetable.Models
{
    public enum ApplicationStatus
    {
        Pending,
        Accept,
        Reject
    }

    public enum conStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public static class BorrowStatus
    {
        public const string Active = "Active";
        public const string Returned = "Returned";
        public const string Overdue = "Overdue";
        public const string Lost = "Lost";
        public const string Reserved = "Reserved";
        public const string Cancelled = "Cancelled";
    }

    public enum UserRole { Librarian, Student, Teacher, Admin }

    public static class SubstitutionStatus
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Cancelled = "Cancelled";
    }

    public class SessionUser
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public int? Grade { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockReason { get; set; }
        public bool IsLibrarian => Role == UserRole.Librarian;
    }

    public static class SessionHelper
    {
        private const string UserSessionKey = "UserSession";

        public static SessionUser GetUser(HttpSessionStateBase session)
        {
            return session[UserSessionKey] as SessionUser;
        }

        public static void SetUser(HttpSessionStateBase session, SessionUser user)
        {
            session[UserSessionKey] = user;
        }

        public static void Logout(HttpSessionStateBase session)
        {
            session[UserSessionKey] = null;
        }
    }

    [Table("AppUsers")]
    public class AppUser
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string FullName { get; set; }
        [Required, StringLength(150)]
        public string Email { get; set; }
        [StringLength(20)]
        public string StudentNumber { get; set; }
        public virtual StudentAccount StudentAccount { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; } = UserRole.Student;
        public int? Grade { get; set; }
        public bool IsBlocked { get; set; } = false;
        [StringLength(400)]
        public string BlockReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }

    [Table("Books")]
    public class Book
    {
        [Key]
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
        public int? PublicationYear { get; set; }
        [Required, Range(0, 9999)]
        public int TotalCopies { get; set; } = 1;
        public int AvailableCopies { get; set; } = 1;
        [StringLength(600)]
        public string Description { get; set; }
        public int? MinGrade { get; set; }
        public int? MaxGrade { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public virtual ICollection<Borrowing> Borrowings { get; set; }
    }

    [Table("Borrowings")]
    public class Borrowing
    {
        [Key]
        public int Id { get; set; }
        public int AppUserId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser Student { get; set; }
        public int BookId { get; set; }
        [ForeignKey("BookId")]
        public virtual Book Book { get; set; }
        public DateTime BorrowedDate { get; set; } = DateTime.Now;
        [Required]
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }
        [Required, StringLength(20)]
        public string Status { get; set; }
        [StringLength(400)]
        public string Notes { get; set; }
        [Column(TypeName = "decimal")]
        public decimal FineAmount { get; set; } = 0;
        public bool FinePaid { get; set; } = false;
    }

    public class Teacher
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string TemporaryPassword { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        [NotMapped]
        public string FullName => FirstName + " " + LastName;
        public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; }
        public virtual ICollection<TimetableSlot> TimetableSlots { get; set; }
        public virtual ICollection<LearningMaterial> LearningMaterials { get; set; }
        public virtual ICollection<Substitution> SubstitutionsAssigned { get; set; }
        public bool IsAdmin { get; internal set; }
    }

    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Color { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<TimetableSlot> TimetableSlots { get; set; }
        public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; }
    }

    public class TimetableSlot
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
        public int ClassId { get; set; }
        public int DayOfWeek { get; set; }
        public int Period { get; set; }
        public string Room { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual SchoolClass Class { get; set; }
        public virtual ICollection<Substitution> Substitutions { get; set; }
        [NotMapped]
        public string DayName => Enum.GetName(typeof(DayOfWeek), (DayOfWeek)DayOfWeek);
    }

    public class Substitution
    {
        public int Id { get; set; }
        public DateTime SubstitutionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Reason { get; set; }
        public int TimetableSlotId { get; set; }
        public virtual TimetableSlot TimetableSlot { get; set; }
        public int OriginalTeacherId { get; set; }
        public virtual Teacher OriginalTeacher { get; set; }
        public int SubstituteTeacherId { get; set; }
        public virtual Teacher SubstituteTeacher { get; set; }
        public string Status { get; set; }
    }

    public class LearningMaterial
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public DateTime UploadDate { get; set; }
        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; }
        public int SchoolClassId { get; set; }
        public virtual SchoolClass Class { get; set; }
    }

    public class SchoolClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Grade { get; set; }
        public int? StudentCount { get; set; }
        public string Room { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<TimetableSlot> TimetableSlots { get; set; }
    }

    public class TeacherSubject
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual Subject Subject { get; set; }
    }

    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime DatePosted { get; set; }
        public int? SchoolClassId { get; set; }
        public virtual SchoolClass TargetedClass { get; set; }
        public int TeacherId { get; set; }
        public virtual Teacher Author { get; set; }
    }

    public class NotificationLog
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool Sent { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Parent
    {
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        public string ResetCode { get; set; }
        [NotMapped]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public DateTime? ResetExpiry { get; set; }
        public DateTime CreatedAt { get; internal set; }
    }

    public class Application
    {
        [Key]
        public int Id { get; set; }
        public bool IsProcessed { get; set; } = false;
        public bool IsPaid { get; set; } = false;
        public int? AssignedClassId { get; set; }
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public int ParentId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string HomeAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }
        public string Allergies { get; set; }
        public string SpecialNeeds { get; set; }
        [Required]
        public string PreviousSchoolName { get; set; }
        [Required]
        public string GradeApplyingFor { get; set; }
        [Required]
        public string ParentName { get; set; }
        [Required]
        public string Relationship { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        public DateTime SubmissionDate { get; set; }
        public double AverageMark { get; set; }
        public virtual ICollection<ApplicantMark> Marks { get; set; }
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the code of conduct")]
        public bool AgreedToCodeOfConduct { get; set; }
    }

    public class ApplicantMark
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string SubjectName { get; set; }
        [Required, Range(0, 100)]
        public int Percentage { get; set; }
        public int ApplicationId { get; set; }
        public virtual Application Application { get; set; }
    }

    [Table("Student")]
    public class Student
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string StudentNumber { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required, EmailAddress]
        public string ParentEmail { get; set; }
        [Required, Column(TypeName = "decimal")]
        public decimal Balance { get; set; } = 1000.00m;
        [Required]
        public bool RegistrationFeePaid { get; set; } = false;
        public bool IsBlocked { get; set; }
        public int ApplicationId { get; set; }
        [ForeignKey("ApplicationId")]
        public virtual Application OriginalApplication { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("EntranceTests")]
    public class Entrance_test
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TestId { get; set; }
        public string StudentNumber { get; set; }
        [Required]
        public string Title { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public string QuetionOne { get; set; }
        public string AnswerOneAA { get; set; }
        public string AnswerOneBB { get; set; }
        public string AnswerOneCC { get; set; }
        public string CorrectAnswer1 { get; set; }
        public string QuetionTwo { get; set; }
        public string AnswerTwoAA { get; set; }
        public string AnswerTwoBB { get; set; }
        public string AnswerTwoCC { get; set; }
        public string CorrectAnswer2 { get; set; }
        public string QuetionThree { get; set; }
        public string AnswerThreeAA { get; set; }
        public string AnswerThreeBB { get; set; }
        public string AnswerThreeCC { get; set; }
        public string CorrectAnswer3 { get; set; }
        public string QuetionFour { get; set; }
        public string AnswerFourAA { get; set; }
        public string AnswerFourBB { get; set; }
        public string AnswerFourCC { get; set; }
        public string CorrectAnswer4 { get; set; }
        public string QuetionFive { get; set; }
        public string AnswerFiveAA { get; set; }
        public string AnswerFiveBB { get; set; }
        public string AnswerFiveCC { get; set; }
        public string CorrectAnswer5 { get; set; }
        public string a { get; set; }
        public string b { get; set; }
        public string c { get; set; }
        public int counter { get; set; }
        public double TotalScore { get; set; }
        public string Email { get; set; }
        public bool IsAllocated { get; set; } = false;

        public int CalcScore(string userAnswer, string correctAnswer)
        {
            if (string.IsNullOrWhiteSpace(userAnswer) || string.IsNullOrWhiteSpace(correctAnswer))
                return 0;
            return string.Equals(userAnswer.Trim(), correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        }

        public double CalcPercent(int score, int totalQuestions)
        {
            if (totalQuestions <= 0) return 0;
            return (double)score / totalQuestions * 100.0;
        }
    }

    [Table("StudentAccounts")]
    public class StudentAccount
    {
        [Key, ForeignKey("AppUser")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required, Display(Name = "Student Number")]
        public string StudentNumber { get; set; }
        public virtual AppUser AppUser { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string TemporaryPassword { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool IsActive { get; set; } = true;
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsBlocked { get; set; }
        [StringLength(400)]
        public string BlockReason { get; set; }
    }

    // --- INTEGRATED ONLINE CLASS MODELS ---

    public class OnlineClass
    {
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; }
        [MaxLength(2000)]
        public string Description { get; set; }
        [Required, MaxLength(100)]
        public string Subject { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; }
        [MaxLength(200)]
        public string MeetingRoomId { get; set; }
        [MaxLength(100)]
        public string JoinPassword { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompleted { get; set; }
        public int MaxStudents { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public virtual AppUser Teacher { get; set; }
        public virtual ICollection<ClassAttendance> Attendances { get; set; }
        [NotMapped]
        public string JitsiRoomUrl => $"https://meet.jit.si/{MeetingRoomId}";
        [NotMapped]
        public bool IsLive => IsActive && !IsCompleted;
    }

    public class ClassAttendance
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public virtual OnlineClass OnlineClass { get; set; }
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual AppUser Student { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool IsPresent { get; set; }
        [NotMapped]
        public int? DurationMinutes
        {
            get
            {
                if (LeftAt.HasValue)
                    return (int)(LeftAt.Value - JoinedAt).TotalMinutes;
                return null;
            }
        }
    }

    public class Consultation
    {
        [Key]
        public int Id { get; set; }
        public conStatus status { get; set; } = conStatus.Pending;
        public DateTime date { get; set; }
        public DateTime time { get; set; }
        public string grade { get; set; }

        public string subject { get; set; }

        public int? ParentId { get; set; }
        public string reason { get; set; }
        public int TeacherSubjectId { get; set; }
        public int ClassId { get; set; }

        [ForeignKey("TeacherSubjectId")]
        public virtual TeacherSubject TeacherSubject { get; set; }
        [ForeignKey("ClassId")]
        public virtual SchoolClass Class { get; set; }

        [ForeignKey("ParentId")]
        public virtual Parent Parent { get; set; }

    }


  
}





    