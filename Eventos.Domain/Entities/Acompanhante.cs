using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventos.Domain.Entities
{
    public class Acompanhante
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "O nome do acompanhante é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        public string Nome { get; set; }

        [ForeignKey("Convidado")]
        public long ConvidadoId { get; set; }

        public Convidado Convidado { get; set; }
    }
}
