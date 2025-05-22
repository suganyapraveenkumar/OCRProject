using System.ComponentModel.DataAnnotations.Schema;

namespace AIAnswerSheetAPI.Models
{
    [Table("Teacher")]
    public class Teacher
    {
         
        public int? TeacherId {  get; set; }
        public string TeacherName { get; set; }
        public string EmailAddress { get; set; }
        public string SubjectTaken { get; set; }
        public string Designation { get; set; }
        public int Experience { get; set; }
        public string? Password { get; set; }

    }
}
