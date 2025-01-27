namespace ManagemateAPI.Management.M_Client.Input_Objects
{
    public class Edit_Client_Data
    {
        public long id { get; set; }
        public string number { get; set; }
        public string surname { get; set; }
        public string name { get; set; }

        public string company_name { get; set; }
        public string nip { get; set; }

        public string phone_number { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string comment { get; set; }
    }
}
