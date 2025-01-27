using ManagemateAPI.Management.M_Client.Table_Model;
using ManagemateAPI.Management.M_Construction_Site.Table_Model;
using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Order.Table_Model
{
    public class Order_Model
    {
        public long id { get; set; }
        public string order_name { get; set; }
        public string order_number { get; set; }

        public Client_Model client_FK { get; set; }

        public Construction_Site_Model construction_site_FK { get; set; }

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
