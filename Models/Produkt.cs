namespace MormorsBageri.Models
{
    public class Produkt
    {
        public int ProduktId { get; set; }
        public string Namn { get; set; }
        public decimal Baspris { get; set; }
        public bool IsDeleted { get; set; } = false; // Ensure this property is present
    }
}