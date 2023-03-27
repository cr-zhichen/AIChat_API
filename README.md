# AIChat_API

## 简介

一个代理请求ChatGPT的后端项目，可以自行生成激活码

## 使用技术

使用技术栈：.Net 6 + EF Core + MySQL

## 使用方法

1. 创建MySQL数据库
2. 导入数据库文件 script.sql
3. 修改appsettings.json中的数据库连接字符串
4. 修改appsettings.json中的JWT密钥
5. 修改appsettings.json中的API请求地址与Key,如需Key轮询，请用逗号分隔
6. 修改appsettings.json中的SMTP邮箱发件配置
7. 运行项目
    - 运行时依赖
        1. 安装 .Net 6 运行时 https://dotnet.microsoft.com/zh-cn/download/dotnet/6.0
        2. 运行项目 dotnet ChatGPT_API.dll
    - 独立应用运行
        1. 给与程序777权限后运行即可

## 注意事项

项目监听端口号为7299
管理员需注册后手动在数据库中将用户角色改为-1

## API

## /ApiRequest/聊天接口
#### 接口URL
> http://127.0.0.1:7299/ApiRequest/Chat

#### 请求方式
> POST

#### Content-Type
> json

#### 请求Body参数
```javascript
{
	"messages": "这是一条测试消息",
	"chatHistory": null
}
```
| 参数名      | 示例值                   | 参数类型 | 是否必填 | 参数描述 |
| ----------- | ------------------------ | -------- | -------- | -------- |
| messages    | 聊天消息                 | String   | 是       | -        |
| chatHistory | 聊天记录ID（若无可传空） | String   | 是       | -        |
#### 认证方式
```text
bearer
```
#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "API请求成功",
	"data": {
		"content": "收到测试消息，测试成功！",
		"historyId": 22
	}
}
```
## /ApiRequest/获取聊天记录
#### 接口URL
> http://127.0.0.1:7299/ApiRequest/GetChatRecord

#### 请求方式
> POST

#### Content-Type
> json

#### 请求Body参数
```javascript
{
    "id": "1"
}
```
| 参数名 | 示例值     | 参数类型 | 是否必填 | 参数描述 |
| ------ | ---------- | -------- | -------- | -------- |
| id     | 聊天记录ID | String   | 是       | -        |
#### 认证方式
```text
bearer
```
#### 预执行脚本
```javascript
暂无预执行脚本
```
#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "获取成功",
	"data": [
		{
			"role": "user",
			"content": "这是一条测试消息"
		},
		{
			"role": "assistant",
			"content": "收到测试消息，测试成功！"
		}
	]
}
```
## /ApiRequest/删除聊天记录
#### 接口URL
> http://127.0.0.1:7299/ApiRequest/DeleteChatRecord

#### 请求方式
> POST

#### Content-Type
> json

