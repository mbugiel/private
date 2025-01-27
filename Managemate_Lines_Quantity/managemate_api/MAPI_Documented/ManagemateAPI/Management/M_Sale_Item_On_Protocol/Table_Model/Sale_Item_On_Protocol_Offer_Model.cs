namespace ManagemateAPI.Management.M_Sale_Item_On_Protocol.Table_Model
{
    public class Sale_Item_On_Protocol_Offer_Model
    {
        public string catalog_number { get; set; }
        public string product_name { get; set; }
        public string counting_unit { get; set; }

        public long sale_item_id { get; set; }
        public List<Sale_Item_On_Protocol_Offer_From_Storage_Model> in_storage_list { get; set; }
    }
}
