// Användare.cs (new file, inferred from earlier)
namespace MormorsBageri.Models
{
    using System.ComponentModel.DataAnnotations;
    using MormorsBageri.Enums;

    public class Användare
    {
        public int AnvändareId { get; set; }

        [Required(ErrorMessage = "Användarnamn är obligatoriskt")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Användarnamn måste vara mellan 3 och 255 tecken")]
        public string? Användarnamn { get; set; }

        [Required(ErrorMessage = "Lösenord är obligatoriskt")]
        public string? LösenordHash { get; set; }

        [Required(ErrorMessage = "Roll är obligatorisk")]
        public Roller Roll { get; set; }

        [EmailAddress(ErrorMessage = "Ogiltig email-adress")]
        public string? Email { get; set; }

        public bool Låst { get; set; }
    }
}

