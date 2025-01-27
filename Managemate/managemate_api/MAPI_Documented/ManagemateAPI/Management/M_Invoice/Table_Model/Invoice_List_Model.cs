using ManagemateAPI.Management.M_Order.Table_Model;
using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Invoice.Table_Model
{
    public class Invoice_List_Model
    {
        public long id { get; set; }

        public Invoice_Type invoice_type { get; set; }

        public int year { get; set; }
        public int month { get; set; }
        public string full_number { get; set; }

        public long order_id { get; set; }

        public bool is_printed { get; set; }

        public DateTime issue_date { get; set; }
        public DateTime sale_date { get; set; }
        public DateTime payment_date { get; set; }

        public string payment_method { get; set; }

        public decimal net_worth { get; set; }
        public decimal tax_worth { get; set; }
        public decimal gross_worth { get; set; }

    }
}
