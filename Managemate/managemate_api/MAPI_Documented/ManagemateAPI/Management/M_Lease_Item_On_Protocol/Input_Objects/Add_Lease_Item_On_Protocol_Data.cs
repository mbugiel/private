namespace ManagemateAPI.Management.M_Lease_Item_On_Protocol.Input_Objects
{
    public class Add_Lease_Item_On_Protocol_Data
    {
        public long protocol_FK { get; set; }
        public long lease_item_in_storage_FK { get; set; }
        public decimal total_quantity { get; set; }
    }
}
