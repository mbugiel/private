using Microsoft.AspNetCore.Mvc;
using ManagemateAPI.Management.M_Company.Input_Objects;
using ManagemateAPI.Management.M_Company.Manager;
using ManagemateAPI.Management.M_Company.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;

/*
 * This is an endpoint controller dedicated to the Company table.
 * 
 * It contains methods for endpoints
 * - Add 
 * - Edit
 * - Get
 */
namespace ManagemateAPI.Controllers.Managemate
{

    [ApiController]
    public class Company_InfoController : ControllerBase
    {
        private Company_Info_Manager _DB_Helper;

        public Company_InfoController(IConfiguration configuration)
        {
            _DB_Helper = new Company_Info_Manager(configuration);
        }

        /*
         * Add_Company endpoint
         * This endpoint is used to add a record to the Company table.
         * 
         * It accepts Add_Company_Data object.
         * The given object is handed over to the Add_Company_Data method in the Company_Manager.
         */
        [Route("api/Add_Company_Info")]
        [HttpPost]
        public async Task<IActionResult> Add_Company_Info([FromBody] Add_Company_Info_Data input_obj)
        {
            if (input_obj == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {
                try
                {
                    Session_Data session = await Session_Manager.Active_Session(Request);

                    string result = await _DB_Helper.Add_Company_Info(input_obj, session);

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

        /*
         * Edit_Company endpoint
         * This endpoint is used to edit a record from the Company table.
         * 
         * It accepts Edit_Company_Data object.
         * The given object is handed over to the Edit_Company_Data method in the Company_Manager.
         */
        [Route("api/Edit_Company_Info")]
        [HttpPost]
        public async Task<IActionResult> Edit_Company_Info([FromBody] Edit_Company_Info_Data input_obj)
        {
            if (input_obj == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {
                try
                {
                    Session_Data session = await Session_Manager.Active_Session(Request);

                    string result = await _DB_Helper.Edit_Company_Info(input_obj, session);

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


        /*
         * Get_Company_By_ID endpoint
         * This endpoint is used to get a record from to the Company table by its ID.
         * 
         * It accepts Get_Company_Data object.
         * The given object is handed over to the Get_Company_Data method in the Company_Manager.
         */
        [Route("api/Get_Company_Info")]
        [HttpGet]
        public async Task<IActionResult> Get_Company_Info()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                Company_Info_Model result = await _DB_Helper.Get_Company_Info(session);

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
