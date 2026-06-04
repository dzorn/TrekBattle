using System.Security.Cryptography;

namespace TrekBattle.Api.Services;

public sealed class ResumeCodeGenerator : IResumeCodeGenerator
{
    private static readonly string[] FirstWords =
    [
        "Amber", "Azure", "Bright", "Cinder", "Crimson", "Dark", "Golden", "Ivory",
        "Mercury", "Nova", "Quiet", "Silver", "Solar", "Velvet", "Violet", "Wild"
    ];

    private static readonly string[] SecondWords =
    [
        "Anchor", "Beacon", "Blade", "Comet", "Crown", "Harbor", "Horizon", "Lancer",
        "Orbit", "Pilot", "Ridge", "Signal", "Star", "Wing", "Watch", "Vector"
    ];

    public string Generate()
    {
        var first = Pick(FirstWords);
        var second = Pick(SecondWords);
        var digits = RandomNumberGenerator.GetInt32(0, 1000);

        return $"{first}{second}{digits:D3}";
    }

    private static string Pick(IReadOnlyList<string> words)
    {
        var index = RandomNumberGenerator.GetInt32(words.Count);
        return words[index];
    }
}
