﻿namespace ManagemateAPI.Management.M_Sale_Item_In_Storage.Table_Model
{
    public class Sale_Item_In_Storage_Error_Model
    {
        public string code { get; set; }
        public long sale_item_id { get; set; }
        public long? sale_item_in_storage_id { get; set; }
        public DateTime timestamp { get; set; }
        public decimal required_quantity { get; set; }
    }
}
