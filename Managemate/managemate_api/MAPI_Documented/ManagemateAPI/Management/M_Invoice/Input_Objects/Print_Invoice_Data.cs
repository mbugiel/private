namespace ManagemateAPI.Management.M_Invoice.Input_Objects
{
    public class Print_Invoice_Data
    {
        public long invoice_id { get; set; }
        public bool print_new { get; set; }
        public DateTime user_current_timestamp { get; set; }
        public bool auto_sign { get; set; }
    }
}
