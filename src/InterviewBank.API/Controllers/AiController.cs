using System.ComponentModel.DataAnnotations;
using InterviewBank.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewBank.API.Controllers;

public class EvaluateAnswerDto
{
    [Required, MaxLength(1000)] public string QuestionText   { get; set; } = string.Empty;
    [Required, MaxLength(5000)] public string ExpectedAnswer { get; set; } = string.Empty;
    [Required, MaxLength(5000)] public string UserAnswer     { get; set; } = string.Empty;
}

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAnswerEvaluator _evaluator;

    public AiController(IAnswerEvaluator evaluator) => _evaluator = evaluator;

    [HttpPost("evaluate-answer")]
    public async Task<IActionResult> EvaluateAnswer([FromBody] EvaluateAnswerDto dto, CancellationToken ct)
    {
        if (!_evaluator.IsConfigured)
            return StatusCode(503, new { error = "AI evaluation is not configured on the server." });

        try
        {
            var result = await _evaluator.EvaluateAsync(dto.QuestionText, dto.ExpectedAnswer, dto.UserAnswer, ct);
            return Ok(new { score = result.Score, feedback = result.Feedback });
        }
        catch (AnswerEvaluatorUnavailableException ex)
        {
            return StatusCode(502, new { error = ex.Message });
        }
    }
}
