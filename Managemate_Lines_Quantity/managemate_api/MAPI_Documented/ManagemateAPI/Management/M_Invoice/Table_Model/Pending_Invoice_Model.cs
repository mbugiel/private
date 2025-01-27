using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Invoice.Table_Model
{
    public class Pending_Invoice_Model
    {
        public Invoice_Type invoice_type { get; set; }
        public long order_id { get; set; }
        public int year { get; set; }
        public int month { get; set; }
    }
}
