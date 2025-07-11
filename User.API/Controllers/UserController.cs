using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace User.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : BaseController
{
    private ILogger<UserController> _logger;
    private readonly UserContext _context;
    public UserController(UserContext context, ILogger<UserController> logger)
    {
        _logger = logger;
        _context = context;
    }

    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var user = await _context.AppUsers
            .AsNoTracking()
            .Include(u => u.Properties)
            .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);
        if (user == null)
        {
            throw new UserOperationException($"错误的用户上下文Id {UserIdentity.UserId}");
        }
        return user == null ? NotFound() : Ok(user);
    }
    #region test code
    // can work
    // [HttpPatch]
    // public async Task<IActionResult> Patch([FromBody] JsonPatchDocument<AppUser> patch)
    // {
    //     if (patch == null) return BadRequest("Invalid patch document.");

    //     var user = await _context.AppUsers
    //         .Include(u => u.Properties)
    //         .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

    //     if (user == null) return NotFound($"用户不存在: {UserIdentity.UserId}");

    //     // 保存原始属性集合
    //     var originalProperties = user.Properties.ToList();

    //     // 应用Patch
    //     patch.ApplyTo(user);

    //     // 获取当前属性集合
    //     var currentProperties = user.Properties.ToList();

    //     // 找出需要删除的属性：在原始集合中但不在当前集合中
    //     var toRemove = originalProperties
    //         .Where(op => !currentProperties.Any(cp =>
    //             cp.Key == op.Key && cp.Value == op.Value))
    //         .ToList();

    //     // 找出需要添加的属性：在当前集合中但不在原始集合中
    //     var toAdd = currentProperties
    //         .Where(cp => !originalProperties.Any(op =>
    //             op.Key == cp.Key && op.Value == cp.Value))
    //         .ToList();

    //     // 更新数据库
    //     foreach (var prop in toRemove)
    //     {
    //         _context.UserProperties.Remove(prop);
    //     }

    //     foreach (var prop in toAdd)
    //     {
    //         prop.AppUserId = user.Id;

    //         // 安全添加（检查是否已存在）
    //         if (!await _context.UserProperties.AnyAsync(p =>
    //             p.AppUserId == user.Id &&
    //             p.Key == prop.Key &&
    //             p.Value == prop.Value))
    //         {
    //             _context.UserProperties.Add(prop);
    //         }
    //     }

    //     await _context.SaveChangesAsync();
    //     return Ok(user);
    // }

    // now  can work
    // [Route("")]
    // [HttpPatch]
    // public async Task<IActionResult> Patch([FromBody] JsonPatchDocument<AppUser> patch)
    // {
    //     if (patch == null)
    //     {
    //         return BadRequest("Invalid patch document.");
    //     }

    //     var user = await _context.AppUsers.Include(u => u.Properties).SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

    //     if (user == null)
    //     {
    //         throw new UserOperationException($"错误的用户上下文Id {UserIdentity.UserId}");
    //     }
    //     //var originalProperties = user.Properties.ToList();
    //     patch.ApplyTo(user);

    //     // foreach (var property in user.Properties)
    //     // {
    //     //     _context.Entry<UserProperty>(property).State = EntityState.Detached;
    //     // }

    //     var originalProperties = await _context.UserProperties.Where(up => up.AppUserId == UserIdentity.UserId).ToListAsync();

    //     var allProperties = originalProperties.Union(user.Properties).Distinct();

    //     var removedProperties = originalProperties.Except(user.Properties);
    //     var newProperties = allProperties.Except(originalProperties);
    //     foreach (var property in removedProperties)
    //     {
    //         //_context.Entry<UserProperty>(property).State = EntityState.Deleted;

    //         _context.UserProperties.Remove(property);
    //     }

    //     foreach (var property in newProperties)
    //     {
    //         //_context.Entry<UserProperty>(property).State = EntityState.Added;

    //         _context.UserProperties.Add(property);
    //     }

    //     //_context.AppUsers.Update(user);
    //     _context.SaveChanges();

    //     return Ok(user);
    // }

    //不能工作的，
    //比方说数据库里有A+轮，现在postman请求的数据有A+轮和B+轮，
    //那么就会报错，因为A+轮已经存在了，不能重复添加。
    //patch.ApplyTo(user); UserProperties 有两个，但是
    //var originalProperties = await _context.UserProperties.Where(up => up.AppUserId == UserIdentity.UserId).ToListAsync();
    // 执行完这句话后，就变成了三个
    // 也就是原来的A+轮和B+轮，还有一个A+轮


    // [Route("")]
    // [HttpPatch]
    // public async Task<IActionResult> Patch([FromBody] JsonPatchDocument<AppUser> patch)
    // {
    //     if (patch == null)
    //     {
    //         return BadRequest("Invalid patch document.");
    //     }

    //     var user = await _context.AppUsers.SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

    //     if (user == null)
    //     {
    //         throw new UserOperationException($"错误的用户上下文Id {UserIdentity.UserId}");
    //     }

    //     patch.ApplyTo(user);
    //     foreach (var property in user.Properties)
    //     {
    //         _context.Entry<UserProperty>(property).State = EntityState.Detached;
    //     }
    //     var originalProperties = await _context.UserProperties.Where(up => up.AppUserId == UserIdentity.UserId).ToListAsync();

    //     var allProperties = originalProperties.Union(user.Properties).Distinct();

    //     var removedProperties = originalProperties.Except(user.Properties);
    //     var newProperties = allProperties.Except(originalProperties);
    //     foreach (var property in removedProperties)
    //     {
    //         //_context.Entry<UserProperty>(property).State = EntityState.Deleted;

    //         _context.UserProperties.Remove(property);
    //     }

    //     foreach (var property in newProperties)
    //     {
    //         //_context.Entry<UserProperty>(property).State = EntityState.Added;

    //         _context.UserProperties.Add(property);
    //     }

    //     _context.AppUsers.Update(user);
    //     await _context.SaveChangesAsync();

    //     return Ok(user);
    // }

    // 加上Include 后工作了
    // [Route("")]
    // [HttpPatch]
    // public async Task<IActionResult> Patch([FromBody] JsonPatchDocument<AppUser> patch)
    // {
    //     if (patch == null)
    //     {
    //         return BadRequest("Invalid patch document.");
    //     }

    //     var user = await _context.AppUsers.Include(u => u.Properties).SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

    //     if (user == null)
    //     {
    //         throw new UserOperationException($"错误的用户上下文Id {UserIdentity.UserId}");
    //     }

    //     patch.ApplyTo(user);

    //     var originalProperties = await _context.UserProperties.Where(up => up.AppUserId == UserIdentity.UserId).ToListAsync();

    //     var allProperties = originalProperties.Union(user.Properties).Distinct();

    //     var removedProperties = originalProperties.Except(user.Properties);
    //     var newProperties = allProperties.Except(originalProperties);
    //     foreach (var property in removedProperties)
    //     {
    //         //_context.Entry<UserProperty>(property).State = EntityState.Deleted;

    //         _context.UserProperties.Remove(property);
    //     }

    //     foreach (var property in newProperties)
    //     {
    //         //_context.Entry<UserProperty>(property).State = EntityState.Added;

    //         _context.UserProperties.Add(property);
    //     }

    //     _context.AppUsers.Update(user);
    //     await _context.SaveChangesAsync();

    //     return Ok(user);
    // }
    #endregion

    //使用优化后的代码
    [Route("")]
    [HttpPatch]
    public async Task<IActionResult> Patch([FromBody] JsonPatchDocument<AppUser> patch)
    {
        if (patch == null)
        {
            return BadRequest("Invalid patch document.");
        }

        // 关键修改：添加 Include 加载导航属性
        var user = await _context.AppUsers
            .Include(u => u.Properties) // 确保加载属性集合
            .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

        if (user == null)
        {
            throw new UserOperationException($"错误的用户上下文Id {UserIdentity.UserId}");
        }

        // 创建原始属性快照（此时已被跟踪）
        var originalProperties = user.Properties.ToList();

        // 应用补丁修改
        patch.ApplyTo(user);

        // 使用业务键比较变更（而非引用比较）
        var removedProperties = originalProperties
            .Where(op => !user.Properties.Any(p =>
                p.Key == op.Key &&
                p.Value == op.Value))
            .ToList();

        var newProperties = user.Properties
            .Where(p => !originalProperties.Any(op =>
                op.Key == p.Key &&
                p.Value == op.Value))
            .ToList();

        // 处理删除和添加
        foreach (var property in removedProperties)
        {
            _context.UserProperties.Remove(property);
        }

        foreach (var property in newProperties)
        {
            _context.UserProperties.Add(property);
        }

        // 不需要调用 Update(user)，因为实体已被跟踪
        await _context.SaveChangesAsync();

        return Ok(user);
    }
}