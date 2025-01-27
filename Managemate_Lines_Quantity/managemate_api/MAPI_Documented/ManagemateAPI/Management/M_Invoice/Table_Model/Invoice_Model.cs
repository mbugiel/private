using ManagemateAPI.Management.M_Order.Table_Model;
using ManagemateAPI.Management.Shared.Enum;

namespace ManagemateAPI.Management.M_Invoice.Table_Model
{
    public class Invoice_Model
    {
        public long id { get; set; }

        public Invoice_Type invoice_type { get; set; }

        public int year { get; set; }
        public int month { get; set; }
        public string full_number { get; set; }

        public long order_id { get; set; }
        public Order_Model_List order { get; set; }

        //public long? invoice_printed_data_FKid { get; set; }
        //public Invoice_Printed_Data? invoice_printed_data_FK { get; set; }

        public DateTime issue_date { get; set; }
        public DateTime sale_date { get; set; }
        public DateTime payment_date { get; set; }

        public string payment_method { get; set; }

        public decimal net_worth { get; set; }
        public decimal tax_worth { get; set; }
        public decimal gross_worth { get; set; }

        public string gross_worth_in_words { get; set; }
        public string comment { get; set; }


        public List<Invoice_Row_Model> invoice_row_list { get; set; }
    }
}
