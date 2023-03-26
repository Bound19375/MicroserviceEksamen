using Crosscutting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Database.Model
{
    public class ActiveLicensesDbModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ProductName { get; set; } //ProductName
        public WhichSpec ProductNameEnum { get; set; } //ProductNameEnum

        public DateTime EndDate { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public virtual UserDbModel User { get; set; }
        [ForeignKey("OrderId")]
        public string OrderId { get; set; }
        public virtual OrderDbModel Order { get; set; }
        [ConcurrencyCheck]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[] RowVersion { get; set; }
    }
}
