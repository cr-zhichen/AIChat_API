using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatGPT_API.Entity.Db;

/// <summary>
/// 用来保存历史记录中的消息
/// </summary>
public class HistoryMessagesEntity
{
    /// <summary>
    /// 历史记录消息的id
    /// </summary>
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    /// <summary>
    /// 消息的发送人
    /// </summary>
    [Column("role")]
    public string Role { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    [Column("content")]
    public string Content { get; set; }
}