using Seguranca.DTO.Request;
using Seguranca.DTO.Response;
using Seguranca.ServiceAgent;
using System;
using System.Collections.Generic;
using System.Text;

namespace CanalIlhas.Repository.Seguranca
{
    public class SegurancaRepository
    {
        private readonly string AplicacaoNome = "CANALILHAS";
        private readonly string AplicacaoNomeTeste = "LOTUS";
        //private readonly IOptions<> appSettinhgs;
        ////waSegurancaUrl
        //public SegurancaRepository(IOptions)
        //{
        //}

        public ObterUsuarioResponse ObterUsuarioPorLogin(string pUsuarioNome)
        {
            try
            {
                return new UsuarioServiceAgent().ObterUsuarioPorLogin(new ObterUsuarioPorLoginRequest { Aplicacao = AplicacaoNomeTeste, Login = pUsuarioNome });
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
                return new UsuarioServiceAgent().Autenticar(new AutenticarRequest { Aplicacao = AplicacaoNome, Senha = pSenha });
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
                return new UsuarioServiceAgent().ObterUsuarioPorMatricula(new ObterUsuarioPorMatriculaRequest { Aplicacao = AplicacaoNome, Matricula = pMatricula});
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
