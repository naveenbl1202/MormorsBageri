namespace MormorsBageri.Models
{
    public class Beställningsdetalj
    {
        public int BeställningsdetaljId { get; set; }
        public int BeställningId { get; set; }
        public int ProduktId { get; set; }
        public int Antal { get; set; }
        public decimal Styckpris { get; set; }
        public decimal Rabatt { get; set; }
    }
}