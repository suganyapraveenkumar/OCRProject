using System.ComponentModel.DataAnnotations.Schema;

namespace AIAnswerSheetAPI.Models
{
    [Table("EvaluationResult")]
    public class EvaluationResult
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int PageNumber { get; set; }
        public double SemanticScore { get; set; }
        public double SemanticGrade { get; set; }
        public double TfidfScore { get; set; }
        public double TfidfGrade { get; set; }
        public string TextPreview { get; set; }
        [ForeignKey("StudentId")]
        public Student Student { get; set; }
        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }
        public int SubjectId { get; set; }
        public int CategoryId { get; set; }
    }

}
