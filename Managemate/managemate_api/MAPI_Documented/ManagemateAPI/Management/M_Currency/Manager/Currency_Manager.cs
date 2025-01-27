using ManagemateAPI.Database.Context;
using ManagemateAPI.Management.M_Currency.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;

namespace ManagemateAPI.Management.M_Currency.Manager
{
    /*
     * This is an endpoint controller dedicated to the Currency table.
     * 
     * It contains methods for endpoints
     * - Get all 
     */
    public class Currency_Manager
    {

        private DB_Context _context;
        private readonly IConfiguration _configuration;


        public Currency_Manager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /*
         * Get_All_Item_Trading_Type endpoint
         * This endpoint is used to to all the records from the Item_Trading_Type table.
         * 
         * It accepts Get_All_Item_Trading_Type_Data object.
         * The given object is handed over to the Get_All_Item_Trading_Type method in the Item_Trading_Type_Manager.
         */
        public List<Currency_Model> Get_All_Currency(Session_Data session)
        {
            if (session == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {
                _context = new DB_Context(session.userId, _configuration);
                _context.Database.EnsureCreated();

                var currency_list = _context.Currency.ToList();

                if (currency_list == null)
                {
                    throw new Exception("19");// currencies not found
                }
                else
                {
                    List<Currency_Model> currency_list_model = new List<Currency_Model>();

                    foreach (var currency in currency_list)
                    {
                        currency_list_model.Add(new Currency_Model { id = currency.id, currency_symbol = currency.currency_symbol, currency_hundreth_symbol = currency.currency_hundreth_symbol });
                    }

                    return currency_list_model;

                }


            }

        }


    }
}
