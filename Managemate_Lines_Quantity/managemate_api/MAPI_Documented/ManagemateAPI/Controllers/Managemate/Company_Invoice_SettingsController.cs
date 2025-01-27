using ManagemateAPI.Management.M_Company_Invoice_Settings.Input_Objects;
using ManagemateAPI.Management.M_Company_Invoice_Settings.Manager;
using ManagemateAPI.Management.M_Company_Invoice_Settings.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Company_Invoice_SettingsController : ControllerBase
    {


        private Company_Invoice_Settings_Manager _DB_Helper;

        public Company_Invoice_SettingsController(IConfiguration configuration)
        {
            _DB_Helper = new Company_Invoice_Settings_Manager(configuration);
        }


        /*
         * Add_Company_Invoice_Settings endpoint
         * This endpoint is used to add a record to the company_invoice_settings table.
         * 
         * It accepts Add_Company_Invoice_Settings_Data object.
         * The given object is handed over to the Add_Company_Invoice_Settings_Data method in the Company_Invoice_Settings_Manager.
         */
        [Route("api/Add_Company_Invoice_Settings")]
        [HttpPost]
        public async Task<IActionResult> Add_Company_Invoice_Settings([FromBody] Add_Company_Invoice_Settings_Data input_obj)
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

                    string result = _DB_Helper.Add_Company_Invoice_Settings(input_obj, session);

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
         * Edit_Company_Invoice_Settings endpoint
         * This endpoint is used to edit a record from the company_invoice_settings table.
         * 
         * It accepts Edit_Company_Logo_Data object.
         * The given object is handed over to the Edit_Company_Invoice_Settings method in the Company_Invoice_Settings_Manager.
         */
        [Route("api/Edit_Company_Invoice_Settings")]
        [HttpPost]
        public async Task<IActionResult> Edit_Company_Invoice_Settings([FromBody] Edit_Company_Invoice_Settings_Data input_obj)
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

                    string result = _DB_Helper.Edit_Company_Invoice_Settings(input_obj, session);

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
         * This endpoint is used to get a record from to the company_invoice_settings table by its ID.
         * 
         * It accepts Get_Company_Data object.
         * The given object is handed over to the Get_Company_Data method in the Company_Invoice_Settings_Manager.
         */
        [Route("api/Get_Company_Invoice_Settings")]
        [HttpGet]
        public async Task<IActionResult> Get_Company_Invoice_Settings()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                Company_Invoice_Settings_Model result = _DB_Helper.Get_Company_Invoice_Settings(session);

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
