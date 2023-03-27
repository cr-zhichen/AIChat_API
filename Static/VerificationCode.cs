using ChatGPT_API.Controllers;

namespace ChatGPT_API.Static;

/// <summary>
/// 验证码保存类
/// </summary>
public static class VerificationCode
{
    public static List<VerificationCodeTemporaryStorage> VerificationCodeList = new();

    /// <summary>
    /// 验证码暂存
    /// </summary>
    public class VerificationCodeTemporaryStorage
    {
        public string Email { get; set; }
        public DateTime ExpireTime { get; set; }
        public int Code { get; set; }
    }
}