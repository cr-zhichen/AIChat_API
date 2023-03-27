using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ChatGPT_API.Context;
using ChatGPT_API.Controllers;
using ChatGPT_API.Entity;
using ChatGPT_API.Entity.Db;
using ChatGPT_API.Entity.Re;
using ChatGPT_API.Tool;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChatGPT_API.WS
{
    public class WebSocketController
    {
        JObject configJson =
            JObject.Parse(System.IO.File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));

        private readonly DbLinkContext _dbLinkContext;
        private List<string> _apiKeys = new();
        private string _apiEndpoint = "";

        public WebSocketController(DbLinkContext dbLinkContext)
        {
            _dbLinkContext = dbLinkContext;
            _apiEndpoint = configJson["Config"]["ApiUrl"].ToString();
            _apiKeys = configJson["Config"]["ApiKeys"].ToString().Split(',').ToList();
        }

        public async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[40960];
            WebSocketReceiveResult result =
                await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                // 接收到的消息
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                Root? j = null;
                try
                {
                    j = JsonConvert.DeserializeObject<Root>(message);
                }
                catch (Exception e)
                {
                    var ls = JsonConvert.SerializeObject(
                        new
                        {
                            route = "err",
                            content = "传入字数过多，超出限制。",
                            historyId = ""
                        });
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(
                            Encoding.UTF8.GetBytes(ls),
                            0,
                            Encoding.UTF8.GetBytes(ls).Length),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                    return;
                }

                string token = j.token;
                string messages = j.data.messages;
                string chatHistory = j.data.chatHistory;

                #region 令牌验证

                UserEntity? user = null;
                string uid = "";
                try
                {
                    // 解析 JWT
                    var jwtHandler = new JwtSecurityTokenHandler();
                    var jwtToken = jwtHandler.ReadJwtToken(token);

                    // 检查令牌是否过期
                    bool isTokenExpired = jwtToken.ValidTo < DateTime.UtcNow;
                    // 通过Token获取用户uid
                    uid = jwtToken.Payload[ClaimTypes.UserData].ToString();

                    if (isTokenExpired)
                    {
                        var ls = JsonConvert.SerializeObject(
                            new
                            {
                                route = "tokenErr",
                                content = "登录过期，请重新登录",
                                historyId = ""
                            });
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(
                                Encoding.UTF8.GetBytes(ls),
                                0,
                                Encoding.UTF8.GetBytes(ls).Length),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                        return;
                    }

                    //根据UID获取用户信息
                    user = await _dbLinkContext.User.FirstOrDefaultAsync(x => x.UID == uid);
                    if (user == null)
                    {
                        var ls = JsonConvert.SerializeObject(
                            new
                            {
                                route = "tokenErr",
                                content = "用户不存在，请重新登录",
                                historyId = ""
                            });
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(
                                Encoding.UTF8.GetBytes(ls),
                                0,
                                Encoding.UTF8.GetBytes(ls).Length),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                        return;
                    }

                    // 提取用户角色
                    string userRole = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                    // 检查用户角色是否满足授权需求
                    var allowedRoles = new List<string> { "-1", "0", "1", "2" };
                    if (!allowedRoles.Contains(userRole))
                    {
                        var ls = JsonConvert.SerializeObject(
                            new
                            {
                                route = "tokenErr",
                                content = "用户无权限",
                                historyId = ""
                            });
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(
                                Encoding.UTF8.GetBytes(ls),
                                0,
                                Encoding.UTF8.GetBytes(ls).Length),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                        return;
                    }
                }
                catch (Exception e)
                {
                    var ls = JsonConvert.SerializeObject(
                        new
                        {
                            route = "tokenErr",
                            content = "Token错误，请重新登录",
                            historyId = ""
                        });
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(
                            Encoding.UTF8.GetBytes(ls),
                            0,
                            Encoding.UTF8.GetBytes(ls).Length),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                    return;
                }

                #endregion

                #region 用户次数判断

                //判断用户是否有剩余次数
                if (user.RemainingTimes <= 0 && user.Grade != -1)
                {
                    var ls = JsonConvert.SerializeObject(
                        new
                        {
                            route = "err",
                            content = "剩余次数不足",
                            historyId = ""
                        });
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(
                            Encoding.UTF8.GetBytes(ls),
                            0,
                            Encoding.UTF8.GetBytes(ls).Length),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                    return;
                }
                else
                {
                    //剩余次数减一
                    _dbLinkContext.User.FirstOrDefault(x => x.UID == uid)!.RemainingTimes -= 1;
                    await _dbLinkContext.SaveChangesAsync();
                }

                //判断用户会员是否过期
                if (user?.ExpireDate < DateTime.Now + TimeSpan.FromHours(1) && user.Grade != -1)
                {
                    var ls = JsonConvert.SerializeObject(
                        new
                        {
                            route = "err",
                            content = "会员已过期",
                            historyId = ""
                        });
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(
                            Encoding.UTF8.GetBytes(ls),
                            0,
                            Encoding.UTF8.GetBytes(ls).Length),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                    return;
                }

                #endregion


                List<Message> historyMessages = new();
                HistoryRecordEntity historyRecord = null;

                #region 检查历史记录

                //判断是否传入历史记录
                if (chatHistory != "" || chatHistory != null)
                {
                    if (chatHistory.IsNumber())
                    {
                        //根据历史记录id获取历史记录
                        historyRecord = await _dbLinkContext.HistoryRecord.FirstOrDefaultAsync(x =>
                            x.Id == chatHistory.ToInt() && x.UID == uid);
                        if (historyRecord is null)
                        {
                            var ls = JsonConvert.SerializeObject(
                                new
                                {
                                    route = "err",
                                    content = "历史记录不存在",
                                    historyId = ""
                                });
                            await webSocket.SendAsync(
                                new ArraySegment<byte>(
                                    Encoding.UTF8.GetBytes(ls),
                                    0,
                                    Encoding.UTF8.GetBytes(ls).Length),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);
                            return;
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
                }

                #endregion

                #region 请求聊天

                string reMessages = "";
                string substring = "";
                foreach (var item in _apiKeys)
                {
                    substring = "";
                    var httpClient = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, _apiEndpoint);
                    request.Headers.Add("Authorization", $"Bearer {item}");

                    historyMessages.Add(new Message()
                    {
                        role = "user",
                        content = messages
                    });

                    if (user.Grade == 0)
                    {
                        request.Content = new StringContent(JsonConvert.SerializeObject(
                            new ApiRequestController.Parameters
                            {
                                model = "gpt-3.5-turbo",
                                messages = historyMessages,
                                stream = true,
                                max_tokens = 150
                            }), Encoding.UTF8, "application/json");
                    }
                    else
                    {
                        request.Content = new StringContent(JsonConvert.SerializeObject(
                            new ApiRequestController.Parameters
                            {
                                model = "gpt-3.5-turbo",
                                messages = historyMessages,
                                stream = true
                            }), Encoding.UTF8, "application/json");
                    }


                    using (var response =
                           await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    await using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream))
                    {
                        //如果返回400错误
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            var ls = JsonConvert.SerializeObject(
                                new
                                {
                                    route = "err",
                                    content = "会话字数超限，或api请求失败，请新建回话后重试",
                                    historyId = ""
                                });
                            await webSocket.SendAsync(
                                new ArraySegment<byte>(
                                    Encoding.UTF8.GetBytes(ls),
                                    0,
                                    Encoding.UTF8.GetBytes(ls).Length),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);
                            return;
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            substring += await reader.ReadToEndAsync();
                            continue;
                        }

                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            //判断是否请求成功
                            if (line.Length < 6)
                            {
                                continue;
                            }

                            substring = line.Substring(6);

                            try
                            {
                                //将json字符串不使用实体类进行反序列化
                                JsonDocument document = JsonDocument.Parse(substring);
                                //获取choices 获取的是一个数组
                                JsonElement choices = document.RootElement.GetProperty("choices");
                                //获取数组中的第一个元素
                                JsonElement choice = choices[0];
                                //获取delta
                                JsonElement delta = choice.GetProperty("delta");
                                //获取delta中的content
                                JsonElement content = delta.GetProperty("content");
                                //转换为string
                                substring = content.ToString();
                                if (substring.Length <= 0)
                                {
                                    continue;
                                }
                            }
                            catch (Exception e)
                            {
                                continue;
                            }

                            reMessages += substring;

                            await Task.Delay(50);

                            await webSocket.SendAsync(
                                new ArraySegment<byte>(
                                    Encoding.UTF8.GetBytes(
                                        JsonConvert.SerializeObject(new
                                        {
                                            route = "chat",
                                            content = substring,
                                            historyId = ""
                                        })),
                                    0,
                                    Encoding.UTF8.GetBytes(
                                        JsonConvert.SerializeObject(new
                                        {
                                            route = "chat",
                                            content = substring,
                                            historyId = ""
                                        })).Length),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);
                        }

                        long? historyId = null;

                        //判断是否不是游客
                        if (user.Grade != 0)
                        {
                            //将聊天记录存入数据库
                            HistoryMessagesEntity historyRecordEntity = new()
                            {
                                Role = "user",
                                Content = messages,
                            };
                            //将historyRecordEntity添加到数据库
                            await _dbLinkContext.HistoryMessages.AddAsync(historyRecordEntity);
                            await _dbLinkContext.SaveChangesAsync();
                            long id1 = historyRecordEntity.ID;

                            historyRecordEntity = new HistoryMessagesEntity()
                            {
                                Role = "assistant",
                                Content = reMessages,
                            };
                            //将historyRecordEntity添加到数据库
                            await _dbLinkContext.HistoryMessages.AddAsync(historyRecordEntity);
                            await _dbLinkContext.SaveChangesAsync();
                            long id2 = historyRecordEntity.ID;

                            if (chatHistory is null || chatHistory == "")
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

                        await webSocket.SendAsync(
                            new ArraySegment<byte>(
                                Encoding.UTF8.GetBytes(
                                    JsonConvert.SerializeObject(new
                                    {
                                        route = "chat",
                                        content = substring,
                                        historyId = historyId
                                    })),
                                0,
                                Encoding.UTF8.GetBytes(
                                    JsonConvert.SerializeObject(new
                                    {
                                        route = "chat",
                                        content = substring,
                                        historyId = historyId
                                    })).Length),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);

                        Console.WriteLine(reMessages);
                    }
                }

                #endregion

                // 继续接收消息
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }


            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }

    public class Data
    {
        /// <summary>
        /// 测试
        /// </summary>
        public string messages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string chatHistory { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Data data { get; set; }
    }
}