using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace ChatGPT_API;

public class JwtHelper
{
    JObject configJson = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));

    public string CreateToken(List<Claim> claims)
    {
        // 2. 从 appsettings.json 中读取SecretKey
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configJson["Jwt"]["SecretKey"].ToString()));

        // 3. 选择加密算法
        var algorithm = SecurityAlgorithms.HmacSha256;

        // 4. 生成Credentials
        var signingCredentials = new SigningCredentials(secretKey, algorithm);

        // 5. 根据以上，生成token
        var jwtSecurityToken = new JwtSecurityToken(
            configJson["Jwt"]["Issuer"].ToString(), //Issuer
            configJson["Jwt"]["Audience"].ToString(), //Audience
            claims, //Claims,
            DateTime.Now, //notBefore
            DateTime.Now.AddDays(1), //expires
            signingCredentials //Credentials
        );

        // 6. 将token变为string
        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return token;
    }
}