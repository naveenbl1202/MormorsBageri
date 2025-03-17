namespace MormorsBageri.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Beställningsdetalj
    {
        public int BeställningsdetaljId { get; set; }
        [Required] public int BeställningId { get; set; }
        [Required] public int ProduktId { get; set; }
        [Range(1, int.MaxValue)] public int Antal { get; set; }
        [Range(0, double.MaxValue)] public decimal Styckpris { get; set; }
        [Range(0, double.MaxValue)] public decimal Rabatt { get; set; }
        public Produkt? Produkt { get; set; } // Nullable to avoid CS8618 warning
    }
}