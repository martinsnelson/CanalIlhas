using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CanalIlhas.Repository.Message
{
    [DataContract]
    public class Mensagens<T>
    {
        [DataMember(Name = "Sucesso")]
        public bool Sucesso { get; set; }

        [DataMember(Name = "ReturnoMensagem")]
        public string ReturnoMensagem { get; set; }

        [DataMember(Name = "Dados")]
        public T Dados { get; set; }
    }
}
