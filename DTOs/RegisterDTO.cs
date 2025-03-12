namespace MormorsBageri.DTOs
{
    using MormorsBageri.Enums;

    public class RegisterDTO
    {
        public string? Användarnamn { get; set; }
        public string? Lösenord { get; set; }
        public Roller Roll { get; set; }
        public string? Email { get; set; }
        public bool Låst { get; set; }
    }
}