using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_item_in_storage")]
    public class Lease_Item_In_Storage
    {
        [Key]
        public long id { get; set; }

        public Storage storage_FK { get; set; }
        public long storage_FKid { get; set; }

        public Lease_Item lease_item_FK { get; set; }
        public long lease_item_FKid { get; set; }

        public List<Lease_Item_In_Storage_Stock_History> lease_item_in_storage_stock_history_FK { get; set; }

        public List<Lease_Item_On_Protocol> lease_item_on_protocol_list_FK { get; set; }
        public List<Lease_Item_Out_Of_Storage> lease_item_out_of_storage_list_FK { get; set; }
        public List<Lease_Item_On_Lease_To_Sale_Protocol> lease_item_on_lease_to_sale_protocol_list_FK { get; set; }
    }
}
