using Microsoft.AspNetCore.Mvc;
using Recommend.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendController : BaseController
{

    private readonly ILogger<RecommendController> _logger;
    private readonly RecommendDBContext _dbContext;

    public RecommendController(ILogger<RecommendController> logger, RecommendDBContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> Get()
    {
        var result = await _dbContext.ProjectRecommends.AsNoTracking().Where(r => r.UserId == UserIdentity.UserId).ToListAsync();
        //var result = await _dbContext.ProjectRecommends.AsNoTracking().ToListAsync();
        return Ok(result);
    }
}
