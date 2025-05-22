using Microsoft.AspNetCore.Mvc;

namespace AIAnswerSheetAPI.Controllers
{
    using AIAnswerSheetAPI.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Data;

    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public StudentController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpGet("GetStudentProfile/{userId}")]
        public IActionResult GetStudentProfile(string userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var student = new Dictionary<string, string>();

            using (var conn = new SqlConnection(connectionString))
            {
                string query = "SELECT StudentId,StudentName,Class,Section,Address FROM Student WHERE StudentId = @StudentId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", userId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            student["StudentId"] = reader["StudentId"].ToString();
                            student["StudentName"] = reader["StudentName"].ToString();
                            student["class"] = reader["Class"].ToString();
                            student["Section"] = reader["Section"].ToString();
                            student["address"] = reader["Address"].ToString();
                            return Ok(student);
                        }
                    }
                }
            }

            return NotFound("Student not found.");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] int userId, [FromForm] string category, [FromForm] string subject)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // Save or process the file
            var path = Path.Combine("Uploads", file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Optionally save to DB here

            return Ok("Upload successful");
        }


        [HttpGet("marks/{userId}")]
        public async Task<IActionResult> GetStudentMarks(int userId, [FromQuery] int category)
        {
           
            var marks = await _context.StudentProfile
                .Where(m => m.StudentId == userId && m.CategoryId == category)
                .Select(m => new {
                    Subject = m.SubjectId,
                    Marks = m.TotalMark,
                    Grade = m.Grade
                })

                .ToListAsync();

            if (marks == null || marks.Count == 0)
                return NotFound("No marks found for this user and category.");

            return Ok(marks);
        }
    }

}
