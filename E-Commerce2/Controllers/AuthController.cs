using Carts.Entities;
using E_Commerce2.Mapping_Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RepositoryPatternWithUnitOfWork.Core;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Users.DTOs;
using Users.Entities;

namespace E_Commerce2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IUnitOfWork _unitOfWork, JwtOptions jwtOptions, IWebHostEnvironment env) : ControllerBase
    {
        private readonly IWebHostEnvironment _env = env;
        // ═══════════════════════════════════════════════════════════
        // Helper Methods
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// توليد Access Token (JWT قصير المدى - 15 دقيقة)
        /// </summary>
        private string GenerateAccessToken(Users.Entities.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = jwtOptions.Issuer,
                Audience = jwtOptions.Audienc,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    SecurityAlgorithms.HmacSha256)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        /// <summary>
        /// توليد Refresh Token (Random String آمن)
        /// </summary>
        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow
            };
        }

        /// <summary>
        /// حفظ Refresh Token في HttpOnly Cookie
        /// </summary>
        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {

                HttpOnly = true,

                // في الـ Production لازم HTTPS
                Secure = !_env.IsDevelopment(),
                // Secure = !Environment.IsDevelopment(),

                // عشان React (localhost:3000) مع API (localhost:5000)
                SameSite = SameSiteMode.None,

                Expires = expires.ToLocalTime(),//
                IsEssential = true,//
                Path = "/"//


                /*
Secure = !_env.IsDevelopment() // HTTPS في Production
HttpOnly = true                 // JS مش هيقدر يسرق Refresh Token
SameSite = SameSiteMode.None    // عشان يشتغل مع Angular أو React
                 */
                //HttpOnly = true,
                //Secure = true,
                //SameSite = SameSiteMode.Strict,
                //Expires = expires.ToLocalTime(),
                //IsEssential = true,
                //Path = "/"
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        /// <summary>
        /// مسح Refresh Token من الـ Cookie
        /// </summary>
        private void DeleteRefreshTokenCookie()
        {
            //Response.Cookies.Delete("refreshToken");
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                Path = "/",
                SameSite = SameSiteMode.None,
                Secure = !_env.IsDevelopment()
            });
        }

        // ═══════════════════════════════════════════════════════════
        // Authentication Endpoints
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Login - إرجاع Access Token + حفظ Refresh Token في Cookie
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto requestLogin)
        {
            // 1️⃣ التحقق من بيانات المستخدم
            //await _unitOfWork.Users.Login(requestLogin.Email, requestLogin.Password)
            var userAuth = await _unitOfWork.Users.Login(requestLogin.Email, requestLogin.Password);
            // await _unitOfWork.Users.GetByIdWithRefreshTokensAsync(  );
            if (userAuth == null)
            {
                return Unauthorized(new { message = "Email or Password is incorrect" });
            }

            // 2️⃣ توليد Access Token
            var accessToken = GenerateAccessToken(userAuth);

            // 3️⃣ توليد Refresh Token
            var refreshToken = GenerateRefreshToken();

            // 4️⃣ إضافة Refresh Token للمستخدم
            if (userAuth!.RefreshTokens == null)
                userAuth.RefreshTokens = new List<RefreshToken>();

            userAuth.RefreshTokens.Add(refreshToken);

            // 5️⃣ حذف الـ Tokens القديمة غير النشطة (Cleanup)
            userAuth.RefreshTokens.RemoveAll(t => !t.IsActive && t.CreatedOn.AddDays(30) < DateTime.UtcNow);

            await _unitOfWork.SaveChangesAsync();

            // 6️⃣ حفظ Refresh Token في HttpOnly Cookie
            SetRefreshTokenInCookie(refreshToken.Token, refreshToken.ExpiresOn);

            // 7️⃣ إرجاع Response
            return Ok(new
            {
                accessToken = accessToken,
                tokenType = "Bearer",
                expiresIn = 900,
                user = new
                {
                    id = userAuth.Id,
                    email = userAuth.Email,
                    roles = userAuth.Roles.Select(r => r.Role.ToString()).ToArray()
                }
            });
        }

        /// <summary>
        /// Register - تسجيل مستخدم جديد + Login تلقائي
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1️⃣ Validation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2️⃣ التحقق من البريد الإلكتروني
            var finedUserByEmail = await _unitOfWork.Users.FinedByEmail(dto.Email);
            if (finedUserByEmail == true)
                return BadRequest(new { message = "Email already registered" });

            // 3️⃣ التحقق من رقم الهاتف
            var finedUserByPhoneNumber = await _unitOfWork.Users.FinedByPhone(dto.PhoneNumber);
            if (finedUserByPhoneNumber == true)
                return BadRequest(new { message = "Phone Number already registered" });

            // 4️⃣ إنشاء المستخدم
            var userResult = Users.Entities.User.Create(
                dto.FirstName,
                dto.LastName,
                dto.Address,
                dto.BirthDate,
                dto.Email,
                dto.Password,
                dto.PhoneNumber
            );

            if (!userResult.IsSuccess)
                return BadRequest(new { message = userResult.Error });

            if (userResult.Value == null)
                return BadRequest(new { message = "Failed to create user" });

            await _unitOfWork.Users.AddAsync(userResult.Value);
            await _unitOfWork.SaveChangesAsync();

            // 5️⃣ إنشاء Cart تلقائيًا
            var cartResult = Cart.Create(userResult.Value!.Id);
            if (!cartResult.IsSuccess)
                return BadRequest(new { message = cartResult.Error });

            await _unitOfWork.Carts.AddAsync(cartResult.Value!);
            await _unitOfWork.SaveChangesAsync();

            // 6️⃣ توليد Tokens
            var accessToken = GenerateAccessToken(userResult.Value);
            var refreshToken = GenerateRefreshToken();

            // 7️⃣ إضافة Refresh Token
            if (userResult.Value.RefreshTokens == null)
                userResult.Value.RefreshTokens = new List<RefreshToken>();

            userResult.Value.RefreshTokens.Add(refreshToken);

            await _unitOfWork.SaveChangesAsync();

            // 8️⃣ حفظ في Cookie
            SetRefreshTokenInCookie(refreshToken.Token, refreshToken.ExpiresOn);

            // 9️⃣ إرجاع Response
            return Ok(new
            {
                accessToken = accessToken,
                tokenType = "Bearer",
                expiresIn = 900,
                user = new
                {
                    id = userResult.Value.Id,
                    email = userResult.Value.Email,
                    roles = userResult.Value.Roles.Select(r => r.Role.ToString()).ToArray()
                }
            });
        }

        /// <summary>
        /// Refresh Token - الحصول على Access Token جديد
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken()
        {
            // 1️⃣ قراءة Refresh Token من Cookie
            var token = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Refresh token not found" });

            // 2️⃣ البحث عن المستخدم والـ Token
            var users = await _unitOfWork.Users
                .FindAsync(u => u.RefreshTokens!.Any(t => t.Token == token));

            var user = users.FirstOrDefault();

            if (user == null)
                return Unauthorized(new { message = "Invalid refresh token" });

            // 3️⃣ الحصول على الـ Refresh Token
            var refreshToken = user.RefreshTokens!.Single(t => t.Token == token);

            // 4️⃣ التحقق من حالة الـ Token
            if (!refreshToken.IsActive)
            {
                // ⚠️ Token Reuse Detection - أمان إضافي
                if (refreshToken.RevokedOn != null)
                {
                    // Token استُخدم بعد ما اتمسح = هجوم محتمل!
                    // امسح كل الـ Tokens الخاصة بالمستخدم
                    foreach (var rt in user.RefreshTokens)
                    {
                        if (rt.IsActive)
                            rt.RevokedOn = DateTime.UtcNow;
                    }
                    await _unitOfWork.SaveChangesAsync();

                    DeleteRefreshTokenCookie();
                    return Unauthorized(new { message = "Token reuse detected. All sessions revoked." });
                }

                DeleteRefreshTokenCookie();
                return Unauthorized(new { message = "Refresh token is not active" });
            }

            // 5️⃣ إلغاء الـ Token القديم (Token Rotation)
            refreshToken.RevokedOn = DateTime.UtcNow;

            // 6️⃣ توليد Tokens جديدة
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            // 7️⃣ حذف الـ Tokens القديمة غير النشطة
            user.RefreshTokens.RemoveAll(t => !t.IsActive && t.CreatedOn.AddDays(30) < DateTime.UtcNow);

            await _unitOfWork.SaveChangesAsync();

            // 8️⃣ توليد Access Token جديد
            var newAccessToken = GenerateAccessToken(user);

            // 9️⃣ تحديث Cookie
            SetRefreshTokenInCookie(newRefreshToken.Token, newRefreshToken.ExpiresOn);

            return Ok(new
            {
                accessToken = newAccessToken,
                tokenType = "Bearer",
                expiresIn = 900
            });
        }

        /// <summary>
        /// Logout - مسح الـ Session الحالي فقط
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return Ok(new { message = "Already logged out" });

            var users = await _unitOfWork.Users
                .FindAsync(u => u.RefreshTokens!.Any(t => t.Token == token));

            var user = users.FirstOrDefault();

            if (user != null)
            {
                var refreshToken = user.RefreshTokens!.SingleOrDefault(t => t.Token == token);
                if (refreshToken != null && refreshToken.IsActive)
                {
                    refreshToken.RevokedOn = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            DeleteRefreshTokenCookie();
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Revoke Token - إلغاء Token معين
        /// </summary>
        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenDto model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            var users = await _unitOfWork.Users
                .FindAsync(u => u.RefreshTokens!.Any(t => t.Token == token));

            var user = users.FirstOrDefault();

            if (user == null)
                return NotFound(new { message = "Token not found" });

            var refreshToken = user.RefreshTokens!.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
                return BadRequest(new { message = "Token is already inactive" });

            refreshToken.RevokedOn = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Token revoked successfully" });
        }

        /// <summary>
        /// Logout من كل الأجهزة (إلغاء كل الـ Sessions)
        /// </summary>
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutFromAllDevices()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var users = await _unitOfWork.Users.FindAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();

            if (user == null)
                return NotFound(new { message = "User not found" });

            // إلغاء كل الـ Refresh Tokens النشطة
            if (user.RefreshTokens != null)
            {
                foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
                {
                    token.RevokedOn = DateTime.UtcNow;
                }
                await _unitOfWork.SaveChangesAsync();
            }

            DeleteRefreshTokenCookie();
            return Ok(new { message = "Logged out from all devices successfully" });
        }

        /// <summary>
        /// الحصول على كل الـ Sessions النشطة
        /// </summary>
        [HttpGet("active-sessions")]
        [Authorize]
        public async Task<ActionResult> GetActiveSessions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var users = await _unitOfWork.Users.FindAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();

            if (user == null)
                return NotFound(new { message = "User not found" });

            var activeSessions = user.RefreshTokens?
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    token = t.Token.Substring(0, 10) + "...", // عرض جزء من الـ Token
                    createdOn = t.CreatedOn,
                    expiresOn = t.ExpiresOn,
                    isCurrentSession = t.Token == Request.Cookies["refreshToken"]
                })
                .OrderByDescending(t => t.createdOn)
                .ToList();

            return Ok(new
            {
                totalSessions = activeSessions?.Count ?? 0,
                sessions = activeSessions
            });
        }

        /// <summary>
        /// Get Current User
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var users = await _unitOfWork.Users.FindAsync(u => u.Id == userId);
            var user = users.FirstOrDefault();

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                roles = user.Roles.Select(r => r.Role.ToString()).ToArray()
            });
        }
    }

    // DTO للـ Revoke Token
    public class RevokeTokenDto
    {
        public string? Token { get; set; }
    }
}

















