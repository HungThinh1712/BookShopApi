using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookShopApi.Functions
{
    public static class Authenticate
    {
        public static string GetToken(string userId,bool isAdmin)
        {
            string securityKey = "NHTIMH281019990965451243990182DKDT7979";
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature);
            var claims = new List<Claim>();
 
            claims.Add(new Claim("sub", userId));
            claims.Add(new Claim("admin", isAdmin.ToString()));

            //create token
            var token = new JwtSecurityToken(
                issuer: "Tina",
                audience: "user",
                expires: DateTime.MaxValue,
                signingCredentials: signingCredentials,
                claims: claims
            );
            return (new JwtSecurityTokenHandler().WriteToken(token));
        }
        public static string DecryptToken(string token, string type = "sub")
        {
            //Bỏ phần Bearer 
            token = token.Replace("Bearer ", "");
            //Lấy id của user đăng nhập
            var encode = new JwtSecurityToken(jwtEncodedString: token);
            string userId = encode.Claims.First(c => c.Type == type).Value;
            return userId;
        }

    }
}
