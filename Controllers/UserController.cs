using System.Security.Claims;
using ChatGPT_API.Context;
using ChatGPT_API.Entity;
using ChatGPT_API.Entity.Db;
using ChatGPT_API.Entity.Re;
using ChatGPT_API.Static;
using ChatGPT_API.Tool;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace ChatGPT_API.Controllers;

/// <summary>
/// 涉及到用户的接口
/// </summary>
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    JObject configJson =
        JObject.Parse(System.IO.File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));

    private readonly DbLinkContext _dbLinkContext;
    private readonly JwtHelper _jwtHelper;

    public UserController(DbLinkContext dbLinkContext, JwtHelper jwtHelper)
    {
        _dbLinkContext = dbLinkContext;
        _jwtHelper = jwtHelper;
    }


    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("Register")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public async Task<Re> Register(RegisterData data, int code)
    {
        try
        {
            //=========判断能否注册=========

            if (!data.Email.IsEmail())
            {
                return new Re()
                {
                    code = -1,
                    msg = "邮箱格式错误",
                    data = null
                };
            }

            //判断邮箱是否已经注册
            if (_dbLinkContext.User.Any(x => x.Email == data.Email))
            {
                return new Re()
                {
                    code = -1,
                    msg = "邮箱已被注册",
                    data = null
                };
            }

            //检查验证码是否正确
            bool isCodeOk = VerificationCode.VerificationCodeList.Any(item =>
                code == item.Code && item.ExpireTime > DateTime.Now && item.Email == data.Email);

            if (!isCodeOk)
            {
                return new Re()
                {
                    code = -1,
                    msg = "验证码错误",
                    data = null
                };
            }

            //删除验证码
            VerificationCode.VerificationCodeList.RemoveAll(item => item.Email == data.Email);


            //=========向数据库添加用户=========

            string UID = Guid.NewGuid().ToString();
            _dbLinkContext.User.Add(new UserEntity()
            {
                UID = UID,
                UserName = data.UserName,
                Password = data.PasswordMd5,
                Email = data.Email,
                Grade = 1,
                RemainingTimes = 20,
                ExpireDate = DateTime.Now + TimeSpan.FromDays(1),
            });


            var c = await _dbLinkContext.SaveChangesAsync();
            if (c == 0)
            {
                return new Re()
                {
                    code = -1,
                    msg = "注册失败",
                    data = null
                };
            }

            //=========返回成功=========

            return new Re()
            {
                code = 0,
                msg = "注册成功",
                data = new
                {
                    userName = data.UserName,
                    token = _jwtHelper.CreateToken(new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, data.UserName),
                        new Claim(ClaimTypes.Email, data.Email),
                        new Claim(ClaimTypes.UserData, UID),
                        new Claim(ClaimTypes.Role, "1")
                    })
                }
            };
        }
        catch (Exception e)
        {
            return new Re()
            {
                code = -1,
                msg = e.Message,
                data = null
            };
        }
    }


    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="data"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPost("ResetPassword")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public async Task<Re> ResetPassword(RegisterData data, int code)
    {
        if (!data.Email.IsEmail())
        {
            return new Re()
            {
                code = -1,
                msg = "邮箱格式错误",
                data = null
            };
        }

        //判断邮箱是否已经注册
        if (!_dbLinkContext.User.Any(x => x.Email == data.Email))
        {
            return new Re()
            {
                code = -1,
                msg = "邮箱未注册",
                data = null
            };
        }

        //检查验证码是否正确
        bool isCodeOK = VerificationCode.VerificationCodeList.Any(item =>
            code == item.Code && item.ExpireTime > DateTime.Now && item.Email == data.Email);

        //验证码错误
        if (!isCodeOK)
        {
            return new Re()
            {
                code = -1,
                msg = "验证码错误",
                data = null
            };
        }

        //删除验证码
        VerificationCode.VerificationCodeList.RemoveAll(item => item.Email == data.Email);

        //重置密码
        var user = await _dbLinkContext.User.FirstOrDefaultAsync(x => x.Email == data.Email);
        user.Password = data.PasswordMd5;
        _dbLinkContext.User.Update(user);
        var c = await _dbLinkContext.SaveChangesAsync();
        if (c == 0)
        {
            return new Re()
            {
                code = -1,
                msg = "重置密码失败",
                data = null
            };
        }
        else
        {
            return new Re()
            {
                code = 0,
                msg = "重置密码成功",
                data = null
            };
        }
    }

    /// <summary>
    /// 发送邮箱验证码
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("VenerateVerificationCode")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public async Task<Re> VenerateVerificationCode(string email)
    {
        //=========判断能否注册=========

        if (!email.IsEmail())
        {
            return new Re()
            {
                code = -1,
                msg = "邮箱格式错误",
                data = null
            };
        }

        //生成验证码
        var code = new Random().Next(100000, 999999);
        //将验证码保存到内存
        VerificationCode.VerificationCodeList.Add(new()
        {
            Code = code,
            ExpireTime = DateTime.Now.AddMinutes(10),
            Email = email
        });

        //发送验证码到邮箱
        SMTPMail smtpMail = new SMTPMail(new MailConfiguration()
        {
            smtpService = configJson["SMTP"]["SmtpService"].ToString(),
            sendEmail = configJson["SMTP"]["SendEmail"].ToString(),
            sendPwd = configJson["SMTP"]["SendPwd"].ToString(),
            port = int.Parse(configJson["SMTP"]["Port"].ToString()),
            reAddress = email,
            subject = "验证码",
            body = $"您的验证码为：{code}，请在10分钟内使用"
        });

        if (smtpMail.Send())
        {
            return new Re()
            {
                code = 0,
                msg = "验证码已发送",
                data = null
            };
        }
        else
        {
            return new Re()
            {
                code = -1,
                msg = "验证码发送失败",
                data = null
            };
        }
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("Login")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public async Task<Re> Login(LoginData data)
    {
        try
        {
            //=========判断能否登录=========
            var userEntities = _dbLinkContext.User
                .Where(x => x.Email == data.Email && x.Password == data.PasswordMd5);

            if (!userEntities.Any())
            {
                return new Re()
                {
                    code = -1,
                    msg = "用户名或密码错误",
                    data = null
                };
            }

            //=========返回登录成功=========

            return new Re()
            {
                code = 0,
                msg = "登录成功",
                data = new
                {
                    userName = userEntities.First().UserName,
                    role = userEntities.First().Grade,
                    token = _jwtHelper.CreateToken(new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, userEntities.First().UserName),
                        new Claim(ClaimTypes.Email, userEntities.First().Email),
                        new Claim(ClaimTypes.UserData, userEntities.First().UID),
                        new Claim(ClaimTypes.Role, userEntities.First().Grade.ToString())
                    })
                }
            };
        }
        catch (Exception e)
        {
            return new Re()
            {
                code = -1,
                msg = e.Message,
                data = null
            };
        }
    }

    /// <summary>
    /// 游客登录
    /// </summary>
    /// <returns></returns>
    [HttpPost("Tourists")]
    [AllowAnonymous]
    [EnableCors("AllowAll")]
    public async Task<Re> Tourists()
    {
        try
        {
            string UID = Guid.NewGuid().ToString();

            //=========向数据库添加游客=========
            _dbLinkContext.User.Add(new UserEntity()
            {
                UID = UID,
                UserName = "",
                Password = "",
                Email = "",
                Grade = 0,
                ExpireDate = DateTime.Now.AddDays(1),
            });
            await _dbLinkContext.SaveChangesAsync();

            //返回Token
            return new Re()
            {
                code = 0,
                msg = "登录成功",
                data = new
                {
                    userName = "游客",
                    role = 0,
                    token = _jwtHelper.CreateToken(new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, "游客"),
                        new Claim(ClaimTypes.Role, "0"),
                        new Claim(ClaimTypes.UserData, UID)
                    })
                }
            };
        }
        catch (Exception e)
        {
            return new Re()
            {
                code = -1,
                msg = e.Message,
                data = null
            };
        }
    }

    /// <summary>
    /// 生成激活码
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("GenerateActivationCode")]
    [Authorize(Roles = "-1")]
    [EnableCors("AllowAll")]
    public async Task<Re> GenerateActivationCode(GenerateActivationCodeClass data)
    {
        try
        {
            List<ActivationCodeEntity> activationCodeEntities = new();
            for (int i = 0; i < data.num; i++)
            {
                //生成激活码
                string code = Guid.NewGuid().ToString().Replace("-", "");

                ActivationCodeEntity activationCodeEntity = new()
                {
                    Code = code,
                    CodeGrade = data.codeGrade,
                    RemainingTimes = data.usageCount,
                };
                //添加到数据库
                _dbLinkContext.ActivationCode.Add(activationCodeEntity);
                await _dbLinkContext.SaveChangesAsync();
                activationCodeEntities.Add(activationCodeEntity);
            }


            return new Re()
            {
                code = 0,
                msg = "生成成功",
                data = activationCodeEntities
            };
        }
        catch (Exception e)
        {
            return new Re()
            {
                code = -1,
                msg = e.Message,
                data = null
            };
        }
    }

    /// <summary>
    /// 使用激活码
    /// </summary>
    /// <returns></returns>
    [HttpPost("UseActivationCode")]
    [Authorize(Roles = "1,2")]
    [EnableCors("AllowAll")]
    public async Task<Re> UseActivationCode(ActivationCode data)
    {
        try
        {
            //根据Token获取用户信息
            var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var claims = claimsIdentity?.Claims;
            var email = claims?.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value;
            var role = claims?.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value;

            //提取验证码
            var activationCodeEntity = _dbLinkContext.ActivationCode.Include(ac => ac.CodeGrade)
                .FirstOrDefault(x => x.Code == data.Code);
            if (activationCodeEntity == null)
            {
                return new Re()
                {
                    code = -1,
                    msg = "激活码不存在",
                    data = null
                };
            }

            //更改用户等级
            var userEntities = _dbLinkContext.User.Where(x => x.Email == email);
            if (!userEntities.Any())
            {
                return new Re()
                {
                    code = -1,
                    msg = "用户不存在",
                    data = null
                };
            }

            if (userEntities.First().Grade == 1)
            {
                userEntities.First().Grade = 2;
                userEntities.First().ExpireDate = DateTime.Now.AddDays(activationCodeEntity.CodeGrade.UsageTime);
                userEntities.First().RemainingTimes += activationCodeEntity.CodeGrade.UsageFrequency;
            }
            else if (userEntities.First().Grade == 2)
            {
                userEntities.First().RemainingTimes = userEntities.First().ExpireDate < DateTime.Now
                    ? activationCodeEntity.CodeGrade.UsageFrequency
                    : userEntities.First().RemainingTimes + activationCodeEntity.CodeGrade.UsageFrequency;

                userEntities.First().ExpireDate = userEntities.First().ExpireDate < DateTime.Now
                    ? DateTime.Now.AddDays(activationCodeEntity.CodeGrade.UsageTime)
                    : userEntities.First().ExpireDate.AddDays(activationCodeEntity.CodeGrade.UsageTime);
            }

            //更改激活码使用次数
            activationCodeEntity.RemainingTimes--;
            if (activationCodeEntity.RemainingTimes <= 0)
            {
                _dbLinkContext.ActivationCode.Remove(activationCodeEntity);
            }

            //保存更改
            var i = await _dbLinkContext.SaveChangesAsync();

            if (i == 0)
            {
                return new Re()
                {
                    code = -1,
                    msg = "激活失败请重试",
                    data = null
                };
            }

            //获取Token
            var token = _jwtHelper.CreateToken(new List<Claim>()
            {
                new Claim(ClaimTypes.UserData, userEntities.First().UID),
                new Claim(ClaimTypes.Name, userEntities.First().UserName),
                new Claim(ClaimTypes.Email, userEntities.First().Email),
                new Claim(ClaimTypes.Role, userEntities.First().Grade.ToString())
            });
            return new Re()
            {
                code = 0,
                msg = "激活成功",
                data = token
            };
        }
        catch (Exception e)
        {
            return new Re()
            {
                code = -1,
                msg = e.Message,
                data = null
            };
        }
    }

    /// <summary>
    /// 查看用户信息
    /// </summary>
    /// <returns></returns>
    [HttpPost("ViewUserInformation")]
    [Authorize(Roles = "-1,0,1,2")]
    [EnableCors("AllowAll")]
    public async Task<Re> ViewUserInformation()
    {
        //根据Token获取用户UID
        var uid = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
        if (uid == null)
        {
            return new Re()
            {
                code = -1,
                msg = "用户不存在",
                data = null
            };
        }

        //根据UID获取用户信息
        var userEntity = _dbLinkContext.User.FirstOrDefault(x => x.UID == uid);
        if (userEntity == null)
        {
            return new Re()
            {
                code = -1,
                msg = "用户不存在",
                data = null
            };
        }

        userEntity.Password = null;

        return new Re()
        {
            code = 0,
            msg = "获取成功",
            data = userEntity
        };
    }

    /// <summary>
    /// 激活码传入类
    /// </summary>
    public class GenerateActivationCodeClass
    {
        public CodeGradeEntity codeGrade { get; set; }

        /// <summary>
        /// 生成数量
        /// </summary>
        public int num { get; set; }

        /// <summary>
        /// 这个激活码能使用的次数
        /// </summary>
        public int usageCount { get; set; } = 1;
    }

    /// <summary>
    /// 激活码传入类
    /// </summary>
    public class ActivationCode
    {
        public string Code { get; set; }
    }

    /// <summary>
    /// 登录传入类
    /// </summary>
    public class LoginData
    {
        public string Email { get; set; }
        public string PasswordMd5 { get; set; }
    }

    /// <summary>
    /// 注册传入类
    /// </summary>
    public class RegisterData
    {
        public string UserName { get; set; }
        public string PasswordMd5 { get; set; }
        public string Email { get; set; }
    }
}