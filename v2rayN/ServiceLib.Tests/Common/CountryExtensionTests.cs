namespace ServiceLib.Tests.Common;

public class CountryExtensionTests
{
    [Test]
    public async Task CountryToEmoji_WithNetherlands_ShouldReturnFlag()
    {
        await "NL".CountryToEmoji().Should().BeEqualTo("🇳🇱");
    }

    [Test]
    public async Task CountryToEmoji_WithLowerCase_ShouldReturnFlag()
    {
        await "nl".CountryToEmoji().Should().BeEqualTo("🇳🇱");
    }

    [Test]
    public async Task CountryToEmoji_WithIran_ShouldReturnFlag()
    {
        await "IR".CountryToEmoji().Should().BeEqualTo("🇮🇷");
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("N")]
    [Arguments("NLD")]
    [Arguments("N1")]
    public async Task CountryToEmoji_WithInvalidCode_ShouldReturnNull(string? countryCode)
    {
        await countryCode.CountryToEmoji().Should().BeNull();
    }

    [Test]
    public async Task ToString_OnWindowsAndOtherSystems_ShouldIncludeFlag()
    {
        var text = new IpInfoResult("NL", "1.2.3.4").ToString();

        await text.Should().BeEqualTo("🇳🇱(NL) 1.2.3.4");
    }
}
