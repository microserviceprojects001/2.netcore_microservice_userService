using Microsoft.AspNetCore.Mvc;
using Project.API.Applications.Commands;
using MediatR;
using System.Runtime.Versioning;
using Project.API.Applications.Service;
using Project.API.Applications.Queries;
using Project.API.Dtos;

namespace Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : BaseController
{

    private readonly IMediator _mediator;
    private IRecommendService _recommendService;

    private IProjectQueries _projectQueries;

    public ProjectController(IMediator mediator, IRecommendService recommendService, IProjectQueries projectQueries)
    {
        _mediator = mediator;
        _recommendService = recommendService;
        _projectQueries = projectQueries;
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetProjects()
    {
        var projects = await _projectQueries.GetProjectByUserIdAsync(UserIdentity.UserId);
        return Ok(projects);
    }
    [HttpGet]
    [Route("my/{projectId}")]
    public async Task<IActionResult> GetMyProjectDetail(int projectId)
    {
        var projectDetail = await _projectQueries.GetProjectDetailAsync(projectId);
        if (projectDetail.UserId != UserIdentity.UserId)
        {
            return BadRequest("没有查看该项目的权限");
        }
        return Ok(projectDetail);
    }
    /// <summary>
    /// api/projects/recommends/{projectId}
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("recommends/{projectId}")]
    public async Task<IActionResult> GetRecommendProjectDetail(int projectId)
    {
        if (await _recommendService.IsProjectInRecommendAsync(projectId, UserIdentity.UserId) == false)
        {
            return BadRequest("没有查看该项目的权限");
        }

        var projectDetail = await _projectQueries.GetProjectDetailAsync(projectId);
        return Ok(projectDetail);
    }



    [HttpPost]
    [Route("")]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createProjectDto)
    {
        if (createProjectDto == null)
        {
            return BadRequest("请求数据不能为空");
        }

        var command = new CreateProjectCommand
        {
            CreateProjectDto = createProjectDto,
            UserId = UserIdentity.UserId,
            UserName = UserIdentity.Name,
            Avatar = UserIdentity.Avatar
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
    public async Task<IActionResult> JoinProject(int projetId, [FromBody] Project.Domain.AggregatesModel.ProjectContributor contributor)
    {
        if (await _recommendService.IsProjectInRecommendAsync(projetId, UserIdentity.UserId) == false)
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
