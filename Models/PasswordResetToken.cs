using System;

namespace MormorsBageri.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}