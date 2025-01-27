using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("sale_item_stock_history")]
    public class Sale_Item_Stock_History
    {
        [Key]
        public long id { get; set; }

        public decimal total_quantity { get; set; }
        public decimal in_storage_quantity { get; set; }
        public decimal blocked_quantity { get; set; }

        public DateTime timestamp { get; set; }

        public Sale_Item sale_item_FK { get; set; }
        public long sale_item_FKid { get; set; }
    }
}
