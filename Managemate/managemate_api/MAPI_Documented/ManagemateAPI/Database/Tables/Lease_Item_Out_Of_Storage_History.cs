using System.ComponentModel.DataAnnotations;

namespace ManagemateAPI.Database.Tables
{
    public class Lease_Item_Out_Of_Storage_History
    {
        [Key]
        public long id { get; set; }

        public decimal total_quantity { get; set; }

        public DateTime timestamp { get; set; }

        public Lease_Item_Out_Of_Storage lease_item_out_of_storage_FK { get; set; }
        public long lease_item_out_of_storage_FKid { get; set; }
    }
}
