using ManagemateAPI.Management.M_Sale_Item_On_Protocol.Table_Model;

namespace ManagemateAPI.Management.M_Sale_Protocol.Table_Model
{
    public class Sale_Offer_Protocol_Model_Id
    {
        public long protocol_id { get; set; }
        public DateTime latest_protocol_timestamp { get; set; }
        public List<Sale_Item_On_Protocol_Error_Model> error_list { get; set; }
    }
}
