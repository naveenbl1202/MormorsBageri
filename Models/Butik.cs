namespace MormorsBageri.Models
{
    public class Butik
    {
        public int ButikId { get; set; }
        public string? ButikNummer { get; set; }
        public string? ButikNamn { get; set; }
        public string? Telefonnummer { get; set; }
        public string? Besöksadress { get; set; }
        public string? Fakturaadress { get; set; }
        public string? ButikschefNamn { get; set; }
        public string? ButikschefTelefon { get; set; }
        public string? BrödansvarigNamn { get; set; }
        public string? BrödansvarigTelefon { get; set; }
        public bool? Låst { get; set; } // Added Låst as nullable boolean
    }
}