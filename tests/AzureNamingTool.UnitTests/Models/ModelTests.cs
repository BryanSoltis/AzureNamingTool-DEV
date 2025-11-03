using AzureNamingTool.Models;
using FluentAssertions;
using Xunit;

namespace AzureNamingTool.UnitTests.Models;

public class ServiceResponseTests
{
    [Fact]
    public void ServiceResponse_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new ServiceResponse();

        // Assert
        response.Success.Should().BeFalse();
        // ResponseObject is dynamic and null - can't use FluentAssertions on it directly
        Assert.Null(response.ResponseObject);
    }

    [Fact]
    public void ServiceResponse_ShouldSetSuccessProperty()
    {
        // Arrange
        var response = new ServiceResponse();

        // Act
        response.Success = true;

        // Assert
        response.Success.Should().BeTrue();
    }

    [Fact]
    public void ServiceResponse_ShouldSetResponseObjectProperty()
    {
        // Arrange
        var response = new ServiceResponse();
        var data = "Test data";

        // Act
        response.ResponseObject = data;

        // Assert
        // Cast dynamic to string for assertion
        string actualData = response.ResponseObject;
        actualData.Should().Be(data);
    }
}

public class AdminLogMessageTests
{
    [Fact]
    public void AdminLogMessage_ShouldInitializeWithDefaultValues()
    {
        // Act
        var message = new AdminLogMessage();

        // Assert
        message.Id.Should().Be(0);
        message.Title.Should().BeEmpty();
        message.Message.Should().BeEmpty();
        message.CreatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AdminLogMessage_ShouldSetProperties()
    {
        // Arrange
        var message = new AdminLogMessage();
        var now = DateTime.Now;

        // Act
        message.Id = 1;
        message.Title = "ERROR";
        message.Message = "Test error message";
        message.CreatedOn = now;

        // Assert
        message.Id.Should().Be(1);
        message.Title.Should().Be("ERROR");
        message.Message.Should().Be("Test error message");
        message.CreatedOn.Should().Be(now);
    }
}

public class ResourceNameResponseTests
{
    [Fact]
    public void ResourceNameResponse_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new ResourceNameResponse();

        // Assert
        response.Success.Should().BeFalse();
        response.ResourceName.Should().BeEmpty();
        response.Message.Should().BeEmpty();
    }

    [Fact]
    public void ResourceNameResponse_ShouldSetProperties()
    {
        // Arrange
        var response = new ResourceNameResponse();

        // Act
        response.Success = true;
        response.ResourceName = "st-test-dev-001";
        response.Message = "Name generated successfully";

        // Assert
        response.Success.Should().BeTrue();
        response.ResourceName.Should().Be("st-test-dev-001");
        response.Message.Should().Be("Name generated successfully");
    }
}

public class ValidateNameResponseTests
{
    [Fact]
    public void ValidateNameResponse_ShouldInitializeWithDefaultValues()
    {
        // Act
        var response = new ValidateNameResponse();

        // Assert
        response.Valid.Should().BeTrue(); // Default is true according to the model
        response.Name.Should().BeNull();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void ValidateNameResponse_ShouldSetProperties()
    {
        // Arrange
        var response = new ValidateNameResponse();

        // Act
        response.Valid = true;
        response.Name = "st-test-dev-001";
        response.Message = "Name is valid";

        // Assert
        response.Valid.Should().BeTrue();
        response.Name.Should().Be("st-test-dev-001");
        response.Message.Should().Be("Name is valid");
    }
}

public class AdminUserTests
{
    [Fact]
    public void AdminUser_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new AdminUser();

        // Assert
        user.Id.Should().Be(0);
        user.Name.Should().BeEmpty();
    }

    [Fact]
    public void AdminUser_ShouldSetIdProperty()
    {
        // Arrange
        var user = new AdminUser();

        // Act
        user.Id = 42;

        // Assert
        user.Id.Should().Be(42);
    }

    [Fact]
    public void AdminUser_ShouldSetNameProperty()
    {
        // Arrange
        var user = new AdminUser();

        // Act
        user.Name = "TestUser";

        // Assert
        user.Name.Should().Be("TestUser");
    }
}
