using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Invoice.Table_Model
{
    public class Order_With_Invoice_List_Model
    {
        public long id { get; set; }
        public string order_name { get; set; }
        public string order_number { get; set; }

        public Order_State state { get; set; }
        public DateTime timestamp { get; set; }

        public List<Invoice_List_Model> invoice_list { get; set; }
    }
}
