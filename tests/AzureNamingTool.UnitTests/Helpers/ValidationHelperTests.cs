using AzureNamingTool.Helpers;
using FluentAssertions;
using Xunit;

namespace AzureNamingTool.UnitTests.Helpers;

public class ValidationHelperTests
{
    [Theory]
    [InlineData("Test1234", true)]
    [InlineData("Password1", true)]
    [InlineData("MyP@ssw0rd", true)]
    [InlineData("lowercase1", false)] // No uppercase
    [InlineData("UPPERCASE", false)] // No number
    [InlineData("Short1", false)] // Less than 8 chars
    [InlineData("NoNumber", false)] // No number
    [InlineData("12345678", false)] // No uppercase
    [InlineData("", false)] // Empty
    public void ValidatePassword_ShouldValidateCorrectly(string password, bool expected)
    {
        // Act
        var result = ValidationHelper.ValidatePassword(password);

        // Assert
        result.Should().Be(expected);
    }
}
