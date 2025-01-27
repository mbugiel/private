using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Order.Table_Model
{
    public class Order_Model_List
    {
        public long id { get; set; }
        public string order_name { get; set; }
        public string order_number { get; set; }

        public long client_id { get; set; }
        public string client_number { get; set; }
        public bool client_is_private_person { get; set; }
        public string client_name { get; set; }
        public string client_surname { get; set; }
        public string client_company_name { get; set; }

        public long construction_site_id { get; set; }
        public string construction_site_number { get; set; }
        public string construction_site_name { get; set; }

        public Order_State state { get; set; }
        public DateTime timestamp { get; set; }

        public string comment { get; set; }

        public string default_payment_method { get; set; }
        public int default_payment_date_offset { get; set; }
        public decimal default_discount { get; set; }

        public bool use_static_rate { get; set; }
        public decimal static_rate { get; set; }
    }

}
