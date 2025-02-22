﻿using System.ComponentModel.DataAnnotations;

namespace CanalIlhas.Models.Conta
{
    public class ContaViewModel
    {
        [Required(ErrorMessage = "O campo usuário é obrigatório!")]
        [Display(Name = "Usuário")]
        public string UsuarioNome { get; set; }

        [Required(ErrorMessage = "O campo senha é obrigatório!")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; }

        [Display(Name = "Lembre de mim?")]
        public bool Lembrar { get; set; }
    }
}