//using Carts.Entities;
//using E_Commerce2.Mapping_Configuration;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity.Data;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using RepositoryPatternWithUnitOfWork.Core;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;
//using Users.DTOs;
//using Users.Entities;

//namespace E_Commerce2.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AuthController(IUnitOfWork _unitOfWork, JwtOptions jwtOptions) : ControllerBase
//    {
//        [HttpPost]
//        [Route("loginAuth")]
//        public async Task<ActionResult<string>> AuthenticationUser(LoginDto requestLogin)
//        {
//            var userAuth = await _unitOfWork.Users.Login(requestLogin.Email,requestLogin.Password);
//            if (userAuth == null)
//            {
//                return Unauthorized("UserName Or Password is not Valid"); // stuts code 401 Unauthorized
//            }

//            // var roles = userResult.Value.Roles.ToArray();
//            //انا كده تاكدة ان اليوزر تمام عايزين نجنريت توكن بقى

//            //بنكريت التوكن بنفس القيم اللي مسجلها عندي ووخدها في الاوبجكت جي دابليو تي اوبشن ودا طبعا بعد ما اتاكدت ان اليوزر ده انا عارفه ومتسجل عندي في اليوزر تيبل وكمان ضفت معلومات عن اليوزر
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Issuer = jwtOptions.Issuer,
//                Audience = jwtOptions.Audienc,
//                Expires = DateTime.UtcNow.AddHours(1),
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
//                SecurityAlgorithms.HmacSha256),
//            };
//            //كده التوكن هتكون فلد لان هي بنفس القيم اللي عندي عايزين معلومات عن اليوزر

