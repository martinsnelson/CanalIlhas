using CanalIlhas.Models;
using CanalIlhas.Repository.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Seguranca.DTO.Response;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

namespace CanalIlhas.Controllers
{
    public class ContaController : Controller
    {
        private const string _SERVERS = "BRASILCENTER,EMBRATEL";
        private readonly string _PTI;
        private PrincipalContext _dominioContext;
        private UserPrincipal _encontradoUsuario;
        private string identityUser, DominioNome, UsuarioNome;
        private bool validado;
        private string[] ud;

        public ContaController(IConfiguration configuration)
        {
            string _PTI = configuration.GetSection("PTI")["PTI"];
            string teste = configuration.GetConnectionString("PTI");
        }

        [AllowAnonymous]
        public IActionResult Conta()
        {
            try
            {
                identityUser = HttpContext.User.Identity.Name.ToUpper().Trim();
                if (identityUser.Contains("\\"))
                {
                    ud = identityUser.Split("\\");
                    DominioNome = ud[0];
                    UsuarioNome = ud[1];
                }
                else if (!string.IsNullOrEmpty(UsuarioNome))
                {
                    _dominioContext = new PrincipalContext(ContextType.Domain, DominioNome);
                    _encontradoUsuario = UserPrincipal.FindByIdentity(_dominioContext, IdentityType.SamAccountName, UsuarioNome);
                    if (_encontradoUsuario != null) { validado = true; }
                    else if (!_SERVERS.Contains(DominioNome.ToUpper()) || !validado) { return View(); }
                    else
                    {
                        try
                        {
                            var logado = new SegurancaRepository().ObterUsuarioPorLogin(UsuarioNome);
                            if (logado.Sucesso)
                            {
                                //FormsAuthentication
                            }
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError("Error_Authentic", "Ocorreu erro inesperado.Tente mais tarde");
                            return View();
                        }
                    }
                }
                else
                {
                    return View();
                }
            }
            catch (Exception)
            {
                return View();
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Autenticacao(ContaViewModel model)
        {
            try
            {

            }
            catch (Exception e)
            {
                ModelState.AddModelError("Error_Authentic", e.Message);
                return View("Conta", model);
            }
            return View("Conta", model);
        }

        private ObterCredenciaisResponse Autenticacao()
        {
            string cookieSso = Request.Cookies["SSO"];
            var apps = new List<string>();
            /*s
            //Cookie não nulo e o usuário possui um perfil para o Lotus.
            if (cookieSso != null && cookieSso["Token"] != null)
            {
            }
            else if (cookieSso != null)
            {
                if (cookieSso["AppRequest"] == null)
                    cookieSso.Values.Add("AppRequest", "CANALILHAS");
                else
                    cookieSso["AppRequest"] = "CANALILHAS";
            }
            else
            {
                ookieSso = Response.Cookies["SSO"];
                cookieSso.Values.Add("AppRequest", "CANALILHAS");
            }
            Response.SetCookie(cookieSso);*/
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