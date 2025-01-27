namespace ManagemateAPI.Management.M_Lease_Item_On_Protocol.Table_Model
{
    public class Lease_Item_On_Protocol_Release_From_Storage_Model
    {
        public string storage_number { get; set; }
        public string storage_name { get; set; }
        public decimal total_quantity { get; set; }
        public string counting_unit { get; set; }

        public long lease_item_in_storage_id { get; set; }
        public long storage_id { get; set; }
    }
}
