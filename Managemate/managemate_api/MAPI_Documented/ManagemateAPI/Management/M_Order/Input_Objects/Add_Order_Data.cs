namespace ManagemateAPI.Management.M_Order.Input_Objects
{
    public class Add_Order_Data
    {
        public string order_name { get; set; }
        public string order_number { get; set; }

        public long client_FK { get; set; }
        
        public long construction_site_FK { get; set; }
        
        public DateTime timestamp { get; set; }
        
        public string comment { get; set; }
        
        public string default_payment_method { get; set; }
        public int default_payment_date_offset { get; set; }
        public decimal default_discount { get; set; }

        public bool use_static_rate { get; set; }
        public decimal static_rate { get; set; }
    }
}
