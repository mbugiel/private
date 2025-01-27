using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManagemateAPI.Database.Tables
{
    [Table("company_invoice_settings")]
    public class Company_Invoice_Settings
    {
        [Key]
        public long id { get; set; }

        public Currency currency_FK { get; set; }
        public long currency_FKid { get; set; }

        public int decimal_digits { get; set; }
        public bool invoice_type_division { get; set; }
        public string sale_invoice_prefix { get; set; }
        public string lease_invoice_prefix { get; set; }

        public string lease_release_protocol_prefix { get; set; }
        public string sale_release_protocol_prefix { get; set; }
        public string return_protocol_prefix { get; set; }
    }
}
