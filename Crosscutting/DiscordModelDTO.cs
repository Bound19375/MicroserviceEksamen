namespace Crosscutting
{
    public class DiscordModelDTO
    {
        public string Channel { get; set; }
        public string Command { get; set; }
        public List<string>? Roles { get; set; }
        public string DiscordUsername { get; set; }
        public string DiscordId { get; set; }
        public string HWID { get; set; }
    }
}
