using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Invoice.Input_Objects
{
    public class Create_Invoice_Data
    {
        public Invoice_Type invoice_type { get; set; }

        public int year { get; set; }
        public int month { get; set; }

        public long order_id { get; set; }

        public DateTime issue_date { get; set; }
        public DateTime sale_date { get; set; }
        public DateTime payment_date { get; set; }

        public string payment_method { get; set; }
        
        public bool use_discount { get; set; }
        public bool discount_is_in_pct { get; set; }
        public decimal discount_value { get; set; }
        
        public string comment { get; set; }

        public string language_code { get; set; }

        public bool overwrite { get; set; }
    }
}
