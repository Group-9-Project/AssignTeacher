using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Setup_Examination_timetable.Models
{
    [Table("Setup_exam_time")]
    public class Setup_exam_time
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Exam_name { get; set; }
        [Required]
        public string venue { get; set; }
        [Required]
        public string Exam_Starttime { get; set; }
        [Required]
        public string Exam_Endtime { get; set; }
        [Required]
        public DateTime Exam_date { get; set; }
        public string Grade { get; set; }
    }
}