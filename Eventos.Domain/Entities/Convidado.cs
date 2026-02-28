using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventos.Domain.Entities
{
    public class Convidado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "O nome do convidado é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A confirmação de presença é obrigatória")]
        public bool PresencaConfirmada { get; set; }

        [Required(ErrorMessage = "O tipo de participação é obrigatório")]
        [StringLength(50)]
        public string Participacao { get; set; }

        [Range(0, 5, ErrorMessage = "A quantidade de acompanhantes não pode ser negativa ou superior a 5")]
        public int QuantidadeAcompanhantes { get; set; }

        public IList<Acompanhante> Acompanhantes { get; set; } = new List<Acompanhante>();
    }
}
