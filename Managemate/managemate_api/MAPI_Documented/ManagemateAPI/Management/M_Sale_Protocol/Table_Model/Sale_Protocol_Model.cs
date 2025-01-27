using ManagemateAPI.Management.M_Sale_Item_On_Protocol.Table_Model;
using ManagemateAPI.Management.M_Service_On_Sale_Protocol.Table_Model;
using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Sale_Protocol.Table_Model
{
    public class Sale_Protocol_Model
    {
        public long id { get; set; }
        public string full_number { get; set; }

        public Protocol_State state { get; set; }
        public DateTime timestamp { get; set; }

        public long order_id { get; set; }

        public string element { get; set; }
        public string transport { get; set; }
        public string comment { get; set; }

        public decimal total_weight_kg { get; set; }
        public decimal total_area_m2 { get; set; }
        public decimal total_worth { get; set; }
        public List<Sale_Item_On_Protocol_Model> sale_item_on_protocol_list_FK { get; set; }
        public List<Service_On_Sale_Protocol_Model> service_on_protocol_list_FK { get; set; }
    }
}
