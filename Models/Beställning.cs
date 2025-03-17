namespace MormorsBageri.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Beställning
    {
        public int BeställningId { get; set; }
        [Required] public int ButikId { get; set; }
        [Required] public DateTime Beställningsdatum { get; set; }
        [StringLength(100)] public string? Beställare { get; set; }
        [StringLength(255)] public string? Säljare { get; set; }
        [Required] public DateTime PreliminärtLeveransdatum { get; set; }
        public List<Beställningsdetalj> Beställningsdetaljer { get; set; } = new List<Beställningsdetalj>();
        public Butik? Butik { get; set; } // Nullable to avoid CS8618 warning
    }
}