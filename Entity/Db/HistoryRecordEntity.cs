using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatGPT_API.Entity.Db;

/// <summary>
/// 数据库中的历史记录表
/// </summary>
public class HistoryRecordEntity
{
    /// <summary>
    /// 历史记录id
    /// </summary>
    [Key]
    [Column("id")]
    //id自增
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// 历史记录列表 用类似1,2,3的方式存储映射到HistoryMessagesEntity的id
    /// </summary>
    [Column("messages")]
    public string Messages { get; set; }

    /// <summary>
    /// 记录历史记录的用户
    /// </summary>
    [ForeignKey("UserEnity")]
    [Column("uid")]
    public string UID { get; set; }
}