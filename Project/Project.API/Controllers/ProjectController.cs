using Microsoft.AspNetCore.Mvc;
using Project.API.Applications.Commands;
using MediatR;
using System.Runtime.Versioning;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : BaseController
{

    private readonly IMediator _mediator;

    public ProjectController(IMediator mediator)
    {
        _mediator = mediator;
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
        var command = new ViewProjectCommand
        {
            ProjectId = projectId,
            UserId = UserIdentity.UserId,
            UserName = UserIdentity.Name,
            Avatar = UserIdentity.Avatar
        };

        await _mediator.Send(command);
        return Ok(true);
    }

    [HttpPut]
    [Route("join/{projectId}")]
    public async Task<IActionResult> JoinProject([FromBody] Project.Domain.AggregatesModel.ProjectContributor contributor)
    {
        var command = new JoinProjectCommand
        {
            ProjectContributor = contributor
        };

        await _mediator.Send(command);
        return Ok(true);
    }
}
