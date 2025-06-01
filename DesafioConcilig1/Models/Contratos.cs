using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesafioConcilig1.Models
{
    public class Contratos
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public DateTime DataImportacao { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }



    }
}