#### 请求Body参数
```javascript
{
	"id": "22"
}
```
| 参数名 | 示例值               | 参数类型 | 是否必填 | 参数描述 |
| ------ | -------------------- | -------- | -------- | -------- |
| id     | 需要删除的聊天记录id | String   | 是       | -        |
#### 认证方式
```text
bearer
```
#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "删除成功",
	"data": null
}
```
#### 错误响应示例
```javascript
{
	"code": -1,
	"msg": "聊天记录不存在",
	"data": null
}
```
## /ApiRequest/查看全部历史记录

#### 接口URL
> http://127.0.0.1:7299/ApiRequest/FindAllHistoryRecord

#### 请求方式
> POST

#### Content-Type
> none

#### 认证方式
```text
bearer
```
#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "获取历史记录成功",
	"data": [
		{
			"id": 1,
			"preview": "这是一条测试消息"
		},
		{
			"id": 5,
			"preview": "这是一条测试"
		},
		{
			"id": 16,
			"preview": "这是一个测试消息"
		},
		{
			"id": 17,
			"preview": "测试"
		},
		{
			"id": 18,
			"preview": "测试"
		},
		{
			"id": 34,
			"preview": "这是一条测试消息"
		},
		{
			"id": 35,
			"preview": "这是一条测试消息"
		},
		{
			"id": 36,
			"preview": "帮我使用C#写一个冒"
		},
		{
			"id": 37,
			"preview": "这是一条测试"
		},
		{
			"id": 38,
			"preview": "测试消息"
		},
		{
			"id": 39,
			"preview": "这是一条测试消息"
		},
		{
			"id": 40,
			"preview": "这是一条测试消息"
		},
		{
			"id": 41,
			"preview": "这是一条测试消息"
		},
		{
			"id": 42,
			"preview": "写一篇300字的有关"
		},
		{
			"id": 43,
			"preview": "你好"
		},
		{
			"id": 44,
			"preview": "写一篇200字左右的"
		},
		{
			"id": 45,
			"preview": "写一篇200字左右的"
		},
		{
			"id": 46,
			"preview": "测试消息"
		},
		{
			"id": 47,
			"preview": "帮我写一份200字左"
		},
		{
			"id": 48,
			"preview": "帮我写一份200字左"
		},
		{
			"id": 49,
			"preview": "测试"
		},
		{
			"id": 50,
			"preview": "这是一条测试消息"
		},
		{
			"id": 51,
			"preview": "这是一个测试消息"
		},
		{
			"id": 52,
			"preview": "这是一个测试消息"
		},
		{
			"id": 53,
			"preview": "这是一条测试消息"
		},
		{
			"id": 54,
			"preview": "帮我写一篇300字左"
		},
		{
			"id": 55,
			"preview": "这是一条测试消息"
		}
	]
}
```
## /ApiRequest/删除全部记录
#### 接口URL
> http://127.0.0.1:7299/ApiRequest/DeleteAllChatRecord

#### 请求方式
> POST

#### Content-Type
> none

#### 认证方式
```text
bearer
```

## /User/游客登录

#### 接口URL
> http://127.0.0.1:7299/User/Tourists

#### 请求方式
> POST

#### Content-Type
> none

#### 认证方式
```text
noauth
```

#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "登录成功",
	"data": {
		"userName": "游客",
		"role": 0,
		"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoi5ri45a6iIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvdXNlcmRhdGEiOiI2ZjQzY2FiNi1mOWMzLTRmYWYtYTMyMy02N2I1NzA3NWNiNGEiLCJuYmYiOjE2Nzg2OTAzNTAsImV4cCI6MTY3ODY5Mzk1MCwiaXNzIjoiV2ViQXBwSXNzdWVyIiwiYXVkIjoiV2ViQXBwQXVkaWVuY2UifQ.HsjNtE0LEjRVAAfd7XcmIYLtXujF-7X5-xoD2kq6UfY"
	}
}
```

## /User/查看用户信息

#### 接口URL
> http://127.0.0.1:7299/User/ViewUserInformation

#### 请求方式
> POST

#### Content-Type
> none

#### 认证方式
```text
bearer
```

#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "获取成功",
	"data": {
		"uid": "e1f3c8d2-4d51-40bf-b875-a580727f7e17",
		"userName": "ccrui",
		"password": "qwertyuiop",
		"email": "zgccrui@outlook.com",
		"grade": 2,
		"expireDate": "2023-04-13T14:53:18.64182",
		"remainingTimes": 540
	}
}
```

## /User/发送邮箱验证码

#### 接口URL
> http://127.0.0.1:7299/User/VenerateVerificationCode?email=zgccrui@outlook.com

#### 请求方式
> POST

#### Content-Type
> none

#### 请求Query参数
| 参数名 | 示例值              | 参数类型 | 是否必填 | 参数描述       |
| ------ | ------------------- | -------- | -------- | -------------- |
| email  | zgccrui@outlook.com | Text     | 是       | 需要发送的邮箱 |
#### 认证方式
```text
noauth
```

