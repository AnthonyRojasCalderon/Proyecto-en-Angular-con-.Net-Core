﻿using FBTarjeta;
using FBTarjeta.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace reactiveFormWeb.Controllers
{
    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<AplicationUser> _userManager;
        private readonly SignInManager<AplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<AplicationUser> userManager,
            SignInManager<AplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this._configuration = configuration;
        }

        [Route("Create")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserInfo model)
        {
            if (ModelState.IsValid)
            {
                var user = new AplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Username or password invalid");
                }
            }
            else
            {
                return BadRequest(ModelState);
            }

        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserInfo userInfo)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(userInfo.Email, userInfo.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return BuildToken(userInfo);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        private IActionResult BuildToken(UserInfo userInfo)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Llave_super_secreta"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(7);

            JwtSecurityToken token = new JwtSecurityToken(
               issuer: "yourdomain.com",
               audience: "yourdomain.com",
               claims: claims,
               expires: expiration,
               signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = expiration
            });

        }
    }
}
