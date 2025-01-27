namespace ManagemateAPI.Management.M_Invoice.Input_Objects
{
    public class Edit_Invoice_Row_Data
    {
        public long id { get; set; }

        public string name { get; set; }
        public string counting_unit { get; set; }

        public bool use_discount { get; set; }
        public bool discount_is_in_pct { get; set; }
        public decimal discount_value { get; set; }
    }
}
