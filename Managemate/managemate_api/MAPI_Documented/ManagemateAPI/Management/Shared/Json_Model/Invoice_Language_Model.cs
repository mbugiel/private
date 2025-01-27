namespace ManagemateAPI.Management.Shared.Json_Model
{
    public class Invoice_Language_Model
    {
        public string invoice { set; get; }
        public string issue_date { set; get; }
        public string sale_date { set; get; }
        public string payment_date { set; get; }
        public string payment_method { set; get; }
        public string seller { set; get; }
        public string client { set; get; }
        public string number { set; get; }
        public string table_head_item { set; get; }
        public string quantity { set; get; }
        public string discount { set; get; }
        public string net_price { set; get; }
        public string net_price_after_discount { set; get; }
        public string net_value { set; get; }
        public string vat_percent { set; get; }
        public string vat_value { set; get; }
        public string gross_value { set; get; }
        public string including { set; get; }
        public string total { set; get; }
        public string comments { set; get; }
        public string to_pay { set; get; }
        public string name_and_surname_of_invoice_recipient { set; get; }
        public string name_and_surname_of_invoice_issuer { set; get; }
        public string name { set; get; }
        public string surname { set; get; }
        public string lease_measurement_unit { set; get; }

        public string lease_for_month { set; get; }
    }
}
