using InterviewBank.API.Services;

namespace InterviewBank.API.Tests.Unit;

public class AnswerEvaluatorParseTests
{
    [Fact]
    public void Parses_plain_json()
    {
        var result = AnthropicAnswerEvaluator.Parse("""{"score":85,"feedback":"Solid answer."}""");

        Assert.Equal(new AnswerEvaluation(85, "Solid answer."), result);
    }

    [Fact]
    public void Parses_json_wrapped_in_markdown_fence()
    {
        var text = "```json\n{\"score\": 40, \"feedback\": \"Missed the key point.\"}\n```";

        var result = AnthropicAnswerEvaluator.Parse(text);

        Assert.Equal(40, result!.Score);
        Assert.Equal("Missed the key point.", result.Feedback);
    }

    [Theory]
    [InlineData("""{"score":150,"feedback":""}""", 100)]
    [InlineData("""{"score":72.6,"feedback":""}""", 73)]
    [InlineData("""{"score":"55","feedback":""}""", 55)]
    public void Normalises_score(string json, int expected)
    {
        Assert.Equal(expected, AnthropicAnswerEvaluator.Parse(json)!.Score);
    }

    [Theory]
    [InlineData("")]
    [InlineData("I cannot grade this.")]
    [InlineData("""{"feedback":"no score"}""")]
    [InlineData("{not json}")]
    public void Returns_null_for_unusable_output(string text)
    {
        Assert.Null(AnthropicAnswerEvaluator.Parse(text));
    }
}
