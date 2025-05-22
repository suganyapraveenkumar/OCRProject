using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AIAnswerSheetAPI.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<AnswerSheet> AnswerSheets { get; set; }
        public DbSet<Student> StudentProfile { get; set; }
        public DbSet<Teacher> TeacherProfile { get; set; }
        public DbSet<EvaluationResult> EvaluationResults { get; set; }
        public DbSet<Subject> Subjects {  get; set; }
        public DbSet<Category> Categories { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }

}
