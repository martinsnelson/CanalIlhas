using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Seguranca.DTO.Request;
using Seguranca.DTO.Response;
using Seguranca.ServiceAgent;
using System;
using System.Net.Http;
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


        public ObterUsuarioResponse ObterUsuarioPorLogin(string pUsuarioNome)
        {
            try
            {
                pUsuarioNome = "NMARTIN";
                var serializer = JsonConvert.SerializeObject(pUsuarioNome);

                //var streamTask = client.GetStreamAsync("HTTPS://WASEGURANCA.BRASILCENTER.COM.BR/WASEGURANCA");

                //var result = client.GetAsync(_waSegurancat).Result;
                //if (result.IsSuccessStatusCode) { }

                return new UsuarioServiceAgent().ObterUsuarioPorLogin(new ObterUsuarioPorLoginRequest { Aplicacao = AplicacaoNomeTeste, Login = serializer });
                //throw new NotImplementedException();
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
