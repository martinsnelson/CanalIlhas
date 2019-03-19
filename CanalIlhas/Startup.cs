using CanalIlhas.Data;
using CanalIlhas.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CanalIlhas
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            //services.AddSession(options =>
            //{
            //    // Set a short timeout for easy testing.
            //    options.IdleTimeout = TimeSpan.FromSeconds(40);
            //    options.Cookie.HttpOnly = true;
            //});

            // WebApi Chamada
            services.Configure<ConfigsModel>(Configuration.GetSection("MySettings"));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<UsuarioAplicativo, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie Configuração
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromMinutes(1);
                options.LoginPath = "/Conta/Login";
                options.LogoutPath = "/Conta/Logout";
                options.AccessDeniedPath = "/Conta/Negado";
                options.SlidingExpiration = true;

            });

            services.AddAuthentication(options => 
            {
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie();

            //  Nome do cabeçalho padrão do Angular para enviar o token XSRF.
            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddHttpClient();

            //services.Configure<FormOptions>(x =>
            //{
            //    x.ValueLengthLimit = int.MaxValue;
            //    x.MultipartBodyLengthLimit = int.MaxValue;
            //});

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseCookiePolicy();
            /*
            app.UseCookieAuthentication(options => {
                // Defini o tipo de autenticação
                options.AuthenticationScheme = "Cookie";
                // url para o usuário não autenticado será direcionado
                options.LoginPAth = new PathString("/Conta/NaoAutorizado");
                // url para o usuário não autorizado
                options.AccessDeniedPath = new PathString("/Conta/Proibido");
                // Matem o usuário autenticado até seu cookie expirar
                options.AutomaticAuthenticate = true;
                options.AutomaticChallenge = true;
            });
            */
            //app.UseSession();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
