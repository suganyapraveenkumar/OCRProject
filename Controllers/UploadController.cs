using AIAnswerSheetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
public class UploadController : ControllerBase
{
    private readonly AppDbContext _context;

    public UploadController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile answerSheet, [FromForm] string studentId, [FromForm] string className)
    {
        if (answerSheet == null || answerSheet.Length == 0)
            return BadRequest("No file uploaded");

        using var ms = new MemoryStream();
        await answerSheet.CopyToAsync(ms);

        var sheet = new AnswerSheet
        {
            StudentId = studentId,
            Class = className,
            FileName = answerSheet.FileName,
            ImageData = ms.ToArray()
        };

        _context.AnswerSheets.Add(sheet);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Image saved to DB", sheet.Id });
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> EvaluateAnswerSheet(
    [FromQuery] int studentId,
    [FromQuery] int subjectId,
    [FromQuery] int categoryId,
    [FromForm] IFormFile studentFile,
    [FromForm] IFormFile modelFile)
    {
        if (studentFile == null || modelFile == null)
        {
            return BadRequest("Both studentFile and modelFile are required.");
        }
        using var client = new HttpClient();
        using var form = new MultipartFormDataContent();

        form.Add(new StreamContent(studentFile.OpenReadStream()), "student_file", studentFile.FileName);
        form.Add(new StreamContent(modelFile.OpenReadStream()), "model_file", modelFile.FileName);

        var response = await client.PostAsync("http://127.0.0.1:8000/evaluate", form); // FastAPI endpoint

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest("Error from FastAPI: " + await response.Content.ReadAsStringAsync());
        }

        var content = await response.Content.ReadAsStringAsync();
        var resultJson = JsonDocument.Parse(content);

        var results = resultJson.RootElement.GetProperty("page_results");

        foreach (var pageResult in results.EnumerateArray())
        {
            var eval = new EvaluationResult
            {
                StudentId = studentId,
                SubjectId = subjectId,         
                CategoryId = categoryId,
                PageNumber = pageResult.GetProperty("page").GetInt32(),
                SemanticScore = pageResult.GetProperty("semantic_score").GetDouble(),
                SemanticGrade = pageResult.GetProperty("semantic_grade").GetDouble(),
                TfidfScore = pageResult.GetProperty("tfidf_score").GetDouble(),
                TfidfGrade = pageResult.GetProperty("tfidf_grade").GetDouble(),
                TextPreview = pageResult.GetProperty("page_text_preview").GetString(),
                
            };
            try
            {
                _context.EvaluationResults.Add(eval);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving to DB: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
            }
        }

        

        return Ok(new { message = "Evaluation results saved", studentId });
    }

}
