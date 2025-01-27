namespace ManagemateAPI.Management.M_Lease_Item_On_Protocol.Table_Model
{
    public class Lease_Item_On_Protocol_Return_Available_Model
    {
        public string catalog_number { get; set; }
        public string product_name { get; set; }
        public string counting_unit { get; set; }
        public decimal total_quantity { get; set; }
        public decimal total_weight_kg { get; set; }

        public long lease_item_id { get; set; }
        public List<Lease_Item_On_Protocol_Return_From_Storage_Model> from_storage_list { get; set; }
    }
}
