using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CanalIlhas.Controllers
{
    public class ContaController : Controller
    {
        private readonly string DominioNome, UsuarioNome, identityUser;
        private readonly bool validado;

        [AllowAnonymous]
        public IActionResult Conta()
        {
            try
            {
                string identityUser = HttpContext.User.Identity.Name.ToUpper().Trim();
                if (identityUser.Contains("\\"))
                {
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                return View();
            }
            return View();
        }

        public IActionResult Autenticacao()
        {
            return null;
        }

        public IActionResult NaoAutorizado()
        {
            return View();
        }

        public IActionResult Proibido()
        {
            return View();
        }

    }
}