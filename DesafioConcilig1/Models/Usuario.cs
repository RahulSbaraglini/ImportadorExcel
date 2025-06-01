using System.ComponentModel.DataAnnotations;

namespace DesafioConcilig1.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string NomeUsuario { get; set; }

        [Required]
        public string Senha { get; set; }
    }
}
