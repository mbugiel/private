namespace ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Table_Model
{
    public class Lease_To_Sale_Protocol_Model
    {
        public long id { get; set; }
        public string full_number { get; set; }
        
        public DateTime timestamp { get; set; }

        public long order_id { get; set; }

        public string element { get; set; }
        public string transport { get; set; }
        public string comment { get; set; }

        public decimal total_weight_kg { get; set; }
        public decimal total_area_m2 { get; set; }
        public decimal total_worth { get; set; }

        public long return_lease_protocol_id { get; set; }

        public List<Lease_Item_On_lease_To_Sale_Protocol_Model> lease_item_on_lease_to_sale_protocol_list_FK { get; set; }
    }
}
