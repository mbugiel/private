namespace ManagemateAPI.Management.M_Sale_Item_In_Storage.Table_Model
{
    public class Sale_Item_In_Storage_Model
    {
        public long id { get; set; }

        public string sale_item_number { get; set; }
        public string sale_item_name { get; set; }

        public string counting_unit { get; set; }

        public string storage_number { get; set; }
        public string storage_name { get; set; }

        public decimal in_storage_quantity { get; set; }
        public decimal blocked_quantity { get; set; }
    }
}
