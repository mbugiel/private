namespace ManagemateAPI.Management.M_Lease_Item_In_Storage.Input_Objects
{
    public class Add_Lease_Item_In_Storage_Data
    {
        public long storage_FK { get; set; }
        public long lease_item_FK { get; set; }
        public decimal total_quantity { get; set; }
        public DateTime timestamp { get; set; }
    }
}
