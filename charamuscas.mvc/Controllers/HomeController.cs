using charamuscas.entities.Entities;
using charamuscas.entities.Models;
using charamuscas.mvc.Helper;
using charamuscas.mvc.Models;
using charamuscas.services.Contextos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Diagnostics;

namespace charamuscas.mvc.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class HomeController : Controller
    {
        private readonly Contexto _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, Contexto db)
        {
            _db = db;
            _logger = logger;
        }
       
        public IActionResult Index()
        {
            return View();
        }
    }
}
