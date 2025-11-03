using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
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

    [Theory]
    [InlineData("123", true)]
    [InlineData("456789", true)]
    [InlineData("0", true)]
    [InlineData("abc", false)]
    [InlineData("123abc", false)]
    [InlineData("", false)]
    [InlineData("12.34", false)]
    public void CheckNumeric_ShouldValidateNumericStrings(string value, bool expected)
    {
        // Act
        var result = ValidationHelper.CheckNumeric(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("abc123", true)]
    [InlineData("ABC", true)]
    [InlineData("123", true)]
    [InlineData("abc123xyz", true)]
    [InlineData("abc-123", false)]
    [InlineData("abc_123", false)]
    [InlineData("abc 123", false)]
    [InlineData("abc@123", false)]
    [InlineData("abc.123", false)]
    public void CheckAlphanumeric_ShouldValidateAlphanumericStrings(string value, bool expected)
    {
        // Act
        var result = ValidationHelper.CheckAlphanumeric(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CheckComponentLength_ShouldReturnTrue_WhenValueWithinRange()
    {
        // Arrange
        var component = new ResourceComponent { MinLength = "3", MaxLength = "10" };
        var value = "test";

        // Act
        var result = ValidationHelper.CheckComponentLength(component, value);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CheckComponentLength_ShouldReturnFalse_WhenValueTooShort()
    {
        // Arrange
        var component = new ResourceComponent { MinLength = "5", MaxLength = "10" };
        var value = "abc";

        // Act
        var result = ValidationHelper.CheckComponentLength(component, value);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CheckComponentLength_ShouldReturnFalse_WhenValueTooLong()
    {
        // Arrange
        var component = new ResourceComponent { MinLength = "3", MaxLength = "5" };
        var value = "toolongvalue";

        // Act
        var result = ValidationHelper.CheckComponentLength(component, value);

        // Assert
        result.Should().BeFalse();
    }
}
