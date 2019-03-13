using CanalIlhas.Models;
using CanalIlhas.Models.Conta;
using CanalIlhas.Models.Menu;
using CanalIlhas.Repository.Seguranca;
using Krypton;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Seguranca.DTO;
using Seguranca.DTO.Request;
using Seguranca.DTO.Response;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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

        private readonly SignInManager<UsuarioAplicativo> _signInManager;

        //const string UsuarioMaster;
        //const string NmUsuarioMaster;
        //const string NmCasMaster;

        //public ContaController(IConfiguration configuration, SignInManager<UsuarioAplicativo> signInManager)
        //{
        //    string _PTI = configuration.GetSection("PTI")["PTI"];
        //    _signInManager = signInManager;
        //}

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync(ContaViewModel model)
        {

            if (ModelState.IsValid)
            {
                var isValid = true; // TODO Validate the username and the password with your own logic
                if (!isValid)
                {
                    ModelState.AddModelError("", "username or password is invalid");
                    return View("Login", model);
                }

                identityUser = model.UsuarioNome.ToUpper().Trim();
                if (identityUser.Contains("\\"))
                {
                    ud = identityUser.Split("\\");
                    DominioNome = ud[0] = "BRASILCENTER";
                    UsuarioNome = ud[1] = "NMARTIN";
                }

                //model.UsuarioNome;
                var logado = new SegurancaRepository().ObterUsuarioPorLogin(UsuarioNome);
                if (logado.Sucesso)
                {
                    var claimsLogin = new ClaimsIdentity();
                    var claims = new List<Claim>();
                    var principal = new ClaimsPrincipal();

                    var ClaimsPerfil = from form in logado.Usuario.Perfis.Select(a => a.TxPerfil) select new Claim("Perfil", form);
                    if (ClaimsPerfil.Select(a => a.Value).Contains(_PTI))
                    {
                        claimsLogin = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

                        claimsLogin.AddClaim(new Claim("UsuarioMaster", logado.Usuario.Nome));
                        claimsLogin.AddClaim(new Claim("NmUsuarioMaster", logado.Usuario.Matricula.ToString()));
                        claimsLogin.AddClaim(new Claim("NmCasMaster", logado.Usuario.Cas));

                        principal = new ClaimsPrincipal(claimsLogin);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false });

                        return RedirectToAction(nameof(ContaController.Login));
                    }
                    else
                    {                                             
                        claims.Add(new Claim(ClaimTypes.Name, logado.Usuario.Nome));
                        claims.Add(new Claim("NomeUser", logado.Usuario.Nome));
                        claims.Add(new Claim("MatrUser", logado.Usuario.Matricula.ToString()));
                        claims.Add(new Claim("CasUser", logado.Usuario.Cas));
                        claims.Add(new Claim("IdCasUser", logado.Usuario.IdCas.ToString()));
                        claims.Add(new Claim("UserName", logado.Usuario.Login));
                        claims.Add(new Claim("UserNameSup", logado.Usuario.LoginSupervisor));
                        claims.Add(new Claim("NomeSup", logado.Usuario.NomeSupervisor));
                        claims.Add(new Claim("DtAdmissao", logado.Usuario.DataAdmissao.ToShortDateString()));
                        claims.Add(new Claim("Cargo", logado.Usuario.Cargo));
                        claims.Add(new Claim("MatrUserTicket", logado.Usuario.Matricula.ToString()));
                        claims.Add(new Claim("NmUserTicket", logado.Usuario.Nome.ToString()));
                        claims.Add(new Claim("NmCasTicket", logado.Usuario.Cas));
                        // You can add roles to use role-based authorization
                        // identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                        // Authenticate using the identity
                        claims.AddRange(ClaimsPerfil);

                        var ClaimsForm = from form in logado.Usuario.Permissoes.Select(a => a.Form).Distinct() select new Claim("Form", form);
                        claims.AddRange(ClaimsForm);

                        var ClaimsRole = from role in logado.Usuario.Permissoes.Select(a => a.Seg).Distinct() select new Claim(ClaimTypes.Role, role);
                        claims.AddRange(ClaimsRole);

                        List<string> lstForm = logado.Usuario.Permissoes.OrderBy(a => a.Form).OrderBy(a => a.TxFormulario).Select(a => a.Form).AsEnumerable().Distinct().ToList();
                        List<string> lstTxForm = logado.Usuario.Permissoes.OrderBy(a => a.Form).OrderBy(a => a.TxFormulario).Select(a => a.TxFormulario).AsEnumerable().Distinct().ToList();

                        claims.Add(new Claim("Menu", Menu(lstForm, lstTxForm, false)));

                        claimsLogin = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        principal = new ClaimsPrincipal(claimsLogin);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false });
                    }
                }
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        protected string Menu(List<string> vForm, List<string> vTextForm, bool perfilTI)
        {
            StringBuilder _menu = new StringBuilder();
            var lstMenu = BuildMenu(vForm, vTextForm);

            int i = 0;
            string menuAnt = null;
            string subMenuAnt = null;
            string subMenu2Ant = null;

            _menu.Append("<nav class='navbar navbar-m2p sidebar' role='navigation'>");
            _menu.Append("<div class='container-fluid'>");
            _menu.Append("<ul class='nav navbar-nav'>");
            _menu.Append("<li><a href ='" + Url.Action("Index", "Home") + "'> Início&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span style='font-size:25px;' class='pull-right hidden-xs showopacity fa fa-home'></span></a></li>");
            foreach (var text in lstMenu)
            {
                i++;
                if (text.ItemSub == null)
                {
                    _menu.Append("<li><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemMaster + "<div class='pull-right hidden-xs showopacity iconCircle'>" + text.Icon + "</div></a></li>");
                }
                else if (text.ItemSub != null && text.ItemSub2 == null)
                {
                    try
                    {
                        if (text.ItemMaster == lstMenu[i].ItemMaster && text.ItemMaster != menuAnt)
                        {
                            _menu.Append("<li  class=''><a href = '#' class='panel-collapse' data-toggle='dropdown'> " + text.ItemMaster + " <span style='font-size:25px;' class='pull-right hidden-xs showopacity fa fa-arrow-circle-down'></span></a>");
                            _menu.Append("<ul class='dropdown-menu' role='menu'>");
                            _menu.Append("<li><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub + "<div  class='pull-right hidden-xs showopacity iconCircle'>" + text.Icon + "</div></a></li>");
                        }
                        else if (text.ItemMaster == lstMenu[i].ItemMaster && text.ItemMaster == menuAnt && text.ItemSub != subMenuAnt && subMenu2Ant != null)
                        {
                            _menu.Append("</ul>");
                            _menu.Append("</li>");
                            //   _menu.Append("</ul>");
                            //   _menu.Append("</li>");
                            _menu.Append("<li><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub + "<div  class='pull-right hidden-xs showopacity iconCircle'>" + text.Icon + "</div></a></li>");
                        }
                        else if (text.ItemMaster == lstMenu[i].ItemMaster && text.ItemMaster == menuAnt)
                        {
                            _menu.Append("<li><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub + "<div  class='pull-right hidden-xs showopacity iconCircle'>" + text.Icon + "</div></a></li>");
                        }
                        else if (text.ItemMaster != lstMenu[i].ItemMaster && text.ItemMaster == menuAnt && subMenu2Ant != null)
                        {
                            _menu.Append("</ul>");
                            _menu.Append("</li>");
                            _menu.Append("<li><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub + "<div class='pull-right hidden-xs showopacity iconCircle'>" + text.Icon + "</div></a></li>");
                            _menu.Append("</ul>");
                            _menu.Append("</li>");
                        }
                        else if (text.ItemMaster != lstMenu[i].ItemMaster && text.ItemMaster == menuAnt)
                        {
                            _menu.Append("<li><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub + "<div class='pull-right hidden-xs showopacity iconCircle'>" + text.Icon + "</div></a></li>");
                            _menu.Append("</ul>");
                            _menu.Append("</li>");
                        }
                        else
                        {
                            _menu.Append("<li  class=''><a href = '#' class='panel-collapse' data-toggle='dropdown'> " + text.ItemMaster + " <span style='font-size:25px;' class='pull-right hidden-xs showopacity fa fa-arrow-circle-down'></span></a>");
                            _menu.Append("<ul class='dropdown-menu' role='menu'>");
                            _menu.Append("<li><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub + "<div class='pull-right hidden-xs showopacity iconCircle'>" + text.Icon + "</div></a></li>");
                            _menu.Append("</ul>");
                            _menu.Append("</li>");
                        }
                    }
                    catch
                    {
                        _menu.Append("<li><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub + "<div class='pull-right hidden-xs showopacity iconCircle'>" + text.Icon + "</div></a></li>");
                        _menu.Append("</ul>");
                        _menu.Append("</li>");
                    }
                }
                else
                {
                    try
                    {
                        if (text.ItemMaster == lstMenu[i].ItemMaster && text.ItemMaster != menuAnt && text.ItemSub != subMenuAnt)
                        {
                            _menu.Append("<li  class=''><a href = '#' class='panel-collapse' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'> " + text.ItemMaster + " <span style='font-size:25px;' class='pull-right hidden-xs showopacity fa fa-arrow-circle-down'></span></a>");
                            _menu.Append("<ul class='dropdown-menu' role='menu'>");
                            _menu.Append("<li><a href = '#' class='dropdown-menu' data-toggle='dropdown' role='menu' disabled='disabled'> " + text.ItemSub + " <span style='font-size:25px;' class='pull-right hidden-xs showopacity fa fa-arrow-circle-down'></span></a>");
                            _menu.Append("<ul class='dropdown-submenu' role='submenu'>");
                            _menu.Append("<li class='dropdown-submenu'><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub2 + "</a></li>");
                        }
                        else if (text.ItemMaster == lstMenu[i].ItemMaster && text.ItemMaster == menuAnt && text.ItemSub != subMenuAnt)
                        {
                            //  _menu.Append("<li  class=''><a href = '#' class='dropdown-toggle' data-toggle='dropdown'> " + text.ItemMaster + " <span style='font-size:25px;' class='pull-right hidden-xs showopacity fa fa-arrow-circle-down'></span></a>");
                            //  _menu.Append("<ul class='dropdown-menu' role='menu'>");
                            _menu.Append("<li><a href = '#' class='dropdown-menu' data-toggle='dropdown' role='menu' disabled='disabled'> " + text.ItemSub + " <span style='font-size:25px;' class='pull-right hidden-xs showopacity fa fa-arrow-circle-down'></span></a>");
                            _menu.Append("<ul class='dropdown-submenu' role='submenu'>");
                            _menu.Append("<li class='dropdown-submenu'><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub2 + "</a></li>");
                        }
                        else if (text.ItemMaster == lstMenu[i].ItemMaster && text.ItemMaster == menuAnt && text.ItemSub == subMenuAnt)
                        {
                            _menu.Append("<li class='dropdown-submenu'><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub2 + "</a></li>");
                        }
                        else if (text.ItemMaster != lstMenu[i].ItemMaster && text.ItemMaster == menuAnt && text.ItemSub == subMenuAnt)
                        {
                            _menu.Append("<li class='dropdown-submenu'><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub2 + "</a></li>");
                            _menu.Append("</ul>");
                            _menu.Append("</li>");
                            _menu.Append("</ul>");
                            _menu.Append("</li>");
                        }
                        else
                        {
                            _menu.Append("<li  class=''><a href = '#' class='panel-collapse' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'> " + text.ItemMaster + " <span style='font-size:25px;' class='pull-right hidden-xs showopacity fa fa-arrow-circle-down'></span></a>");
                            _menu.Append("<ul class='dropdown-menu' role='menu'>");
                            _menu.Append("<li><a href = '#' class='dropdown-menu' data-toggle='dropdown' role='menu' disabled='disabled'> " + text.ItemSub + " <span style='font-size:25px;' class='pull-right hidden-xs showopacity fa fa-arrow-circle-down'></span></a>");
                            _menu.Append("<ul class='dropdown-submenu' role='submenu'>");
                            _menu.Append("<li class='dropdown-submenu'> <a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub2 + "</a></li>");
                            _menu.Append("</ul>");
                            _menu.Append("</li>");
                            _menu.Append("</ul>");
                            _menu.Append("</li>");
                        }
                    }
                    catch
                    {
                        _menu.Append("<li><a href ='" + Url.Action("Index", text.Controller) + "'> " + text.ItemSub2 + "<div class='pull-right hidden-xs showopacity iconCircle'>" + text.Icon + "</div></a></li>");
                        _menu.Append("</ul>");
                        _menu.Append("</li>");
                        _menu.Append("</ul>");
                        _menu.Append("</li>");
                    }

                }
                menuAnt = text.ItemMaster;
                subMenuAnt = text.ItemSub;
                subMenu2Ant = text.ItemSub2;
            }
            if (perfilTI)
            {
                _menu.Append("<li><a href ='" + Url.Action("Logout", "Login") + "'> Trocar Usuário<span style='font-size:20px;' class='pull-right hidden-xs showopacity fa fa-user-times'></span></a></li>");
            }

            _menu.Append("</ul>");
            _menu.Append("</div>");
            _menu.Append("</nav>");

            return _menu.ToString();
        }

        protected IList<MenuModel> BuildMenu(List<string> vForm, List<string> vTextForm)
        {
            List<MenuModel> itens = new List<MenuModel>();
            var vIcons = (from a in MenuIconModel.listaIcons() select new { id = a.idIcon, desc = a.descIcon }).ToList();

            decimal p = 0;
            foreach (var text in vTextForm)
            {
                p++;
                decimal s = 0;
                string[] menu = text.ToString().Split(';');

                foreach (var ctrll in vForm)
                {
                    string ico = null;
                    foreach (var ic in vIcons)
                    {
                        if (ic.id == ctrll.ToString())
                            ico = ic.desc;
                    }
                    s++;
                    if (p == s)
                    {
                        itens.Add(new MenuModel
                        {
                            ItemMaster = menu[0],
                            ItemSub = menu.Length > 1 ? menu[1] : null,
                            ItemSub2 = menu.Length > 2 ? menu[2] : null,
                            Desc = menu.Length == 3 ? menu[2] : menu.Length == 2 ? menu[1] : menu[0],
                            Controller = ctrll.ToString(),
                            Icon = ico
                        });
                    }
                }
            }
            return itens;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string retornaUrl = null)
        {
            // Limpa o cookie externo existente para garantir um processo de login limpo
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["RetornaUrl"] = retornaUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(ContaViewModel model, string retornaUrl = null)
        {
            ViewData["RetornaUrl"] = retornaUrl;
            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(model.UsuarioNome, model.Senha, model.Lembrar, lockoutOnFailure: false);
                if (resultado.Succeeded)
                {
                    return RedirecionarParaLocal(retornaUrl);
                }
                if (resultado.RequiresTwoFactor)
                {
                }
                if (resultado.IsLockedOut)
                {
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirecionarParaLocal(string retornaUrl)
        {
            if (Url.IsLocalUrl(retornaUrl))
            {
                return Redirect(retornaUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
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
                if (model.UsuarioNome.Contains("\\"))
                {
                    ud = model.UsuarioNome.ToUpper().Trim().Split("\\");
                    DominioNome = ud[0];
                    UsuarioNome = ud[1];
                }
                else
                {
                }
                if (!_SERVERS.Contains(DominioNome.ToUpper().Trim()))
                {
                    ModelState.AddModelError("Error_Authentic", "Usuário/Senha Inválido");
                    return View("Conta", model);
                }

                var credenciais = new CredentialDto { Domain = DominioNome, Username = UsuarioNome, Password = model.Senha  };
                var serializarCredenciais = JsonConvert.SerializeObject(credenciais);
                var credenciaisSerializeCriptografadas = KryptonHelper.Encriptar(serializarCredenciais, "15m");
                var logado = new SegurancaRepository().ValidarCredenciais(new ValidateCredentialsRequest { Credential = credenciaisSerializeCriptografadas });
                if (logado)
                {
                    var logadoOK = new SegurancaRepository().ObterUsuarioPorLogin(UsuarioNome);
                    if (logadoOK.Sucesso)
                    {
                        var claimsPerfil = from form in logadoOK.Usuario.Perfis.Select(a => a.TxPerfil) select new Claim("Perfil", form);
                        if (claimsPerfil.Select(a => a.Value).Contains(_PTI))
                        {
                            //HttpContext.Session.SetString("", "");

                            //HttpContext.Session.Set(UsuarioMaster, logadoOK.Usuario.Matricula.ToString());
                            
                            //["UsuarioMaster"] = logadoOK.Usuario.Matricula.ToString();
                            //Session["NmUsuarioMaster"] = logadoOK.Usuario.Nome;
                            //Session["NmCasMaster"] = logadoOK.Usuario.Cas;
                            return RedirectToAction("TIPerfil", "Conta");
                        }
                    }
                }

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

        public IActionResult Negado()
        {
            return View();
        }

    }
}