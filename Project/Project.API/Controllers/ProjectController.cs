using Microsoft.AspNetCore.Mvc;
using Project.API.Applications.Commands;
using MediatR;
using System.Runtime.Versioning;
using Project.API.Applications.Service;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : BaseController
{

    private readonly IMediator _mediator;
    private IRecommendService _recommendService;
    public ProjectController(IMediator mediator, IRecommendService recommendService)
    {
        _mediator = mediator;
        _recommendService = recommendService;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> CreateProject([FromBody] Project.Domain.AggregatesModel.Project project)
    {
        var command = new CreateProjectCommand
        {
            Project = project
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut]
    [Route("view/{projectId}")]
    public async Task<IActionResult> ViewProject(int projectId)
    {
        if (await _recommendService.IsProjectInRecommendAsync(projectId, UserIdentity.UserId) == false)
        {
            return BadRequest("没有查看该项目的权限");
        }

        var command = new ViewProjectCommand
        {
            ProjectId = projectId,
            UserId = UserIdentity.UserId,
            UserName = UserIdentity.Name,
            Avatar = UserIdentity.Avatar
        };

        await _mediator.Send(command);
        return Ok();
    }

    [HttpPut]
    [Route("join/{projectId}")]
    public async Task<IActionResult> JoinProject([FromBody] Project.Domain.AggregatesModel.ProjectContributor contributor)
    {
        if (await _recommendService.IsProjectInRecommendAsync(contributor.ProjectId, UserIdentity.UserId) == false)
        {
            return BadRequest("没有查看该项目的权限");
        }
        var command = new JoinProjectCommand
        {
            ProjectContributor = contributor
        };

        await _mediator.Send(command);
        return Ok();
    }
}
