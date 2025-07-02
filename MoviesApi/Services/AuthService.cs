using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Helpers;
using MoviesApi.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoviesApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
            _roleManager = roleManager;
        }

        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();
            var user = _userManager.FindByEmailAsync(model.Email).Result;
            if (user == null ||!await _userManager.CheckPasswordAsync(user,model.Password))
            {
                authModel.Message = "Invalid email or password";
                return authModel;
            }
            var jwtSecurityToken = await CreateJwtToken(user);
            authModel.IsAuthinticated = true;
            authModel.Username = user.UserName;
            authModel.Email = user.Email;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.ExpireOn = jwtSecurityToken.ValidTo;

            var RoleList = await _userManager.GetRolesAsync(user);
            authModel.Roles = RoleList.ToList();
            return authModel;
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if(await _userManager.FindByEmailAsync(model.Email) != null)
            {
                return new AuthModel
                {
                    Message = "Email already exists",
                };
            }if(await _userManager.FindByNameAsync(model.Username) != null)
            {
                return new AuthModel
                {
                    Message = "username already exists",
                };
            }
            var user = new ApplicationUser
            {
                FName = model.FirstName,
                LName = model.LastName,
                UserName = model.Username,
                Email = model.Email
            };
           var result= await _userManager.CreateAsync(user, model.Password);
            if(!result.Succeeded)
            {
                var errors=string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description} ";
                }
                return new AuthModel
                {
                    Message = errors,
                };
            }
            await _userManager.AddToRoleAsync(user, "User");
            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Message = "User created successfully",
                IsAuthinticated = true,
                Username = user.UserName,
                Email = user.Email,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpireOn = jwtSecurityToken.ValidTo
            };
        }

        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
           var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid username or role";
            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User already has this role";

            var result = await _userManager.AddToRoleAsync(user, model.Role);
            return result.Succeeded ? "" : "Failed to add role";
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.ExpirationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
    }
}
