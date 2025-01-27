using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_item_stock_history")]
    public class Lease_Item_Stock_History
    {
        [Key]
        public long id { get; set; }

        public decimal total_quantity { get; set; }
        public decimal in_storage_quantity { get; set; }
        public decimal out_of_storage_quantity { get; set; }
        public decimal blocked_quantity { get; set; }

        public DateTime timestamp { get; set; }

        public Lease_Item lease_item_FK { get; set; }
        public long lease_item_FKid { get; set; }
    }
}
