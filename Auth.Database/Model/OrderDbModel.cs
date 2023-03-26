using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Database.Model
{
    public class OrderDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string OrderId { get; set; }
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public virtual UserDbModel User { get; set; }
        public string ProductName { get; set; } //ProductName
        public string UniqId { get; set; }
        public string ProductPrice { get; set; }
        [Required]
        public DateTime PurchaseDate { get; set; }
        //
        [ConcurrencyCheck]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[] RowVersion { get; set; }
    }
}
