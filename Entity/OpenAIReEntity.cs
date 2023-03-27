namespace ChatGPT_API.Entity;

/// <summary>
/// OpenAI返回的实体
/// </summary>
public class OpenAIReEntity
{
    /// <summary>
    /// 
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string @object { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int created { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<ChoicesItem> choices { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Usage usage { get; set; }

    public class Usage
    {
        /// <summary>
        /// 
        /// </summary>
        public int prompt_tokens { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int completion_tokens { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int total_tokens { get; set; }
    }

    public class ChoicesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Message message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string finish_reason { get; set; }
    }
}

/// <summary>
/// 消息传递
/// </summary>
public class Message
{
    /// <summary>
    /// 
    /// </summary>
    public string role { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string content { get; set; }
}