namespace ManagemateAPI.Management.M_Order.Table_Model
{
    public class Order_Error_Model
    {
        public string code { get; set; }
        public string object_number { get; set; }
        public DateTime timestamp { get; set; }
        public decimal total_quantity { get; set; }
    }
}
