using System.ComponentModel.DataAnnotations.Schema;

namespace ChatGPT_API.Entity.Db;

/// <summary>
/// 记录激活码等级的表
/// </summary>
public class CodeGradeEntity
{
    /// <summary>
    /// id
    /// </summary>
    [Column("id")]
    //id自增
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    /// <summary>
    /// 可使用时间 -1:永久 其他:时间（单位天）
    /// </summary>
    [Column("usageTime")]
    public int UsageTime { get; set; }

    /// <summary>
    /// 周期内可使用次数 -1:无限 其他:次数
    /// </summary>
    [Column("usageFrequency")]
    public int UsageFrequency { get; set; }
}