//            var claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.NameIdentifier, userAuth.Id.ToString()),
//                new Claim(ClaimTypes.Email, userAuth.Email)
//            };

//            foreach (var role in userAuth.Roles)
//            {
//                claims.Add(new Claim(ClaimTypes.Role, role.Role.ToString()));
//            }
//            tokenDescriptor.Subject = new ClaimsIdentity(claims);


//            //نكريت التوكن باستخدام الهندلر وبالموصفات اللي حددناها
//            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
//            //نحول الاوبجكت توكن ده لسترنج اللي هيتبعت
//            var accessToken = tokenHandler.WriteToken(securityToken);
//            return Ok(accessToken);

//        }

//        [HttpPost("Register")]
//        public async Task<IActionResult> Create([FromBody] RegisterDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            //  Check if email already exists
//            var finedUserByEmail = await _unitOfWork.Users.FinedByEmail(dto.Email);
//            if (finedUserByEmail == true)
//                return BadRequest(new { message = "Email already registered" });


//            var finedUserByPhoneNumber = await _unitOfWork.Users.FinedByPhone(dto.PhoneNumber);
//            if (finedUserByPhoneNumber == true)
//                return BadRequest(new { message = "Phone Number already registered" });

//            // var role = dto.Role ?? Roles.User;

