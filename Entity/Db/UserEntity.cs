using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatGPT_API.Entity.Db;

/// <summary>
/// 数据库中的用户表
/// </summary>
public class UserEntity
{
    /// <summary>
    /// 用户UID
    /// </summary>
    [Key]
    [Column("uid")]
    public string UID { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Column("userName")]
    public string UserName { get; set; }

    /// <summary>
    /// 用户密码（md5）
    /// </summary>
    [Column("Password")]
    public string Password { get; set; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    [Column("email")]
    public string Email { get; set; }

    /// <summary>
    /// 用户等级 -1:管理员 0:未登录 1:普通用户 2:激活用户 
    /// </summary>
    [Column("grade")]
    public int Grade { get; set; }

    /// <summary>
    /// 剩余时间
    /// </summary>
    [Column("expireDate")]
    public DateTime ExpireDate { get; set; }

    /// <summary>
    /// 可用次数 -1:不限制
    /// </summary>
    [Column("remainingTimes")]
    public int RemainingTimes { get; set; } = 10;
}