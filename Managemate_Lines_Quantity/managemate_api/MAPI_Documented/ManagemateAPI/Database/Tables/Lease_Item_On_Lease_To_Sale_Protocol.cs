using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_item_on_lease_to_sale_protocol")]
    public class Lease_Item_On_Lease_To_Sale_Protocol
    {
        [Key]
        public long id { get; set; }

        public Lease_To_Sale_Protocol lease_to_sale_protocol_FK { get; set; }
        public long lease_to_sale_protocol_FKid { get; set; }

        public Lease_Item_In_Storage lease_item_in_storage_FK { get; set; }
        public long lease_item_in_storage_FKid { get; set; }

        public decimal total_quantity { get; set; }
        public decimal total_weight_kg { get; set; }
        public decimal total_area_m2 { get; set; }
        public decimal total_worth { get; set; }

        public byte[] comment { get; set; }
    }
}
