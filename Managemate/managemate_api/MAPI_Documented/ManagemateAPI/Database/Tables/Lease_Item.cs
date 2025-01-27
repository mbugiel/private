using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("lease_item")]
    public class Lease_Item
    {
        [Key]
        public long id { get; set; }
        public string catalog_number { get; set; }
        public string product_name { get; set; }

        public Lease_Item_Group lease_group_FK { get; set; }
        public long lease_group_FKid { get; set; }
        
        public List<Lease_Item_Stock_History> lease_item_stock_history_FK { get; set; }

        public decimal weight_kg { get; set; }
        public decimal size_cm_x { get; set; }
        public decimal size_cm_y { get; set; }
        public decimal area_m2 { get; set; } 

        public decimal price { get; set; }
        public byte[] comment { get; set; }
        
        public Counting_Unit counting_unit_FK { get; set; }
        public long counting_unit_FKid { get; set; }

        public List<Lease_Item_In_Storage> lease_item_in_storage_list_FK { get; set; }

        public bool deleted { get; set; }
    }
}
