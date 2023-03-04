namespace Crosscutting
{
    public class AuthModelDTO
    {
        public string? Email { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? DiscordUsername { get; set; }
        public string? DiscordId { get; set; }
        public string? HWID { get; set; }
        //ActiveLicenses
        public string ProductName { get; set; } //ProductName
        public DateTime EndDate { get; set; }
        public string? UserId { get; set; }
        public WhichSpec ProductNameEnum { get; set; } //ProductNameEnum
    }
}
