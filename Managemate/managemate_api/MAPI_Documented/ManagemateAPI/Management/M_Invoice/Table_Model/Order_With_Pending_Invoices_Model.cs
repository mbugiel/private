namespace ManagemateAPI.Management.M_Invoice.Table_Model
{
    public class Order_With_Pending_Invoices_Model
    {
        public long order_id { get; set; }
        public string number { get; set; }
        public string name { get; set; }

        public long client_id { get; set; }
        public string client_number { get; set; }
        public string client_info { get; set; }
        public bool client_is_private_person { get; set; }
    }
}
