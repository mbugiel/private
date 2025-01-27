namespace ManagemateAPI.Management.M_Sale_Item.Input_Objects
{
    public class Edit_Sale_Item_Data
    {
        public long id { get; set; }
        public string catalog_number { get; set; }
        public string product_name { get; set; }
        public long sale_group_FK { get; set; }
        public decimal weight_kg { get; set; }
        public decimal size_cm_x { get; set; }
        public decimal size_cm_y { get; set; }
        public decimal total_quantity { get; set; }
        public decimal price { get; set; }
        public long counting_unit_FK { get; set; }
        public string comment { get; set; }
        public DateTime timestamp { get; set; }
    }
}