//            var userResult = Users.Entities.User.Create(
//                dto.FirstName,
//                dto.LastName,
//                dto.Address,
//                dto.BirthDate,
//                dto.Email,
//                dto.Password,
//                dto.PhoneNumber
//            );

//            if (!userResult.IsSuccess)
//                return BadRequest(new { message = userResult.Error });

//            if (userResult.Value == null)
//                return BadRequest(new { message = "Failed to create user" });

//            await _unitOfWork.Users.AddAsync(userResult.Value);

//            await _unitOfWork.SaveChangesAsync();

//            // 2️⃣ إنشاء Cart تلقائيًا
//            var cartResult = Cart.Create(userResult.Value!.Id);

//            if (!cartResult.IsSuccess)
//                return BadRequest(cartResult.Error);

//            await _unitOfWork.Carts.AddAsync(cartResult.Value!);
//            await _unitOfWork.SaveChangesAsync();

//            // var roles = userResult.Value.Roles.ToArray();

//            var tokenHandler = new JwtSecurityTokenHandler();
//            var tokenDescriptor = new SecurityTokenDescriptor
//            {
//                Issuer = jwtOptions.Issuer,
//                Audience = jwtOptions.Audienc,
//                Expires = DateTime.UtcNow.AddHours(1),
//                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
//                SecurityAlgorithms.HmacSha256),
//            };

//            //كده التوكن هتكون فلد لان هي بنفس القيم اللي عندي عايزين معلومات عن اليوزر

//            var claims = new List<Claim>
//            {
//                new Claim(ClaimTypes.NameIdentifier, userResult.Value.Id.ToString()),
//                new Claim(ClaimTypes.Email, userResult.Value.Email)
//            };

//            foreach (var role in userResult.Value.Roles)
//            {
//                claims.Add(new Claim(ClaimTypes.Role, role.Role.ToString()));
//            }
//            tokenDescriptor.Subject = new ClaimsIdentity(claims);

//            //نكريت التوكن باستخدام الهندلر وبالموصفات اللي حددناها
//            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
//            //نحول الاوبجكت توكن ده لسترنج اللي هيتبعت
//            var accessToken = tokenHandler.WriteToken(securityToken);
//            return Ok(accessToken);


//        }

//    }
//}
