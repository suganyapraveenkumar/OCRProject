using AIAnswerSheetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIAnswerSheetAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Role == "student")
            {
                var student = _context.StudentProfile
                    .FirstOrDefault(s => s.StudentId == request.UserId && s.Password == request.Password);

                if (student != null)
                    return Ok(new { isValid = true, role = "student" });
            }
            else if (request.Role == "teacher")
            {
                var teacher = _context.TeacherProfile
                    .FirstOrDefault(t => t.TeacherId == request.UserId && t.Password == request.Password);

                if (teacher != null)
                    return Ok(new { isValid = true, role = "teacher" });
            }
            return Unauthorized(new { isValid = false });
        }
    }

    public class LoginRequest
    {
        public int UserId { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

}
