using FluentAssertions;
using SecureAppServiceApiLite.Api.Contracts;
using SecureAppServiceApiLite.Api.Validation;

namespace SecureAppServiceApiLite.Api.Tests;

public sealed class CreateMessageRequestValidatorTests
{
    private readonly CreateMessageRequestValidator _validator = new();

    [Fact]
    public void Validate_ShouldSucceed_WhenRequestIsValid()
    {
        var request = new CreateMessageRequest(
            Subject: "Deployment status",
            Body: "The deployment completed successfully.",
            Priority: "High");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenSubjectIsEmpty(string subject)
    {
        var request = new CreateMessageRequest(subject, "Body", "Normal");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateMessageRequest.Subject));
    }

    [Fact]
    public void Validate_ShouldFail_WhenSubjectExceedsMaxLength()
    {
        var request = new CreateMessageRequest(new string('S', 121), "Body", "Normal");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateMessageRequest.Subject));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenBodyIsEmpty(string body)
    {
        var request = new CreateMessageRequest("Subject", body, "Normal");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateMessageRequest.Body));
    }

    [Fact]
    public void Validate_ShouldFail_WhenBodyExceedsMaxLength()
    {
        var request = new CreateMessageRequest("Subject", new string('B', 2001), "Normal");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateMessageRequest.Body));
    }

    [Fact]
    public void Validate_ShouldAllowMissingPriority()
    {
        var request = new CreateMessageRequest("Subject", "Body", null);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Normal")]
    [InlineData("High")]
    [InlineData("low")]
    [InlineData("HIGH")]
    public void Validate_ShouldAllowSupportedPriorities(string priority)
    {
        var request = new CreateMessageRequest("Subject", "Body", priority);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenPriorityIsUnsupported()
    {
        var request = new CreateMessageRequest("Subject", "Body", "Urgent");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateMessageRequest.Priority));
    }
}
