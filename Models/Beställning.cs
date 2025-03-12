namespace MormorsBageri.Models
{
    public class Beställning
    {
        public int BeställningId { get; set; }
        public int ButikId { get; set; }
        public DateTime Beställningsdatum { get; set; }
        public string? Beställare { get; set; }
        public string? Säljare { get; set; }
        public DateTime PreliminärtLeveransdatum { get; set; }
        public List<Beställningsdetalj> Beställningsdetaljer { get; set; } = new List<Beställningsdetalj>();
    }
}