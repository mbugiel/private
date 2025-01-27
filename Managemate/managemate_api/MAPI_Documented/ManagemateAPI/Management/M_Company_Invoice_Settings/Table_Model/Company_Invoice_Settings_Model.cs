using ManagemateAPI.Management.M_Currency.Table_Model;

namespace ManagemateAPI.Management.M_Company_Invoice_Settings.Table_Model
{
    public class Company_Invoice_Settings_Model
    {
        public Currency_Model currency_id_FK { get; set; }
        public int decimal_digits { get; set; }
        public bool invoice_type_division { get; set; }
        public string sale_invoice_prefix { get; set; }
        public string lease_invoice_prefix { get; set; }

        public string lease_release_protocol_prefix { get; set; }
        public string sale_release_protocol_prefix { get; set; }
        public string return_protocol_prefix { get; set; }
    }
}
