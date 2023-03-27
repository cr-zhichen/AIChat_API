using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ChatGPT_API.Context;
using ChatGPT_API.Entity;
using ChatGPT_API.Entity.Db;
using ChatGPT_API.Entity.Re;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatGPT_API.Controllers;

/// <summary>
/// 用来请求API的接口
/// </summary>
/// 
[ApiController]
[Route("[controller]")]
public class ApiRequestController : ControllerBase
{
    JObject configJson =
        JObject.Parse(System.IO.File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));

    //读取配置文件
    private readonly DbLinkContext _dbLinkContext;
    private readonly JwtHelper _jwtHelper;

    private string _apiEndpoint = "";
    private List<string> _apiKeys = new();

    public ApiRequestController(DbLinkContext dbLinkContext, JwtHelper jwtHelper)
    {
        _dbLinkContext = dbLinkContext;
        _jwtHelper = jwtHelper;
        _apiEndpoint = configJson["Config"]["ApiUrl"].ToString();
        _apiKeys = configJson["Config"]["ApiKeys"].ToString().Split(',').ToList();
    }

    /// <summary>
    /// 聊天接口
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("Chat")]
    [Authorize(Roles = "-1,0,1,2")]
    [EnableCors("AllowAll")]
    public async Task<Re> Chat(IncomingContent data)
    {
        //从token 中获取用户uid
        var uid = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
        //根据UID获取用户信息
        var user = await _dbLinkContext.User.FirstOrDefaultAsync(x => x.UID == uid);
        //判断用户是否存在
        if (user is null)
        {
            return new Re()
            {
                code = -1,
                msg = "用户不存在",
                data = null
            };
        }

        //判断用户是否有剩余次数
        if (user.RemainingTimes <= 0 && user.Grade != -1)
        {
            return (new Re()
            {
                code = -1,
                msg = "剩余次数不足",
                data = null
            });
        }
        else
        {
            //剩余次数减一
            _dbLinkContext.User.FirstOrDefault(x => x.UID == uid)!.RemainingTimes -= 1;
            await _dbLinkContext.SaveChangesAsync();
        }

        //判断用户会员是否过期
        if (user?.ExpireDate < DateTime.Now)
        {
            return (new Re()
            {
                code = -1,
                msg = "会员已过期",
                data = null
            });
        }

        List<Message> historyMessages = new();
        HistoryRecordEntity historyRecord = null;
        //判断是否传入历史记录
        if (data.ChatHistory is not null)
        {
            //根据历史记录id获取历史记录
            historyRecord = await _dbLinkContext.HistoryRecord.FirstOrDefaultAsync(x =>
                x.Id == data.ChatHistory && x.UID == uid);
            if (historyRecord is null)
            {
                return (new Re()
                {
                    code = -1,
                    msg = "历史记录不存在",
                    data = null
                });
            }

            //根据历史记录获取历史消息id
            var ids = historyRecord.Messages.Split(",");
            //根据id获取聊天记录
            foreach (var item in ids)
            {
                var historyMessages1 =
                    await _dbLinkContext.HistoryMessages.FirstOrDefaultAsync(x => x.ID == long.Parse(item));
                if (historyMessages is not null)
                {
                    historyMessages.Add(new Message()
                    {
                        role = historyMessages1.Role,
                        content = historyMessages1.Content
                    });
                }
            }
        }

        HttpResponseMessage response = null;

        string errMsg = "API请求失败";

        foreach (var item in _apiKeys)
        {
            //Post请求
            var request = new HttpRequestMessage(HttpMethod.Post, _apiEndpoint);
            request.Headers.Add("Authorization", $"Bearer {item}");

            historyMessages.Add(new Message()
            {
                role = "user",
                content = data.Messages
            });

            //判断是否是游客
            if (user.Grade == 0)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(new Parameters
                {
                    model = "gpt-3.5-turbo",
                    messages = historyMessages,
                    max_tokens = 150,
                }), Encoding.UTF8, "application/json");
            }
            else
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(new Parameters
                {
                    model = "gpt-3.5-turbo",
                    messages = historyMessages,
                }), Encoding.UTF8, "application/json");
            }


            //发送请求
            response = new HttpClient().SendAsync(request).Result;

            if (response.IsSuccessStatusCode)
            {
                //获取返回内容
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var responseJson = JsonConvert.DeserializeObject<OpenAIReEntity>(responseContent);

                HistoryMessagesEntity historyRecordEntity = new()
                {
                    Role = "user",
                    Content = data.Messages,
                };
                long? historyId = null;
                //判断是否不是游客
                if (user.Grade != 0)
                {
                    //将聊天记录存入数据库
                    //将historyRecordEntity添加到数据库
                    await _dbLinkContext.HistoryMessages.AddAsync(historyRecordEntity);
                    await _dbLinkContext.SaveChangesAsync();
                    long id1 = historyRecordEntity.ID;

                    historyRecordEntity = new HistoryMessagesEntity()
                    {
                        Role = responseJson.choices[0].message.role,
                        Content = responseJson.choices[0].message.content,
                    };
                    //将historyRecordEntity添加到数据库
                    await _dbLinkContext.HistoryMessages.AddAsync(historyRecordEntity);
                    await _dbLinkContext.SaveChangesAsync();
                    long id2 = historyRecordEntity.ID;

                    if (data.ChatHistory is null)
                    {
                        var historyRecordNew = new HistoryRecordEntity()
                        {
                            Messages = $"{id1},{id2}",
                            UID = uid
                        };
                        await _dbLinkContext.HistoryRecord.AddAsync(historyRecordNew);
                        await _dbLinkContext.SaveChangesAsync();
                        historyId = historyRecordNew.Id;
                    }
                    else
                    {
                        historyRecord.Messages += $",{id1},{id2}";
                        _dbLinkContext.HistoryRecord.Update(historyRecord);
                        await _dbLinkContext.SaveChangesAsync();
                        historyId = historyRecord.Id;
                    }
                }

                return new Re()
                {
                    code = 0,
                    msg = "API请求成功",
                    data = new
                    {
                        content = responseJson.choices[0].message.content,
                        historyId = historyId
                    }
                };
            }
        }

        return new Re()
        {
            code = -1,
            msg = "API请求失败",
            data = response.Content.ReadAsStringAsync().Result
        };
    }
    

    /// <summary>
    /// 查看全部历史记录
    /// </summary>
    /// <returns></returns>
    [HttpPost("FindAllHistoryRecord")]
    [Authorize(Roles = "-1,0,1,2")]
    [EnableCors("AllowAll")]
    public async Task<Re> FindAllHistoryRecord()
    {
        //从token 中获取用户uid
        var uid = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
        //根据UID获取用户信息
        var user = await _dbLinkContext.User.FirstOrDefaultAsync(x => x.UID == uid);
        //判断用户是否存在
        if (user is null)
        {
            return new Re()
            {
                code = -1,
                msg = "用户不存在",
                data = null
            };
        }

        var re = new List<object>();

        //根据UID获取历史记录
        var historyRecord = await _dbLinkContext.HistoryRecord.Where(x => x.UID == uid).ToListAsync();
        foreach (var item in historyRecord)
        {
            var ids = item.Messages.Split(",");
            var historyMessages1 =
                await _dbLinkContext.HistoryMessages.FirstOrDefaultAsync(x => x.ID == long.Parse(ids[0]));

            //获取historyMessages1.Content的前10个字符
            var preview = historyMessages1.Content.Length > 25
                ? historyMessages1.Content.Substring(0, 10)
                : historyMessages1.Content;

            re.Add(new
            {
                id = item.Id,
                preview = preview
            });

            // var ids = item.Messages.Split(",");
            // foreach (var item2 in ids)
            // {
            //     var historyMessages1 =
            //         await _dbLinkContext.HistoryMessages.FirstOrDefaultAsync(x => x.ID == long.Parse(item2));
            //
            //     re.Add(new
            //     {
            //         id = item.Id,
            //         preview = historyMessages1
            //     });
            // }
        }

        return new Re()
        {
            code = 0,
            msg = "获取历史记录成功",
            data = re
        };
    }

    /// <summary>
    /// 删除聊天记录
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("DeleteChatRecord")]
    [Authorize(Roles = "-1,0,1,2")]
    [EnableCors("AllowAll")]
    public async Task<Re> DeleteChatRecord(idClass data)
    {
        try
        {
            //删除聊天记录
            var historyRecord = await _dbLinkContext.HistoryRecord.FirstOrDefaultAsync(x => x.Id == data.id);
            if (historyRecord is null)
            {
                return new Re()
                {
                    code = -1,
                    msg = "聊天记录不存在",
                    data = null
                };
            }

            //根据逗号分隔historyRecord.Messages
            var ids = historyRecord.Messages.Split(',');
            //根据id删除历史消息
            foreach (var item in ids)
            {
                var historyMessages =
                    await _dbLinkContext.HistoryMessages.FirstOrDefaultAsync(x => x.ID == long.Parse(item));
                if (historyMessages is not null)
                {
                    _dbLinkContext.HistoryMessages.Remove(historyMessages);
                }
            }

            //删除聊天记录
            _dbLinkContext.HistoryRecord.Remove(historyRecord);
            await _dbLinkContext.SaveChangesAsync();

            return new Re()
            {
                code = 0,
                msg = "删除成功",
                data = null
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
    /// 删除所有历史记录
    /// </summary>
    /// <returns></returns>
    [HttpPost("DeleteAllChatRecord")]
    [Authorize(Roles = "-1,0,1,2")]
    [EnableCors("AllowAll")]
    public async Task<Re> DeleteAllChatRecord()
    {
        try
        {
//根据UID获取用户信息
            var uid = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;
            var user = await _dbLinkContext.User.FirstOrDefaultAsync(x => x.UID == uid);
            //判断用户是否存在
            if (user is null)
            {
                return new Re()
                {
                    code = -1,
                    msg = "用户不存在",
                    data = null
                };
            }

            //根据UID获取历史记录
            var historyRecord = await _dbLinkContext.HistoryRecord.Where(x => x.UID == uid).ToListAsync();
            foreach (var item in historyRecord)
            {
                //根据逗号分隔historyRecord.Messages
                var ids = item.Messages.Split(',');
                //根据id删除历史消息
                foreach (var item2 in ids)
                {
                    var historyMessages =
                        await _dbLinkContext.HistoryMessages.FirstOrDefaultAsync(x => x.ID == long.Parse(item2));
                    if (historyMessages is not null)
                    {
                        _dbLinkContext.HistoryMessages.Remove(historyMessages);
                        await _dbLinkContext.SaveChangesAsync();
                    }
                }

                //删除聊天记录
                _dbLinkContext.HistoryRecord.Remove(item);
                await _dbLinkContext.SaveChangesAsync();
            }

            return new Re()
            {
                code = 0,
                msg = "删除成功",
                data = null
            };
        }
        catch (Exception e)
        {
            return new Re()
            {
                code = -1,
                msg = "删除成功",
                data = e.Message
            };
        }
    }

    /// <summary>
    /// 获取聊天记录
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPost("GetChatRecord")]
    [Authorize(Roles = "-1,0,1,2")]
    [EnableCors("AllowAll")]
    public async Task<Re> GetChatRecord(idClass data)
    {
        //从token 中获取Role
        var uid = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;

        //判断传入的聊天记录是否存在 并且是否是当前用户的
        var historyRecord =
            await _dbLinkContext.HistoryRecord.FirstOrDefaultAsync(x =>
                x.Id == data.id && x.UID == uid);

        if (historyRecord is null)
        {
            return new Re()
            {
                code = -1,
                msg = "历史记录不存在",
                data = null
            };
        }

        //根据历史记录获取聊天记录
        //根据逗号分隔historyRecord.Messages
        var ids = historyRecord?.Messages.Split(',') ?? Array.Empty<string>();
        var messages = new List<Message>();
        //根据id获取聊天记录
        foreach (var item2 in ids)
        {
            var historyMessages =
                await _dbLinkContext.HistoryMessages.FirstOrDefaultAsync(x => x.ID == long.Parse(item2));
            if (historyMessages is not null)
            {
                messages.Add(new Message()
                {
                    role = historyMessages.Role,
                    content = historyMessages.Content
                });
            }
        }

        return new Re()
        {
            code = 0,
            msg = "获取成功",
            data = messages
        };
    }

    /// <summary>
    /// 传入id类
    /// </summary>
    public class idClass
    {
        public long id { get; set; }
    }

    /// <summary>
    /// 用户传入内容
    /// </summary>
    public class IncomingContent
    {
        public string Messages { get; set; }
        public long? ChatHistory { get; set; } = null;
    }

    /// <summary>
    /// 传入OpenAI的参数
    /// </summary>
    public class Parameters
    {
        /// <summary>
        /// 模型名称
        /// </summary>
        public string model { get; set; }

        public double temperature { get; set; } = 0.9;
        public int max_tokens { get; set; } = 3072;
        public double top_p { get; set; } = 1;
        public double frequency_penalty { get; set; } = 0;
        public double presence_penalty { get; set; } = 0;
        public bool stream { get; set; } = false;

        /// <summary>
        /// 消息列表
        /// </summary>
        public List<Message> messages { get; set; }
    }
}