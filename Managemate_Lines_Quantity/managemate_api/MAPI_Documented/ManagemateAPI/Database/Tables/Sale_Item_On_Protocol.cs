using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("sale_item_on_protocol")]
    public class Sale_Item_On_Protocol
    {
        [Key]
        public long id { get; set; }

        public Sale_Protocol sale_protocol_FK { get; set; }
        public long sale_protocol_FKid { get; set; }

        public Sale_Item_In_Storage sale_item_in_storage_FK { get; set; }
        public long sale_item_in_storage_FKid { get; set; }

        public decimal total_quantity { get; set; }
        public decimal total_weight_kg { get; set; }
        public decimal total_area_m2 { get; set; }
        public decimal total_worth { get; set; }

        public byte[] comment { get; set; }
    }
}
