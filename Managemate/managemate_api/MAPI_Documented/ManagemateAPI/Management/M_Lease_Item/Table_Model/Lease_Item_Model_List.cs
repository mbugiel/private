namespace ManagemateAPI.Management.M_Lease_Item.Table_Model
{
    public class Lease_Item_Model_List
    {
        public long id { get; set; }
        public string catalog_number { get; set; }
        public string product_name { get; set; }
        public string lease_group { get; set; }

        public decimal total_quantity { get; set; }
        public decimal in_storage_quantity { get; set; }
        public decimal out_of_storage_quantity { get; set; }
        public decimal blocked_quantity { get; set; }

        public decimal weight_kg { get; set; }
        public decimal size_cm_x { get; set; }
        public decimal size_cm_y { get; set; }
        public decimal area_m2 { get; set; }

        public decimal price { get; set; }
        public string counting_unit { get; set; }
        public string comment { get; set; }
    }
}
