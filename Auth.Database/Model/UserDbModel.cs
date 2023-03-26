using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Database.Model
{
    [Index(nameof(DiscordId), IsUnique = true)]
    [Index(nameof(DiscordUsername), IsUnique = true)]
    public class UserDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string UserId { get; set; }

        public string Email { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        [Required]
        [RegularExpression("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{4,}$", ErrorMessage = "Discord Name Isn't In Correct Format!")]
        public string DiscordUsername { get; set; }

        [Required]
        [RegularExpression("^(?=.*?[0-9]).{18,}$", ErrorMessage = "Discord Id Isn't Isn't Correct!")]
        public string DiscordId { get; set; }

        [Required]
        [RegularExpression("^(?=.*?[-]).{39,}$")]
        public string HWID { get; set; }

        //
        [ConcurrencyCheck]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[] RowVersion { get; set; }
    }
}
