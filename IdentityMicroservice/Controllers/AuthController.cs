using IdentityMicroservice.BusinessLogic.Contracts;
using IdentityMicroservice.Domain.Entities;
using IdentityMicroservice.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityMicroservice.Controllers
{
    [Authorize]
    [Route("v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthBusinessLogic authBusinessLogic;
        private readonly ITokenService tokenService;

        public AuthController(IAuthBusinessLogic authBusinessLogic, ITokenService tokenService)
        {
            this.authBusinessLogic = authBusinessLogic;
            this.tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser)
        {
            return new ObjectResult(await authBusinessLogic.ProcessRegisterUser(registerUser));
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginCredentials loginCredentials)
        {
            return new ObjectResult(await authBusinessLogic.ProcessAuthenticate(loginCredentials));
        }

        [AllowAnonymous]
        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel tokenModel)
        {
            return new ObjectResult(await authBusinessLogic.ProcessRefreshToken(tokenModel));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] TokenModel tokenModel)
        {
            await authBusinessLogic.ProcessLogout(tokenModel.RefreshToken, GetUserIdFromRequest());

            return Ok();
        }

        [HttpPost("logout-everywhere")]
        public async Task<IActionResult> LogoutEverywhere()
        {
            await authBusinessLogic.ProcessLogoutEverywhere(GetUserIdFromRequest());

            return Ok();
        }

        private Guid GetUserIdFromRequest()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            var principal = tokenService.GetPrincipalFromExpiredToken(token);
            var userId = new Guid(principal.FindFirstValue(ClaimTypes.NameIdentifier));

            return userId;
        }
    }
}
