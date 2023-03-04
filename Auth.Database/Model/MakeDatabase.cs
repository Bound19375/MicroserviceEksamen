using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Auth.Database.Model;

public class MakeDatabase
{
    [Key]
    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Key { get; set; }
    public string Event { get; set; }
    public string Product { get; set; }
    public string Email { get; set; }
    public string DiscordUsername { get; set; }
    public string DiscordID { get; set; }
    public string HWID { get; set; }
    public string ProductID { get; set; }
    public string OrderID { get; set; }
    public string CreatedAt { get; set; }
    public DateTime PurchaseDate { get; set; }
    //
    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public byte[] RowVersion { get; set; }
}