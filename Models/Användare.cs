namespace MormorsBageri.Models
{
    using MormorsBageri.Enums;

    public class Användare
    {
        public int AnvändareId { get; set; }
        public string? Användarnamn { get; set; }
        public string? LösenordHash { get; set; }
        public Roller Roll { get; set; }
        public string? Email { get; set; }
        public bool Låst { get; set; }
    }
}