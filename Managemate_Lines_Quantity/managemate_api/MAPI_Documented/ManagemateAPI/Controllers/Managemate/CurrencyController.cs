using ManagemateAPI.Management.M_Currency.Manager;
using ManagemateAPI.Management.M_Currency.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{

    [ApiController]
    public class CurrencyController : ControllerBase
    {

        private Currency_Manager _DB_Helper;

        public CurrencyController(IConfiguration configuration)
        {
            _DB_Helper = new Currency_Manager(configuration);
        }

        /*
         * Get_All_Currency endpoint
         * This endpoint is used to to all the records from the Item_Trading_Type table.
         * 
         * The session object is handed over to the Get_All_Currency method in the Currency_Manager.
         */
        [Route("api/Get_All_Currency")]
        [HttpGet]
        public async Task<IActionResult> Get_All_Currency()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Currency_Model> result = _DB_Helper.Get_All_Currency(session);

                if (result == null)
                {
                    throw new Exception("14");//_14_NULL_ERROR
                }

                ResponseType responseType = ResponseType.Success;

                return Ok(Response_Handler.GetAppResponse(responseType, result));

            }
            catch (Exception e)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(e));
            }
        }

    }
}
