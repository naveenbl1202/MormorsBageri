
namespace MormorsBageri.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Butik
    {
        public int ButikId { get; set; }

        [StringLength(50, ErrorMessage = "ButikNummer får vara max 50 tecken")]
        public string? ButikNummer { get; set; }

        [Required(ErrorMessage = "ButikNamn är obligatoriskt")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "ButikNamn måste vara mellan 2 och 100 tecken")]
        public string? ButikNamn { get; set; }

        [Phone(ErrorMessage = "Ogiltigt telefonnummer")]
        public string? Telefonnummer { get; set; }

        [StringLength(200, ErrorMessage = "Besöksadress får vara max 200 tecken")]
        public string? Besöksadress { get; set; }

        [StringLength(200, ErrorMessage = "Fakturaadress får vara max 200 tecken")]
        public string? Fakturaadress { get; set; }

        [StringLength(100, ErrorMessage = "ButikschefNamn får vara max 100 tecken")]
        public string? ButikschefNamn { get; set; }

        [Phone(ErrorMessage = "Ogiltigt telefonnummer")]
        public string? ButikschefTelefon { get; set; }

        [StringLength(100, ErrorMessage = "BrödansvarigNamn får vara max 100 tecken")]
        public string? BrödansvarigNamn { get; set; }

        [Phone(ErrorMessage = "Ogiltigt telefonnummer")]
        public string? BrödansvarigTelefon { get; set; }

        public bool? Låst { get; set; }
    }
}

