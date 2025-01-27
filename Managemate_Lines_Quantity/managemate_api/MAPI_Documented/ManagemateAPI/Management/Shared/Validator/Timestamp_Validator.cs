using ManagemateAPI.Database.Tables;

namespace ManagemateAPI.Management.Shared.Validator
{
    public class Timestamp_Validator
    {
        /**
         * Checks if timestamp in input object is equal to or before latest timestamp in stock history
         * If true -> it throws an Exception
         */
        public static void Validate_Input_Timestamp(DateTime latest_stock_state_timestamp, DateTime input_timestamp)
        {
            if (input_timestamp <= latest_stock_state_timestamp)
            {
                throw new Exception("41");// specified date is earlier than latest stock state date
            }
        }

        /**
         * Checks if timestamp given by user is later than now (timestamp)
         * If is "in the future" -> throws an Exception
         */
        public static void Validate_New_Timestamp(DateTime new_timestamp, DateTime now)
        {
            if(new_timestamp > now)
            {
                throw new Exception("46");//new timestamp is later than now (is in the future)
            }
        }

        /**
         * Checks if protocol timestamp is present in Lease_Item_Stock_History
         * If true -> throws an Exception
         */
        public static void Validate_Protocol_Timestamp(List<Lease_Item_Stock_History> stock_history, DateTime protocol_timestamp)
        {
            if(stock_history.Select(sh => sh.timestamp).Contains(protocol_timestamp))
            {
                throw new Exception("42");//timestamp duplicate in history
            }
        }

        /**
         * Checks if protocol timestamp is present in Lease_Item_In_Storage_Stock_History
         * If true -> throws an Exception
         */
        public static void Validate_Protocol_Timestamp(List<Lease_Item_In_Storage_Stock_History> stock_history, DateTime protocol_timestamp)
        {
            if (stock_history.Select(sh => sh.timestamp).Contains(protocol_timestamp))
            {
                throw new Exception("42");//timestamp duplicate in history
            }
        }

        /**
         * Checks if protocol timestamp is present in Lease_Item_Out_Of_Storage_History
         * If true -> throws an Exception
         */
        public static void Validate_Protocol_Timestamp(List<Lease_Item_Out_Of_Storage_History> out_of_storage_history, DateTime protocol_timestamp)
        {
            if (out_of_storage_history.Select(oh => oh.timestamp).Contains(protocol_timestamp))
            {
                throw new Exception("42");//timestamp duplicate in history
            }
        }



        /**
         * Checks if protocol timestamp is present in Sale_Item_Stock_History
         * If true -> throws an Exception
         */
        public static void Validate_Protocol_Timestamp(List<Sale_Item_Stock_History> stock_history, DateTime protocol_timestamp)
        {
            if (stock_history.Select(sh => sh.timestamp).Contains(protocol_timestamp))
            {
                throw new Exception("42");//timestamp duplicate in history
            }
        }

        /**
         * Checks if protocol timestamp is present in Sale_Item_In_Storage_Stock_History
         * If true -> throws an Exception
         */
        public static void Validate_Protocol_Timestamp(List<Sale_Item_In_Storage_Stock_History> stock_history, DateTime protocol_timestamp)
        {
            if (stock_history.Select(sh => sh.timestamp).Contains(protocol_timestamp))
            {
                throw new Exception("42");//timestamp duplicate in history
            }
        }

    }
}
