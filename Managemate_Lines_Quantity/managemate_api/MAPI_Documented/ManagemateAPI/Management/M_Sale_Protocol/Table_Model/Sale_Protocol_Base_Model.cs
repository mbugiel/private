using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Sale_Protocol.Table_Model
{
    public class Sale_Protocol_Base_Model
    {
        public long id { get; set; }
        public string full_number { get; set; }

        public Protocol_State state { get; set; }
        public DateTime timestamp { get; set; }

        public string element { get; set; }
        public string transport { get; set; }
        public string comment { get; set; }

        public decimal total_weight_kg { get; set; }
        public decimal total_area_m2 { get; set; }
        public decimal total_worth { get; set; }
    }
}
