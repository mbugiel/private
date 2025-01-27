namespace ManagemateAPI.Management.M_Sale_Item_On_Protocol.Table_Model
{
    public class Sale_Item_On_Protocol_Model
    {
        public long id { get; set; }
        public string catalog_number { get; set; }
        public string product_name { get; set; }
        public string counting_unit { get; set; }
        public long sale_item_id { get; set; }
        public long sale_item_in_storage_id { get; set; }
        public decimal total_quantity { get; set; }
        public decimal weight_kg { get; set; }
        public decimal total_weight_kg { get; set; }
        public decimal total_area_m2 { get; set; }
        public decimal total_worth { get; set; }
        public string comment { get; set; }
    }
}
