using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityService.DTOs;
using IdentityService.Entities;
using IdentityService.Persistence;
using IdentityService.Services.Account;
using IdentityService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Web.Controller;
using Shared.Web.Helper.Interface;

namespace IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseApiController
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenServices _tokenServices;
        private readonly IUserAccessor _userAccessor;
        private readonly IHttpContextAccessorHelper _httpContextAccessorHelper;
        private readonly IIdentityUnitOfWork _unitOfWork;
        public AccountController(
            SignInManager<User> signInManager,
            ITokenServices tokenServices,
            IUserAccessor userAccessor,
            IHttpContextAccessorHelper httpContextAccessorHelper,
            IIdentityUnitOfWork unitOfWork)
        {
            _signInManager = signInManager;
            _tokenServices = tokenServices;
            _userAccessor = userAccessor;
            _httpContextAccessorHelper = httpContextAccessorHelper;
            _unitOfWork = unitOfWork;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto loginDto)
        {
            var result = await _signInManager.PasswordSignInAsync(
                loginDto.Email,
                loginDto.Password,
                isPersistent: true,  // Set to true if you want "remember me" functionality
                lockoutOnFailure: false);  // Set to true to enable account lockout on failed attempts

            if (!result.Succeeded) return Unauthorized();

            var user = await _userAccessor.GetUserAsync();

            if (user == null) return Unauthorized();

            var roles = await _signInManager.UserManager.GetRolesAsync(user);
            var accessToken = await _tokenServices.CreateAccessTokenAsync(user);
            Response.Cookies.Append("access_token", accessToken.Token, new CookieOptions
            {
                HttpOnly = true, //httponly, k cho phep FE doc cookies
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = accessToken.Expires
            });
            var ipAddress = _httpContextAccessorHelper.GetClientIp();
            var refreshToken = _tokenServices.CreateRefreshToken(user, ipAddress);
            await _unitOfWork.RefreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.CommitAsync();

            Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true, //httponly, k cho phep FE doc cookies
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = refreshToken.Expires
            });

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var userInforJson = JsonSerializer.Serialize(new UserDto
            {
                DisplayName = user.DisplayName ?? string.Empty,
                Id = user.Id,
                ImageUrl = user.Image?.Url ?? string.Empty,
                Roles = roles.ToList(),
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Gender = user.Gender.ToString(),
                TotalSpent = user.TotalSpent,
            }, options); //set cookies nhung thong tin co ban cua user
            Response.Cookies.Append("user", userInforJson, new CookieOptions
            {
                HttpOnly = false, // cho phep FE doc cookies
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = accessToken.Expires,
                IsEssential = true
            });
            return Ok(new UserDto
            {
                DisplayName = user.DisplayName ?? string.Empty,
                Id = user.Id,
                ImageUrl = user.Image?.Url ?? string.Empty,
                Roles = roles.ToList(),
                Email = user.Email ?? string.Empty,
                // user.Photos,
            });

            // return Ok( new {token});
        }

        [HttpGet("user-info")]
        [Authorize]
        public async Task<ActionResult> GetUserInfo()
        {
            if (User.Identity?.IsAuthenticated == false) return NoContent();

            var user = await _userAccessor.GetUserAsync();

            if (user == null) return Unauthorized("User not found");

            var roles = await _signInManager.UserManager.GetRolesAsync(user);
            List<string> listRoles = new List<string>(roles);
            // var listNotiGrs = await _notificationGroupRepository.GetNotificationGroupByUserId(user.Id);
            return Ok(new UserDto
            {
                DisplayName = user.DisplayName ?? string.Empty,
                Id = user.Id,
                ImageUrl = user.Image?.Url ?? string.Empty,
                Roles = listRoles,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Gender = user.Gender.ToString(),
                Email = user.Email ?? string.Empty,
                // NotificationGroupIds = listNotiGrs.Select(x => x.Id).ToList(),
                IsAdmin = user.IsAdmin
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser(RegisterDto registerDto)
        {
            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                DisplayName = registerDto.DisplayName,
            };

            var result = await _signInManager.UserManager.CreateAsync(user, registerDto.Password);


            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("error", error.Description);
                }
                return BadRequest(ModelState); // lỗi tạo user
            }
            else
            {
                // await SendConfirmationEmailAsync(user, registerDto.Email);
            }

            // await Mediator.Send(new CreateBasketCommand { UserId = user.Id });
            await _signInManager.UserManager.AddToRoleAsync(user, "Member");


            return Ok(new UserDto
            {
                DisplayName = user.DisplayName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Id = user.Id,
                ImageUrl = user.Image?.Url ?? string.Empty,
                TotalSpent = user.TotalSpent,
                Roles = ["Member"],
                // user.Photos,
            });
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {       
            var token = Request.Cookies["refresh_token"];
            string ipAddress = _httpContextAccessorHelper.GetClientIp();
            if (!string.IsNullOrEmpty(token))
            {
                await _unitOfWork.RefreshTokenRepository.RevokeAsync(ipAddress: ipAddress, token: token, reason: "logout");
                // if (refreshToken != null && refreshToken.IsActive)
                // {
                //     await _refreshTokenRepository.RevokeAsync(token, ipAddress, "logout");
                //     await _unitOfWork.CommitAsync(CancellationToken.None);
                // }
                await _unitOfWork.CommitAsync();
            }
            var clearOptions = new CookieOptions { SameSite = SameSiteMode.Lax, Secure = true };
            Response.Cookies.Delete("access_token", clearOptions);
            Response.Cookies.Delete("refresh_token", clearOptions);
            Response.Cookies.Delete("user", clearOptions);
            await _signInManager.SignOutAsync();
            return NoContent();
        }

        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto passwordDto)
        {
            //b1: check user
            var user = await _signInManager.UserManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            //b2: doi mat khau
            var result = await _signInManager.UserManager
                .ChangePasswordAsync(user, passwordDto.CurrentPassword, passwordDto.NewPassword);

            if (!result.Succeeded) return BadRequest(result.Errors.First().Description);

            //b3: revoke token
            var ipAddress = _httpContextAccessorHelper.GetClientIp();
            await _unitOfWork.RefreshTokenRepository.RevokeAsync(user.Id, ipAddress, "pwchanged");
            await _unitOfWork.CommitAsync();

            //b4: xoa cookies
            var clearOptions = new CookieOptions { SameSite = SameSiteMode.Lax, Secure = true };
            Response.Cookies.Delete("access_token", clearOptions);
            Response.Cookies.Delete("refresh_token", clearOptions);
            Response.Cookies.Delete("user", clearOptions);

            //b5: logout
            await _signInManager.SignOutAsync();

            return Ok("Change password successfully for user " + user.UserName);
        }

        [AllowAnonymous]
        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            //b1: lay rf token tu cookie
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized();

            //b2: kiem tra rf token co hop le khong, neu co thi revoke and replace by token moi
            var tokenInDb = await _unitOfWork.RefreshTokenRepository.GetActiveByTokenAsync(refreshToken);
            if (tokenInDb == null || !tokenInDb.IsActive) return Unauthorized();

            var user = await _signInManager.UserManager.FindByIdAsync(tokenInDb.UserId);
            if (user == null) return Unauthorized();

            var newAccessToken = await _tokenServices.CreateAccessTokenAsync(user);
            var ipAddress = _httpContextAccessorHelper.GetClientIp();
            var newRefreshToken = _tokenServices.CreateRefreshToken(user, ipAddress);

            await _unitOfWork.RefreshTokenRepository.RevokeAsync(refreshToken, ipAddress, "refresh"); //revoke refresh token cu

            //b3: cap nhat rf token moi vao db va update token cu
            await _unitOfWork.RefreshTokenRepository.AddAsync(newRefreshToken);
            await _unitOfWork.CommitAsync();

            //b4: gan vao response
            Response.Cookies.Append("access_token", newAccessToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = newAccessToken.Expires
            });

            Response.Cookies.Append("refresh_token", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = newRefreshToken.Expires
            });

            var roles = await _signInManager.UserManager.GetRolesAsync(user);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var userInforJson = JsonSerializer.Serialize(new UserDto
            {
                DisplayName = user.DisplayName ?? string.Empty,
                Id = user.Id,
                ImageUrl = user.Image?.Url ?? string.Empty,
                Roles = roles.ToList(),
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Gender = user.Gender.ToString(),
                TotalSpent = user.TotalSpent,
            }, options); //set cookies nhung thong tin co ban cua user
            Response.Cookies.Append("user", userInforJson, new CookieOptions
            {
                HttpOnly = false, // cho phep FE doc cookies
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = newAccessToken.Expires,
                IsEssential = true
            });
            var response = new
            {
                accessTokenExpiration = newAccessToken.Expires
            };
            return Ok(response);
        }

        [HttpPut("update-photo")]
        public async Task<IActionResult> UpdateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            return HandleAppResult(await Mediator.Send(new UpdateUserImageCommand { UserId = userId, NewImage = file }));
            
        }
    }
}