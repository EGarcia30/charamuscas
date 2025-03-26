using charamuscas.entities.Entities;
using charamuscas.entities.Models;
using charamuscas.mvc.Helper;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index(string search, int? numPag)
        {
            int cantidadRegistros = 6;
            if (!String.IsNullOrEmpty(search))
            {
                var usuariosFiltrado = await _db.vw_usuarios
                    .Where(x => x.nombres.Contains(search) || x.apellidos.Contains(search) || x.nombre_usuario.Contains(search)
                    || x.correo.Contains(search) || x.rol.Contains(search))
                    .OrderByDescending(x => x.PK_codigo)
                    .ToListAsync();

                return View(Paginacion<vw_usuarios>.CrearPaginacion(usuariosFiltrado.AsQueryable(), numPag ?? 1, cantidadRegistros));
            }

            var usuarios = await _db.vw_usuarios.OrderByDescending(x => x.PK_codigo).ToListAsync();
            return View(Paginacion<vw_usuarios>.CrearPaginacion(usuarios.AsQueryable(), numPag ?? 1, cantidadRegistros));
        }
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int id)
        {
            var usuario = await _db.vw_usuarios.FirstOrDefaultAsync(x => x.PK_codigo == id);
            return View(usuario);
        }
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _db.vw_usuarios.FirstOrDefaultAsync(x => x.PK_codigo == id);
            var roles = await _db.usuarios_rol.ToListAsync();

            //Convertir lista en SelectListItem
            var sliroles = roles.Select(x => new SelectListItem
            {
                Value = x.PK_codigo.ToString(),
                Text = $"{x.PK_codigo} - {x.nombre}"
            }).ToList();

            ViewBag.roles = sliroles;

            return View(usuario);
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Edit(vw_usuarios value)
        {
            try
            {
                if(value != null)
                {
                    //traer usuario de bd
                    var usuarioBD = await _db.usuarios.FirstOrDefaultAsync(x => x.PK_codigo == value.PK_codigo);

                    //cambiar información
                    usuarioBD.nombres = value.nombres;
                    usuarioBD.apellidos = value.apellidos;
                    //verificar si nombre de usuario existe
                    var usuarioExiste = await _db.usuarios.AnyAsync(x => x.nombre_usuario == value.nombre_usuario);
                    if (!usuarioExiste) usuarioBD.nombre_usuario = value.nombre_usuario;
                    usuarioBD.FK_rol = value.FK_rol;
                    usuarioBD.correo = value.correo;
                    usuarioBD.celular = value.celular;

                    //Guardar cambios en bd
                    await _db.SaveChangesAsync();

                    return RedirectToAction("Details", "Usuarios", new {id = value.PK_codigo});

                }
                return RedirectToAction("Index", "Usuarios");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Usuarios");
            }
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
                        case "Usuario":
                            return RedirectToAction("Index", "Venta");
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

        [Authorize(Roles ="Administrador")]
        public async Task<IActionResult> Register()
        {
            ViewBag.usuarios_rol = await _db.usuarios_rol.ToListAsync();
            return View();
        }
        [Authorize(Roles = "Administrador")]
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

        [Authorize(Roles = "Administrador,Contador,Usuario")]
        public IActionResult Logout()
        {
            Auth.DeleteCookie(HttpContext);
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> _Modal_Cambiar_Clave(int id)
        {
            var usuario = await _db.vw_usuarios.FirstOrDefaultAsync(x => x.PK_codigo == id);
            return PartialView(usuario);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<JsonResult> CambiarClave(int PK_codigo, string clave)
        {
            try
            {
                if (PK_codigo != 0)
                {
                    //Crear nueva contraseña
                    Hash.CreatePasswordHash(clave, out byte[] clave_hash, out byte[] clave_salt);

                    //traer usuario de bd
                    var usuarioBD = await _db.usuarios.FirstOrDefaultAsync(x => x.PK_codigo == PK_codigo);

                    //realizar cambios
                    usuarioBD.clave_hash = clave_hash;
                    usuarioBD.clave_salt = clave_salt;

                    //guardarlos en bd
                    await _db.SaveChangesAsync();

                    return Json(usuarioBD);
                }
                return Json(null);
            }
            catch (Exception ex)
            {
                return Json(null);
            }
        }
    }
}
