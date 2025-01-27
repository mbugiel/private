namespace ManagemateAPI.Management.M_Invoice.Table_Model
{
    public class Order_Pending_Invoice_List_Model
    {
        public long order_id { get; set; }
        public string number { get; set; }
        public string name { get; set; }
        public long client_id { get; set; }
        public string client_number { get; set; }
        public string client_info { get; set; }
        public List<Pending_Invoice_Model> pending_invoice_list { get; set; }
    }
}
