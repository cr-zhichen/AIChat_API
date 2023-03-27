using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatGPT_API.Entity.Db;

/// <summary>
/// 用来保存激活码
/// </summary>
public class ActivationCodeEntity
{
    /// <summary>
    /// 激活码id
    /// </summary>
    [Key]
    [Column("id")]
    //id自增
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    /// <summary>
    /// 激活码
    /// </summary>
    [Column("code")]
    public string Code { get; set; }

    /// <summary>
    /// 激活码等级
    /// </summary>
    [Column("codeGrade")]
    public CodeGradeEntity CodeGrade { get; set; }

    /// <summary>
    /// 激活码的剩余使用次数
    /// </summary>
    [Column("remainingTimes")]
    public int RemainingTimes { get; set; }
}