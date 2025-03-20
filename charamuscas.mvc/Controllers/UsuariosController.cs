using charamuscas.entities.Entities;
using charamuscas.entities.Models;
using charamuscas.mvc.Helper;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Security.Claims;

namespace charamuscas.mvc.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly Contexto _db;
        public UsuariosController(Contexto db) {
            _db = db;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(login login)
        {
            try
            {
                if (login != null)
                {
                    var verifyUser = await _db.vw_usuarios.AnyAsync(vu => vu.nombre_usuario.Equals(login.nombre_usuario));
                    if (!verifyUser)
                    {
                        //ViewBag.Message = "Usuario no encontrado.";
                        return View();
                    }

                    var userBd = await _db.vw_usuarios.FirstOrDefaultAsync(ub => ub.nombre_usuario.Equals(login.nombre_usuario));
                    if (!Hash.VerifyPasswordHash(login.clave, userBd!.clave_hash!, userBd.clave_salt!))
                    {
                        //ViewBag.Message = "Constraseña incorrecta.";
                        return View();
                    }

                    Auth.CreateCookie(HttpContext, userBd);

                    switch (userBd.rol)
                    {
                        case "Administrador":
                            return RedirectToAction("Index", "Home");
                        case "Contador":
                            return RedirectToAction("Index", "Home");
                        default:
                            return View();
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View();
            }
        }
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Register()
        {
            ViewBag.usuarios_rol = await _db.usuarios_rol.ToListAsync();
            return View();
        }
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<IActionResult> Register(usuarios user, string clave)
        {
            try
            {
                if (user != null)
                {
                    var usuarioRegistrado = await _db.vw_usuarios.AnyAsync(u => u.correo == user.correo);

                    if (usuarioRegistrado)
                    {
                        //ViewBag.Message = "Email ya registrado. Inicie sesión o regístrese con otra cuenta.";
                        return View();
                    }

                    Hash.CreatePasswordHash(clave, out byte[] clave_hash, out byte[] clave_salt);

                    usuarios nuevoUsuario = new usuarios
                    {
                        nombres = user.nombres,
                        apellidos = user.apellidos,
                        nombre_usuario = user.nombre_usuario,
                        clave_hash = clave_hash,
                        clave_salt = clave_salt,
                        FK_rol = user.FK_rol,
                        correo = user.correo,
                        celular = user.celular,
                    };
                    _db.Add(nuevoUsuario);
                    await _db.SaveChangesAsync();

                    var userClaim = await _db.vw_usuarios.FirstOrDefaultAsync(uc => uc.correo.Equals(nuevoUsuario.correo));
                    Auth.CreateCookie(HttpContext, userClaim!);
                    return RedirectToAction("Index", "Home");
                }
                return View();
            }
            catch (Exception ex)
            {
                //ViewBag.Message = $"Error: {ex.Message}";
                return View();
            }

        }

        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public IActionResult Logout()
        {
            Auth.DeleteCookie(HttpContext);
            return RedirectToAction("Index", "Home");
        }
    }
}
