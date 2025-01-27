using System.ComponentModel.DataAnnotations;

namespace ManagemateAPI.Database.Tables
{
    public class Sale_Item_In_Storage_Stock_History
    {
        [Key]
        public long id { get; set; }

        public decimal in_storage_quantity { get; set; }
        public decimal blocked_quantity { get; set; }

        public DateTime timestamp { get; set; }

        public Sale_Item_In_Storage sale_item_in_storage_FK { get; set; }
        public long sale_item_in_storage_FKid { get; set; }
    }
}
