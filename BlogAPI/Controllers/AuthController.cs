using BlogAPI.Dto;
using BlogAPI.Dto.OtherObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlogAPI.Interface;

namespace BlogAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenBlacklist _tokenBlacklist;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ILogger<AuthController> logger, ITokenBlacklist tokenBlacklist)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _tokenBlacklist = tokenBlacklist;
            _logger = logger;
        }

        //Route for seeding roles to the Database.
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [Route("seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            bool isUserExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);
            bool isAdminExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
            bool isOwnerExists = await _roleManager.RoleExistsAsync(StaticUserRoles.OWNER);

            if (isUserExists && isAdminExists && isOwnerExists)
                return Ok("Role already exists");

            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.OWNER));

            return Ok("Role Seeding to the Database succeeded.");
        }

        //Route for registering users
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var isUserExists = await _userManager.FindByNameAsync(registerDto.UserName);

            if (isUserExists != null)
            {
                return BadRequest("User already exists");
            }

            IdentityUser newUser = new IdentityUser()
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var createdUserResults = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!createdUserResults.Succeeded)
            {
                var errorString = "User creation failed becasue ";
                foreach (var error in createdUserResults.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return BadRequest(errorString);
            }

            //Adding a default user role on creation of the user. 
            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
            return Ok("User successfully created😁😁.");
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user is null)
                return Unauthorized("Invalid Credentials");

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordCorrect)
                return Unauthorized("Invalid Credentials");

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("JWTID", Guid.NewGuid().ToString())
            };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateNewJsonWebToken(authClaims);

            return Ok(token);
        }

        //Function for Token Generation
        private string GenerateNewJsonWebToken(List<Claim> claims)
        {
            SymmetricSecurityKey authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var tokenObject = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(1),
                claims: claims,
                signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
            );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            return token;
        }


        //Route for Logging out the User
        [HttpPost]
        [Route("logout")]
        public IActionResult Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer", "");
            
            //Blacklisting the token
            _tokenBlacklist.BlacklistToken(token);
            
            _logger.LogInformation("User logged out at {Time}", DateTime.UtcNow);
            return NoContent();
        }
        //Route for adding roles to users
        [HttpPost]
        [Route("make-admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MakeAdmin([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)

                return BadRequest("Invalid User name!!!");
            await _userManager.AddToRoleAsync(user, StaticUserRoles.ADMIN);

            return Ok("User is now an Admin");

        }


        //Route for making user owner
        [HttpPost]
        [Route("make-owner")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MakeOwner([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)

                return BadRequest("Invalid User name!!!");
            await _userManager.AddToRoleAsync(user, StaticUserRoles.OWNER);

            return Ok("User is now an Owner");

        }
    }
}
    
