namespace ManagemateAPI.Management.M_Invoice.Table_Model
{
    public class Sale_Value_Model
    {
        public string sale_object_name { get; set; }


        public decimal total_quantity { get; set; }
        public string counting_unit { get; set; }

        public decimal tax_pct { get; set; }

        public decimal net_worth { get; set; }

        public decimal gross_worth { get; set; }

        public decimal tax_worth { get; set; }
    }
}
