using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MultiBiz.Server.Models;
using System.Net;
using System.Web;

namespace MultiBiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PasswordController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _cfg;
    private readonly IEmailSender _email;

    public PasswordController(UserManager<ApplicationUser> userManager, IConfiguration cfg, IEmailSender email)
    {
        _userManager = userManager;
        _cfg = cfg;
        _email = email;
    }

    public record ForgotPasswordRequest(string UserNameOrEmail);
    public record ResetPasswordRequest(string UserName, string Token, string NewPassword);

    [HttpPost("forgot")]
    public async Task<ActionResult> Forgot([FromBody] ForgotPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserNameOrEmail)) return Ok();
        var user = await _userManager.FindByNameAsync(req.UserNameOrEmail)
            ?? await _userManager.FindByEmailAsync(req.UserNameOrEmail);

        // Evitar user enumeration: misma respuesta aunque no exista
        if (user == null) return Ok();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var clientUrl = _cfg["Reset:PublicClientUrl"]?.TrimEnd('/') ?? "https://demo.localtest.me:5001";
        var resetUrl = $"{clientUrl}/reset?user={HttpUtility.UrlEncode(user.UserName!)}&token={HttpUtility.UrlEncode(token)}";

        var html = $@"
            <p>Hola {WebUtility.HtmlEncode(user.FullName ?? user.UserName)},</p>
            <p>Para restablecer su contraseña haga clic en el siguiente enlace:</p>
            <p><a href=""{resetUrl}"">Restablecer contraseña</a></p>
            <p>Si usted no solicitó este cambio, puede ignorar este mensaje.</p>
        ";

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            await _email.SendAsync(user.Email, "Restablecer contraseña", html);
        }

#if DEBUG
        // Útil en desarrollo para probar sin SMTP configurado
        return Ok(new { DevelopmentToken = token, ResetUrl = resetUrl });
#else
        return Ok();
#endif
    }

    [HttpPost("reset")]
    public async Task<ActionResult> Reset([FromBody] ResetPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Token) || string.IsNullOrWhiteSpace(req.NewPassword))
            return BadRequest("Datos incompletos.");

        var user = await _userManager.FindByNameAsync(req.UserName);
        if (user == null) return Ok(); // misma respuesta

        var result = await _userManager.ResetPasswordAsync(user, req.Token, req.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => $"{e.Code}: {e.Description}"));
        }
        return Ok();
    }
}
