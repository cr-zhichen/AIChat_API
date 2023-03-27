using System.Net;
using System.Net.WebSockets;
using System.Text;
using ChatGPT_API;
using ChatGPT_API.Context;
using ChatGPT_API.Server;
using ChatGPT_API.Static;
using ChatGPT_API.WS;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);


JObject configJson = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));


//更改监听端口
builder.WebHost.UseUrls("http://*:7299");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 允许所有域名访问
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers();

// 在服务容器中添加 TestContext 类别作为数据库内容服务
builder.Services.AddDbContext<DbLinkContext>(opt =>
{
    // 从 appsettings.json 文件中取得 MySQL 的连接字符串
    string? connectionString = configJson["ConnectionStrings"]["MySQL"].ToString();
    // 自动侦测 MySQL 的服务器版本
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    // 使用 MySQL 数据库提供者和连接字符串来配置数据库内容选项
    opt.UseMySql(connectionString, serverVersion);
});

//JWT鉴权
builder.Services.AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true, //是否验证Issuer
            ValidIssuer = configJson["Jwt"]["Issuer"].ToString(), //发行人Issuer
            ValidateAudience = true, //是否验证Audience
            ValidAudience = configJson["Jwt"]["Audience"].ToString(), //订阅人Audience
            ValidateIssuerSigningKey = true, //是否验证SecurityKey
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configJson["Jwt"]["SecretKey"].ToString())), //SecurityKey
            ValidateLifetime = true, //是否验证失效时间
            ClockSkew = TimeSpan.FromMinutes(5), //Token验证偏差时间 
            RequireExpirationTime = true, //是否需要过期时间
        };
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    // 读取XML文档
    var xmlFile = $"ChatGPT_API.WebApi.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    // 添加JWT验证配置
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "请输入Token Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddTransient<IHostedService, DatabaseCheckService>();

builder.Services.AddSingleton(new JwtHelper());

builder.Services.AddTransient<WebSocketController>();

var app = builder.Build();
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

//开启定时任务
ScheduledTask.Start();
//每多少秒执行一次
ScheduledTask._intervalSecond = 60;
//添加定时删除过期验证码
ScheduledTask._action.Add("DeleteCode",
    () => { VerificationCode.VerificationCodeList.RemoveAll(x => x.ExpireTime < DateTime.Now); });

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

// 启用 CORS
app.UseCors("AllowAll");

//JWT中间件
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });


//ws中间件
app.UseWebSockets();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var webSocketController = context.RequestServices.GetService<WebSocketController>();
            await webSocketController.HandleWebSocketAsync(context, webSocket);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //增加jwt验证
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Web API");
        c.RoutePrefix = string.Empty;
        c.OAuthClientId("swagger");
        c.OAuthAppName("Swagger UI");
        c.OAuthUsePkce();
    });
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();