using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Seguranca.DTO.Request;
using Seguranca.DTO.Response;
using Seguranca.ServiceAgent;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CanalIlhas.Repository.Seguranca
{
    public class SegurancaRepository
    {
        private readonly string AplicacaoNomeTeste = "LOTUS";
        private readonly string _aplicacaoNome;
        private readonly string _waSeguranca = "HTTPS://WASEGURANCA.BRASILCENTER.COM.BR/WASEGURANCA";
        private readonly string _waSegurancat = "http://localhost:52578";
        //private static readonly HttpClient client = new HttpClient();
        private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IOptions<> appSettinhgs;
        ////waSegurancaUrl
        //public SegurancaRepository(IConfiguration configuration)
        //{
        //    _waSeguranca = configuration.GetSection("APIEXTERNA")["WASEGURANCA"];
        //    _aplicacaoNome = configuration.GetSection("APLICACAO")["NOME"];
        //}

        public void OnGet([FromServices]IConfiguration config)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string baseURL = config.GetSection("MySettings:WebApiBaseUrl").Value;
                HttpResponseMessage response = client.GetAsync(
                baseURL + "User/GetAllUsers").Result;
                response.EnsureSuccessStatusCode();
                string conteudo = response.Content.ReadAsStringAsync().Result;
                dynamic resultado = JsonConvert.DeserializeObject(conteudo);
            }
        }


        public ObterUsuarioResponse GetUserByLogin(string pUsername)
        {
            try
            {
                return new UsuarioServiceAgent().ObterUsuarioPorLogin(new ObterUsuarioPorLoginRequest { Aplicacao = AplicacaoNomeTeste, Login = pUsername });
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
                //   throw new Exception(SecurityMessages.GET_USER_BY_LOGIN);
            }
        }


        //Usuario/ObterUsuarioPorLogin
        //public async Task<List<UsersModel>> GetUsers()
        //{
        //    var requestUrl = CreateRequestUri(string.Format(System.Globalization.CultureInfo.InvariantCulture, 
        //        "User/GetAllUsers"));
        //    return await GetAsync<List<UsersModel>>(requestUrl);
        //}


        public ObterUsuarioResponse ObterUsuarioPorLogin(string pUsuarioNome)
        {
            try
            {
                return new UsuarioServiceAgent().ObterUsuarioPorLogin(new ObterUsuarioPorLoginRequest { Aplicacao = AplicacaoNomeTeste, Login = pUsuarioNome });
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        public AutenticarResponse Autenticar(int pCadastro, string pSenha)
        {

            try
            {
                return new UsuarioServiceAgent().Autenticar(new AutenticarRequest { Aplicacao = _aplicacaoNome, Senha = pSenha });
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        public ObterUsuarioResponse ObterUsuarioPorMatricula(int pMatricula)
        {
            try
            {
                return new UsuarioServiceAgent().ObterUsuarioPorMatricula(new ObterUsuarioPorMatriculaRequest { Aplicacao = _aplicacaoNome, Matricula = pMatricula});
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }

        public bool ValidarCredenciais(ValidateCredentialsRequest pValidarPedidoCredenciais)
        {
            try
            {
                return new UsuarioServiceAgent().ValidateCredentials(pValidarPedidoCredenciais).Sucesso;
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
        }
    }
}
