using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Table_Model;

namespace ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Table_Model
{
    public class Lease_Protocol_Id_Model_After_Remove_Data
    {
        public long protocol_id { get; set; }
        public List<Lease_Item_On_Protocol_Error_Model> error_list { get; set; }
    }
}
