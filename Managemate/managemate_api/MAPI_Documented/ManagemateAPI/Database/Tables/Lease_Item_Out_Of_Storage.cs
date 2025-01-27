using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_item_out_of_storage")]
    public class Lease_Item_Out_Of_Storage
    {
        [Key]
        public long id { get; set; }

        public long lease_item_in_storage_FKid { get; set; }
        public Lease_Item_In_Storage lease_item_in_storage_FK { get; set; }

        public long order_FKid { get; set; }
        public Order order_FK { get; set; }

        public List<Lease_Item_Out_Of_Storage_History> lease_item_out_of_storage_history_FK { get; set; }
    }
}
