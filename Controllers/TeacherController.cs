using AIAnswerSheetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIAnswerSheetAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeacherController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile/{teacherId}")]
        public IActionResult GetTeacherProfile(int teacherId)
        {
            var profile = _context.TeacherProfile.FirstOrDefault(t => t.TeacherId == teacherId);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            return Ok(_context.Categories.OrderBy(x=>x.CategoryName).ToList());
        }

        [HttpGet("subjects")]
        public IActionResult GetSubjectsById()
        {
            return Ok(_context.Subjects.OrderBy(x=>x.SubjectName).ToList());
        }

        [HttpGet("student-uploads")]
        public IActionResult GetStudentUploads([FromQuery] int categoryId, [FromQuery] int subjectId)
        {
            var uploads = _context.StudentProfile
                .Where(s => s.CategoryId == categoryId && s.SubjectId == subjectId)
                .ToList();
            return Ok(uploads);
        }
    }

}
