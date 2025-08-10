namespace Contact.API.Dtos;

public class BaseUserInfo
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 公司
    /// </summary>
    public string Company { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    public string Title { get; set; }
}