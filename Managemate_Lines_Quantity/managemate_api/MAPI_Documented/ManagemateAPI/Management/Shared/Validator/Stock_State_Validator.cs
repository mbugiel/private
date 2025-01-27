using ManagemateAPI.Database.Tables;

namespace ManagemateAPI.Management.Shared.Validator
{
    public class Stock_State_Validator
    {
        public static void Validate_Stock_State(Lease_Item_Stock_History stock_state)
        {
            if (
                stock_state.in_storage_quantity + stock_state.out_of_storage_quantity > stock_state.total_quantity ||
                stock_state.blocked_quantity > stock_state.in_storage_quantity
            )
            {
                throw new Exception("36");// checksum error
            }

            if (
                stock_state.total_quantity < 0 ||
                stock_state.in_storage_quantity < 0 ||
                stock_state.out_of_storage_quantity < 0 ||
                stock_state.blocked_quantity < 0
            )
            {
                throw new Exception("39");// negative quantity
            }
        }

        public static void Validate_Stock_State(Lease_Item_In_Storage_Stock_History stock_state)
        {
            if (
                !stock_state.total_quantity.Equals(stock_state.in_storage_quantity + stock_state.out_of_storage_quantity) ||
                stock_state.blocked_quantity > stock_state.in_storage_quantity
            )
            {
                throw new Exception("36");// checksum error
            }

            if (
                stock_state.total_quantity < 0 ||
                stock_state.in_storage_quantity < 0 ||
                stock_state.out_of_storage_quantity < 0 ||
                stock_state.blocked_quantity < 0
            )
            {
                throw new Exception("39");// negative quantity
            }
        }


        public static void Validate_Stock_State(Sale_Item_Stock_History stock_state)
        {
            if (
                stock_state.in_storage_quantity > stock_state.total_quantity ||
                stock_state.blocked_quantity > stock_state.in_storage_quantity
            )
            {
                throw new Exception("36");// checksum error
            }

            if (
                stock_state.total_quantity < 0 ||
                stock_state.in_storage_quantity < 0 ||
                stock_state.blocked_quantity < 0
            )
            {
                throw new Exception("39");// negative quantity
            }
        }

        public static void Validate_Stock_State(Sale_Item_In_Storage_Stock_History stock_state)
        {
            if (
                stock_state.blocked_quantity > stock_state.in_storage_quantity
            )
            {
                throw new Exception("36");// checksum error
            }

            if (
                stock_state.in_storage_quantity < 0 ||
                stock_state.blocked_quantity < 0
            )
            {
                throw new Exception("39");// negative quantity
            }
        }



        public static void Validate_Out_Of_Storage_State(Lease_Item_Out_Of_Storage_History out_of_storage_state, decimal lease_item_in_storage_out_of_storage_quantity)
        {
            if (
                out_of_storage_state.total_quantity > lease_item_in_storage_out_of_storage_quantity
            )
            {
                throw new Exception("36");// checksum error
            }

            if (
                out_of_storage_state.total_quantity < 0
            )
            {
                throw new Exception("39");// negative quantity
            }
        }

    }
}
