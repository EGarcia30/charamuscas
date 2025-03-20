using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using charamuscas.entities.Entities;

namespace charamuscas.mvc.Helper
{
    public class Auth
    {
        public async static void CreateCookie(HttpContext httpContext, vw_usuarios user)
        {
            var claims = new List<Claim>()
            {
                new Claim("id", user.PK_codigo.ToString()),
                new Claim("nombre_usuario", user.nombre_usuario),
                new Claim(ClaimTypes.Role, user.rol.ToString().ToLower()),
                new Claim(ClaimTypes.Email, user.correo),
                new Claim(ClaimTypes.MobilePhone, user.celular)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        public async static void DeleteCookie(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
