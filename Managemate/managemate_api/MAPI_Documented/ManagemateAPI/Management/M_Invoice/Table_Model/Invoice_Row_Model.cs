namespace ManagemateAPI.Management.M_Invoice.Table_Model
{
    public class Invoice_Row_Model
    {
        public long id { get; set; }

        public int row_number { get; set; }
        public string name { get; set; }

        public decimal total_quantity { get; set; }
        public string counting_unit { get; set; }

        public bool use_discount { get; set; }
        public bool discount_is_in_pct { get; set; }
        public decimal discount_value { get; set; }

        public decimal net_worth { get; set; }
        public decimal net_worth_after_discount { get; set; }
        public decimal net_worth_total { get; set; }

        public decimal tax_pct { get; set; }
        public decimal tax_worth { get; set; }

        public decimal gross_worth { get; set; }
    }
}