#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "验证码已发送",
	"data": null
}
```
#### 错误响应示例
```javascript
{
	"code": -1,
	"msg": "邮箱格式错误",
	"data": null
}
```
## /User/用户登录

#### 接口URL
> http://127.0.0.1:7299/User/Login

#### 请求方式
> POST

#### Content-Type
> json

#### 请求Body参数
```javascript
{
	"email": "zgccrui@outlook.com",
	"passwordMd5": "qwertyuiop"
}
```
| 参数名      | 示例值   | 参数类型 | 是否必填 | 参数描述       |
| ----------- | -------- | -------- | -------- | -------------- |
| email       | 用户邮箱 | String   | 是       | 需要发送的邮箱 |
| passwordMd5 | 用户密码 | String   | 是       | -              |
#### 认证方式
```text
noauth
```

#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "登录成功",
	"data": {
		"userName": "ccrui",
		"role": 1,
		"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiY2NydWkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ6Z2NjcnVpQG91dGxvb2suY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy91c2VyZGF0YSI6ImUxZjNjOGQyLTRkNTEtNDBiZi1iODc1LWE1ODA3MjdmN2UxNyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IjEiLCJuYmYiOjE2Nzg2OTAzMTUsImV4cCI6MTY3ODY5MzkxNSwiaXNzIjoiV2ViQXBwSXNzdWVyIiwiYXVkIjoiV2ViQXBwQXVkaWVuY2UifQ.WB1u8IPDkSvZhn_Q9gNnAZtUqhKU1fjDWHyd6bt1UiM"
	}
}
```

## /User/生成激活码

#### 接口URL
> http://127.0.0.1:7299/User/GenerateActivationCode

#### 请求方式
> POST

#### Content-Type
> json

#### 请求Body参数
```javascript
{
	"codeGrade": {
		"usageTime": "10",
		"usageFrequency": "100"
	},
	"num": "1",
	"usageCount": "10"
}
```
| 参数名                   | 示例值                   | 参数类型 | 是否必填 | 参数描述               |
| ------------------------ | ------------------------ | -------- | -------- | ---------------------- |
| codeGrade                | -                        | Object   | 是       | -                      |
| codeGrade.usageTime      | 激活码可激活的时长       | String   | 是       | -                      |
| codeGrade.usageFrequency | 激活码可激活的数量       | String   | 是       | -                      |
| num                      | 生成的数量               | String   | 是       | 生成数量               |
| usageCount               | 生成的激活码可使用的次数 | String   | 是       | 这个激活码能使用的次数 |
#### 认证方式
```text
bearer
```

#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "生成成功",
	"data": [
		{
			"id": 11,
			"code": "c0590c51b4a3433da545dbc4be8f5e8a",
			"codeGrade": {
				"id": 3,
				"usageTime": 10,
				"usageFrequency": 100
			},
			"remainingTimes": 1
		},
		{
			"id": 12,
			"code": "4bef6b01c3934691b4036d6f4e4b796e",
			"codeGrade": {
				"id": 3,
				"usageTime": 10,
				"usageFrequency": 100
			},
			"remainingTimes": 1
		},
		{
			"id": 13,
			"code": "a003c7ffa89d4634b84f2212f04b06c8",
			"codeGrade": {
				"id": 3,
				"usageTime": 10,
				"usageFrequency": 100
			},
			"remainingTimes": 1
		},
		{
			"id": 14,
			"code": "c1b81f44a3bd449e8b948718b3911179",
			"codeGrade": {
				"id": 3,
				"usageTime": 10,
				"usageFrequency": 100
			},
			"remainingTimes": 1
		},
		{
			"id": 15,
			"code": "181e841e0c0e4d5bb049c8c80f64cc9d",
			"codeGrade": {
				"id": 3,
				"usageTime": 10,
				"usageFrequency": 100
			},
			"remainingTimes": 1
		}
	]
}
```

## /User/重置密码

#### 接口URL
> http://127.0.0.1:7299/User/ResetPassword?code=973875

#### 请求方式
> POST

#### Content-Type
> json

#### 请求Query参数
| 参数名 | 示例值 | 参数类型 | 是否必填 | 参数描述 |
| ------ | ------ | -------- | -------- | -------- |
| code   | 973875 | Text     | 是       | 验证码   |
#### 请求Body参数
```javascript
{
	"userName": "zgccrui",
	"passwordMd5": "qwertyuiop",
	"email": "zgccrui@outlook.com"
}
```
| 参数名      | 示例值     | 参数类型 | 是否必填 | 参数描述       |
| ----------- | ---------- | -------- | -------- | -------------- |
| userName    | 新用户名   | String   | 是       | -              |
| passwordMd5 | 新用户密码 | String   | 是       | -              |
| email       | 用户邮箱   | String   | 是       | 需要发送的邮箱 |
#### 认证方式
```text
noauth
```

#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "重置密码成功",
	"data": null
}
```

