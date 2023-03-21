namespace Crosscutting
{
    public class DiscordModelDto
    {
        public string Channel { get; set; } = null!;
        public string Command { get; set; } = null!;
        public List<string>? Roles { get; set; }
        public string DiscordUsername { get; set; } = null!;
        public string DiscordId { get; set; } = null!;
        public string Hwid { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
