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

public class ResourceNameRequestTests
{
    [Fact]
    public void ResourceNameRequest_ShouldInitializeWithDefaultValues()
    {
        // Act
        var request = new ResourceNameRequest();

        // Assert
        request.ResourceEnvironment.Should().BeEmpty();
        request.ResourceFunction.Should().BeEmpty();
        request.ResourceInstance.Should().BeEmpty();
        request.ResourceLocation.Should().BeEmpty();
        request.ResourceOrg.Should().BeEmpty();
        request.ResourceProjAppSvc.Should().BeEmpty();
        request.ResourceType.Should().BeEmpty();
        request.ResourceUnitDept.Should().BeEmpty();
    }

    [Fact]
    public void ResourceNameRequest_ShouldSetAllProperties()
    {
        // Arrange
        var request = new ResourceNameRequest();

        // Act
        request.ResourceEnvironment = "dev";
        request.ResourceFunction = "web";
        request.ResourceInstance = "001";
        request.ResourceLocation = "eastus";
        request.ResourceOrg = "contoso";
        request.ResourceProjAppSvc = "myapp";
        request.ResourceType = "Microsoft.Storage/storageAccounts";
        request.ResourceUnitDept = "marketing";

        // Assert
        request.ResourceEnvironment.Should().Be("dev");
        request.ResourceFunction.Should().Be("web");
        request.ResourceInstance.Should().Be("001");
        request.ResourceLocation.Should().Be("eastus");
        request.ResourceOrg.Should().Be("contoso");
        request.ResourceProjAppSvc.Should().Be("myapp");
        request.ResourceType.Should().Be("Microsoft.Storage/storageAccounts");
        request.ResourceUnitDept.Should().Be("marketing");
    }
}

public class CustomComponentTests
{
    [Fact]
    public void CustomComponent_ShouldInitializeWithDefaultValues()
    {
        // Act
        var component = new CustomComponent();

        // Assert
        component.Id.Should().Be(0);
        component.ParentComponent.Should().BeEmpty();
        component.Name.Should().BeEmpty();
        component.ShortName.Should().BeEmpty();
        component.SortOrder.Should().Be(0);
        component.MinLength.Should().Be("1");
    }

    [Fact]
    public void CustomComponent_ShouldSetShortNameProperty()
    {
        // Arrange
        var component = new CustomComponent();

        // Act
        component.ShortName = "tst";

        // Assert
        component.ShortName.Should().Be("tst");
    }

    [Fact]
    public void CustomComponent_ShouldSetAllProperties()
    {
        // Arrange
        var component = new CustomComponent();

        // Act
        component.Id = 1;
        component.ParentComponent = "ResourceEnvironment";
        component.Name = "Testing";
        component.ShortName = "tst";
        component.SortOrder = 5;
        component.MinLength = "2";

        // Assert
        component.Id.Should().Be(1);
        component.ParentComponent.Should().Be("ResourceEnvironment");
        component.Name.Should().Be("Testing");
        component.ShortName.Should().Be("tst");
        component.SortOrder.Should().Be(5);
        component.MinLength.Should().Be("2");
    }
}

public class GeneratedNameTests
{
    [Fact]
    public void GeneratedName_ShouldInitializeWithDefaultValues()
    {
        // Act
        var generatedName = new GeneratedName();

        // Assert
        generatedName.Id.Should().Be(0);
        generatedName.CreatedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        generatedName.ResourceName.Should().BeEmpty();
        generatedName.ResourceTypeName.Should().BeEmpty();
        generatedName.Components.Should().BeEmpty();
        generatedName.User.Should().Be("General");
        generatedName.Message.Should().BeNull();
    }

    [Fact]
    public void GeneratedName_ShouldSetAllProperties()
    {
        // Arrange
        var generatedName = new GeneratedName();
        var testDate = DateTime.Now.AddDays(-1);
        var components = new List<string[]> { new[] { "env", "dev" }, new[] { "location", "eastus" } };

        // Act
        generatedName.Id = 42;
        generatedName.CreatedOn = testDate;
        generatedName.ResourceName = "st-test-dev-001";
        generatedName.ResourceTypeName = "Storage Account";
        generatedName.Components = components;
        generatedName.User = "admin@contoso.com";
        generatedName.Message = "Test message";

        // Assert
        generatedName.Id.Should().Be(42);
        generatedName.CreatedOn.Should().Be(testDate);
        generatedName.ResourceName.Should().Be("st-test-dev-001");
        generatedName.ResourceTypeName.Should().Be("Storage Account");
        generatedName.Components.Should().HaveCount(2);
        generatedName.User.Should().Be("admin@contoso.com");
        generatedName.Message.Should().Be("Test message");
    }
}