## /User/使用激活码

#### 接口URL
> http://127.0.0.1:7299/User/UseActivationCode

#### 请求方式
> POST

#### Content-Type
> json

#### 请求Body参数
```javascript
{
	"code": "1d8c5bff9ae04984bf5c0defff6c52cc"
}
```
| 参数名 | 示例值 | 参数类型 | 是否必填 | 参数描述 |
| ------ | ------ | -------- | -------- | -------- |
| code   | 激活码 | String   | 是       | -        |
#### 认证方式
```text
bearer
```

#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "激活成功",
	"data": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiY2NydWkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ6Z2NjcnVpQG91dGxvb2suY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiMiIsIm5iZiI6MTY3ODY5MDM5OCwiZXhwIjoxNjc4NjkzOTk4LCJpc3MiOiJXZWJBcHBJc3N1ZXIiLCJhdWQiOiJXZWJBcHBBdWRpZW5jZSJ9.5R-NqlMyC1jm_GZGIe2CcpC08vxAs2IjztWBoPmXv5A"
}
```
#### 错误响应示例
```javascript
{
	"code": -1,
	"msg": "激活码不存在",
	"data": null
}
```
## /User/用户注册

#### 接口URL
> http://127.0.0.1:7299/User/Register?code=861903

#### 请求方式
> POST

#### Content-Type
> json

#### 请求Query参数
| 参数名 | 示例值 | 参数类型 | 是否必填 | 参数描述 |
| ------ | ------ | -------- | -------- | -------- |
| code   | 861903 | Text     | 是       | 验证码   |
#### 请求Body参数
```javascript
{
	"userName": "ccrui",
	"passwordMd5": "qwertyuiop",
	"email": "zgccrui@outlook.com"
}
```
| 参数名      | 示例值   | 参数类型 | 是否必填 | 参数描述       |
| ----------- | -------- | -------- | -------- | -------------- |
| userName    | 用户名   | String   | 是       | -              |
| passwordMd5 | 用户密码 | String   | 是       | -              |
| email       | 用户邮箱 | String   | 是       | 需要发送的邮箱 |
#### 认证方式
```text
noauth
```

#### 成功响应示例
```javascript
{
	"code": 0,
	"msg": "注册成功",
	"data": {
		"userName": "ccrui",
		"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiY2NydWkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJ6Z2NjcnVpQG91dGxvb2suY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy91c2VyZGF0YSI6ImUxZjNjOGQyLTRkNTEtNDBiZi1iODc1LWE1ODA3MjdmN2UxNyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IjEiLCJuYmYiOjE2Nzg2OTAyOTQsImV4cCI6MTY3ODY5Mzg5NCwiaXNzIjoiV2ViQXBwSXNzdWVyIiwiYXVkIjoiV2ViQXBwQXVkaWVuY2UifQ.JVl3foJWXRMPLkDTNqIJCTKcEuXrZrNRNpmJM8zqGps"
	}
}
```

## /WS聊天接口
```text
使用WebSocket连接
```
#### URL
> wss://localhost:7299/ws

#### Method
> Raw


#### 传入值

``` json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiemdjY3J1aSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InpnY2NydWlAb3V0bG9vay5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiMjdkNWQyNWEtOTdmMC00MjI4LWEyODctZmE1YWQ0MGUyNzgwIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiLTEiLCJuYmYiOjE2Nzk3NDMzNjIsImV4cCI6MTY3OTgyOTc2MiwiaXNzIjoiV2ViQXBwSXNzdWVyIiwiYXVkIjoiV2ViQXBwQXVkaWVuY2UifQ.49SG_M-r69asN6wgeilXbGHVryPn5m29wZsCK-08tqk",
  "data": {
    "messages": "测试",
    "chatHistory": null
  }
}
```

#### 返回值

``` json

{
  "route": "chat",
  "content": "测试",
  "historyId": ""
}

```

##### 返回值解释

route: 路由 [chat,tokenErr,err]
content: 聊天内容 [聊天结束使用[DONE]标识]
historyId: 聊天记录ID [未结束聊天时为空]
