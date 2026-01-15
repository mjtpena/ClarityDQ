using ClarityDQ.Api.Controllers;
using ClarityDQ.Core.Entities;
using ClarityDQ.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace ClarityDQ.Tests.Controllers;

public class RulesControllerTests
{
    private readonly Mock<IRuleService> _mockService;
    private readonly Mock<ILogger<RulesController>> _mockLogger;
    private readonly RulesController _controller;

    public RulesControllerTests()
    {
        _mockService = new Mock<IRuleService>();
        _mockLogger = new Mock<ILogger<RulesController>>();
        _controller = new RulesController(_mockService.Object, _mockLogger.Object);
        
        // Setup controller context with user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "test-user"),
        }, "mock"));
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreateRule_ReturnsCreatedResult()
    {
        var request = new CreateRuleRequest(
            "Test Rule",
            "Description",
            RuleType.Completeness,
            "ws-1",
            "ds-1",
            "t-1",
            "col-1",
            "expression",
            95.0,
            RuleSeverity.High,
            true);

        var expectedRule = new Rule { Id = Guid.NewGuid(), Name = "Test Rule" };
        _mockService.Setup(s => s.CreateRuleAsync(It.IsAny<Rule>(), default))
            .ReturnsAsync(expectedRule);

        var result = await _controller.CreateRule(request);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().Be(expectedRule);
    }

    [Fact]
    public async Task GetRule_ReturnsOk_WhenExists()
    {
        var ruleId = Guid.NewGuid();
        var rule = new Rule { Id = ruleId, Name = "Test" };

        _mockService.Setup(s => s.GetRuleAsync(ruleId, default))
            .ReturnsAsync(rule);

        var result = await _controller.GetRule(ruleId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(rule);
    }

    [Fact]
    public async Task GetRule_ReturnsNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetRuleAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Rule?)null);

        var result = await _controller.GetRule(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetRules_ReturnsOk_WithRules()
    {
        var rules = new List<Rule>
        {
            new() { Id = Guid.NewGuid(), Name = "Rule 1" },
            new() { Id = Guid.NewGuid(), Name = "Rule 2" }
        };

        _mockService.Setup(s => s.GetRulesAsync("ws-1", null, default))
            .ReturnsAsync(rules);

        var result = await _controller.GetRules("ws-1");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(rules);
    }

    [Fact]
    public async Task UpdateRule_ReturnsOk_WhenExists()
    {
        var ruleId = Guid.NewGuid();
        var existing = new Rule { Id = ruleId, Name = "Old" };
        var request = new UpdateRuleRequest("New", "Desc", "expr", 90.0, RuleSeverity.Low, true);

        _mockService.Setup(s => s.GetRuleAsync(ruleId, default))
            .ReturnsAsync(existing);
        _mockService.Setup(s => s.UpdateRuleAsync(It.IsAny<Rule>(), default))
            .ReturnsAsync(existing);

        var result = await _controller.UpdateRule(ruleId, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(existing);
    }

    [Fact]
    public async Task UpdateRule_ReturnsNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetRuleAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Rule?)null);

        var request = new UpdateRuleRequest("Name", "Desc", "expr", 90, RuleSeverity.Low, true);
        var result = await _controller.UpdateRule(Guid.NewGuid(), request);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteRule_ReturnsNoContent()
    {
        var ruleId = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteRuleAsync(ruleId, default))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteRule(ruleId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ExecuteRule_ReturnsOk_WithExecution()
    {
        var ruleId = Guid.NewGuid();
        var execution = new RuleExecution { Id = Guid.NewGuid(), RuleId = ruleId };

        _mockService.Setup(s => s.ExecuteRuleAsync(ruleId, default))
            .ReturnsAsync(execution);

        var result = await _controller.ExecuteRule(ruleId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(execution);
    }

    [Fact]
    public async Task ExecuteRule_ReturnsNotFound_WhenRuleNotExists()
    {
        _mockService.Setup(s => s.ExecuteRuleAsync(It.IsAny<Guid>(), default))
            .ThrowsAsync(new InvalidOperationException("Not found"));

        var result = await _controller.ExecuteRule(Guid.NewGuid());

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetRuleExecutions_ReturnsOk_WithExecutions()
    {
        var ruleId = Guid.NewGuid();
        var executions = new List<RuleExecution>
        {
            new() { Id = Guid.NewGuid(), RuleId = ruleId },
            new() { Id = Guid.NewGuid(), RuleId = ruleId }
        };

        _mockService.Setup(s => s.GetRuleExecutionsAsync(ruleId, 0, 50, default))
            .ReturnsAsync(executions);

        var result = await _controller.GetRuleExecutions(ruleId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(executions);
    }
}
