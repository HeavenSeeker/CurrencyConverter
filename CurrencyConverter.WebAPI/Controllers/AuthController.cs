using CurrencyConverter.WebAPI.Data;
using CurrencyConverter.WebAPI.DTO;
using CurrencyConverter.WebAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CurrencyConverter.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<IdentityUser> userManager,
                              ILogger<AuthController> logger,
                              ITokenService tokenService)
        {
            _userManager = userManager;
            _logger = logger;
            this._tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    return BadRequest("User with this username is not registered with us.");
                }
                bool isValidPassword = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!isValidPassword)
                {
                    return Unauthorized();
                }

                // creating the necessary claims
                List<Claim> authClaims = [
                new (ClaimTypes.Name, user.UserName),
                // unique id for token
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //user id
                new ("cid", user.Id)];

                var userRoles = await _userManager.GetRolesAsync(user);

                // adding roles to the claims. So that we can get the user role from the token.
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                // generating access token
                var token = _tokenService.GenerateAccessToken(authClaims);

                return Ok(new
                {
                    AccessToken = token,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Unauthorized();
            }

        }
    }
}
