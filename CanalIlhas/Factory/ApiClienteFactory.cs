using CanalIlhas.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CanalIlhas.Factory
{
    internal static class ApiClienteFactory
    {
        private static Uri apiUri;

        private static Lazy<ApiCliente> restClient = new Lazy<ApiCliente>(
            () => new ApiCliente(apiUri), 
            LazyThreadSafetyMode.ExecutionAndPublication);

        static ApiClienteFactory()
        {
            apiUri = new Uri(AplicacaoConfig.WebApiUrl);
        }

        public static ApiCliente Instance
        {
            get
            {
                return restClient.Value;
            }
        }
    }
}
