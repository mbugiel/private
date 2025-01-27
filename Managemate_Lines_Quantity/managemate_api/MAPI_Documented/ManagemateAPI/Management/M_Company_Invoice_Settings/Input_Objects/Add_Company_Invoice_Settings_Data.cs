using ManagemateAPI.Management.M_Session.Input_Objects;

namespace ManagemateAPI.Management.M_Company_Invoice_Settings.Input_Objects
{
    public class Add_Company_Invoice_Settings_Data
    {
        public long currency_id_FK { get; set; }
        public int decimal_digits { get; set; }
        public bool invoice_type_division { get; set; }
        public string sale_invoice_prefix { get; set; }
        public string lease_invoice_prefix { get; set; }

        public string lease_release_protocol_prefix { get; set; }
        public string sale_release_protocol_prefix { get; set; }
        public string return_protocol_prefix { get; set; }
    }
}
