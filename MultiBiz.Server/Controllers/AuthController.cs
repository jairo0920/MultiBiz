using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MultiBiz.Server.Models;
using MultiBiz.Server.Tenants;
using MultiBiz.Shared.Users;

namespace MultiBiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantProvider _tenantProvider;
    private readonly IConfiguration _cfg;

    public AuthController(UserManager<ApplicationUser> userManager, ITenantProvider tenantProvider, IConfiguration cfg)
    {
        _userManager = userManager;
        _tenantProvider = tenantProvider;
        _cfg = cfg;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest req)
    {
        // Validate username availability
        var existing = await _userManager.FindByNameAsync(req.UserName);
        if (existing != null) return Conflict("Usuario ya existe");

        var user = new ApplicationUser
        {
            UserName = req.UserName,
            Email = req.Email,
            FullName = req.FullName,
            TenantId = _tenantProvider.TenantId,
            EmailConfirmed = false,
            IsActive = false // requiere activación admin
        };
        var res = await _userManager.CreateAsync(user, req.Password);
        if (!res.Succeeded) return BadRequest(res.Errors);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await _userManager.FindByNameAsync(req.UserName);
        if (user == null) return Unauthorized();
        if (!await _userManager.CheckPasswordAsync(user, req.Password)) return Unauthorized();
        if (!user.IsActive) return Forbid();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim("tenant", user.TenantId.ToString())
        };
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_cfg["Jwt:ExpireMinutes"]!)),
            Issuer = _cfg["Jwt:Issuer"],
            Audience = _cfg["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });
        var jwt = tokenHandler.WriteToken(token);
        return new AuthResponse(jwt, user.UserName!, user.Email ?? "", user.TenantId);
    }

    [HttpPost("forgot")]
    public async Task<ActionResult> Forgot([FromBody] string userNameOrEmail)
    {
        var user = await _userManager.FindByNameAsync(userNameOrEmail) ??
                   await _userManager.FindByEmailAsync(userNameOrEmail);
        if (user == null) return Ok(); // do not reveal existence
        // TODO: generate password reset token and send email with link
        return Ok();
    }
}
