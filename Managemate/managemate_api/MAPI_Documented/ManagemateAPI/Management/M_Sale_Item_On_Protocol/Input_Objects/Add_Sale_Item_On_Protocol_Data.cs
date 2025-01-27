namespace ManagemateAPI.Management.M_Sale_Item_On_Protocol.Input_Objects
{
    public class Add_Sale_Item_On_Protocol_Data
    {
        public long protocol_FK { get; set; }
        public long sale_item_in_storage_FK { get; set; }
        public decimal total_quantity { get; set; }
    }
}
