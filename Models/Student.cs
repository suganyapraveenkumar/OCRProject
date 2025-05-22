using System.ComponentModel.DataAnnotations.Schema;

namespace AIAnswerSheetAPI.Models
{
    [Table("Student")]
    public class Student
    {
     
        public int? StudentId { get; set; }
        public string StudentName { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public int SubjectId { get; set; }
        public int CategoryId { get; set; }
        public string Grade { get; set; }
        public int TotalMark { get; set; }
        public string? Password { get; set; }
    }
}
