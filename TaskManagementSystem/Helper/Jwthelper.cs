using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



namespace TaskManagementSystem.Helper
{
    public class Jwthelper
    {
        private readonly IConfiguration _conFiguration;

        public Jwthelper(IConfiguration configuration)
        {
            _conFiguration = configuration;
        }


        public string GenerateJwtToken(string email)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conFiguration["JWT:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "mroy13.com",
                audience: "all.com",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        public string GenerateJwtRefreshToken()
        {
        //    var claims = new[]
        //    {
        //    new Claim(JwtRegisteredClaimNames.Sub, email),
        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //};

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conFiguration["JWT:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "mroy13.com",
                audience: "all.com",
                //claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }







        public string? getJWTTokenClaim(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_conFiguration["JWT:Key"]!));


                //var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                //var claimValue = securityToken.Claims.FirstOrDefault(c => c.Type == "Sub")?.Value;
                //return claimValue;

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _conFiguration["JWT:issuer"],
                    ValidateAudience = true,
                    ValidAudience = _conFiguration["JWT:audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    //ClockSkew = TimeSpan.FromMinutes(10)
                };


                var decryptData = tokenHandler.ValidateToken(token, validationParameters, out _);
                
                Console.WriteLine($"{decryptData}");
                var email = decryptData?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? decryptData?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return email;

            }
            catch (Exception)
            {
                return null;
            }
        }


    }
}
