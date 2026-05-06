using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Repository.JwtSecutiry
{
    public class JwtExtensions
    {
            private readonly IConfiguration _configuration;

            public JwtExtensions(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public string GenerateToken()
            {              
                var userClaims = new[]
                {
                  new Claim(ClaimTypes.NameIdentifier, "AdminUser"),
                  new Claim(ClaimTypes.Email, "correo@dominio.com"!)
                };

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]!));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

                //crear detalle del token
                var jwtConfig = new JwtSecurityToken(
                    claims: userClaims,
                    expires: DateTime.UtcNow.AddMinutes(10),
                    signingCredentials: credentials
                    );

                return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
            }        
    }
}
