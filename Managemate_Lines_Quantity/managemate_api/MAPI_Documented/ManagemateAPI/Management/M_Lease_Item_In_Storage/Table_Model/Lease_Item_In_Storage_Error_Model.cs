namespace ManagemateAPI.Management.M_Lease_Item_In_Storage.Table_Model
{
    public class Lease_Item_In_Storage_Error_Model
    {
        public string code { get; set; }
        public long lease_item_id { get; set; }
        public long? lease_item_in_storage_id { get; set; }
        public DateTime timestamp { get; set; }
        public decimal required_quantity { get; set; }
    }
}
