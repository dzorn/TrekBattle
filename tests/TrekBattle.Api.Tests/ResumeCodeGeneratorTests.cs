using System.Text.RegularExpressions;
using TrekBattle.Api.Services;

namespace TrekBattle.Api.Tests;

public class ResumeCodeGeneratorTests
{
    [Fact]
    public void Generate_ReturnsPascalCaseResumeCodeWithoutWhitespace()
    {
        var generator = new ResumeCodeGenerator();

        var code = generator.Generate();

        Assert.Matches(new Regex(@"^[A-Z][a-z]+[A-Z][a-z]+\d{3}$"), code);
        Assert.DoesNotContain(' ', code);
    }
}
