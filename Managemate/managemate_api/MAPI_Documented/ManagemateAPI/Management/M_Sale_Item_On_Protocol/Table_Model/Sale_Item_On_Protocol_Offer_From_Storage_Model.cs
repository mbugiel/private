namespace ManagemateAPI.Management.M_Sale_Item_On_Protocol.Table_Model
{
    public class Sale_Item_On_Protocol_Offer_From_Storage_Model
    {
        public string storage_number { get; set; }
        public string storage_name { get; set; }
        public string counting_unit { get; set; }

        public long sale_item_in_storage_id { get; set; }
        public long storage_id { get; set; }
    }
}
