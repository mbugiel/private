using Microsoft.AspNetCore.Mvc;
using ManagemateAPI.Management.M_Construction_Site.Input_Objects;
using ManagemateAPI.Management.M_Construction_Site.Manager;
using ManagemateAPI.Management.M_Construction_Site.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;

/*
 * This is an endpoint controller dedicated to the Construction_Site table.
 * 
 * It contains methods for endpoints
 * - Add 
 * - Edit
 * - Delete
 * - Get by ID
 * - Get all 
 */
namespace ManagemateAPI.Controllers.Managemate
{

    [ApiController]
    public class Construction_SiteController : ControllerBase
    {
        private Construction_Site_Manager _DB_Helper;
        public Construction_SiteController(IConfiguration configuration)
        {
            _DB_Helper = new Construction_Site_Manager(configuration);
        }

        /*
         * Add_Construction_Site endpoint
         * This endpoint is used to add a record to the Construction_Site table.
         * 
         * It accepts Add_Construction_Site_Data object.
         * The given object is handed over to the Add_Construction_Site method in the Construction_Site_Manager.
         */
        [Route("api/Add_Construction_Site")]
        [HttpPost]
        public async Task<IActionResult> Add_Construction_Site([FromBody] Add_Construction_Site_Data input_obj)
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

                    string result = await _DB_Helper.Add_Construction_Site(input_obj, session);

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
         * Edit_Construction_Site endpoint
         * This endpoint is used to edit a record from the Construction_Site table.
         * 
         * It accepts Edit_Construction_Site_Data object.
         * The given object is handed over to the Edit_Construction_Site method in the Construction_Site_Manager.
         */
        [Route("api/Edit_Construction_Site")]
        [HttpPost]
        public async Task<IActionResult> Edit_Construction_Site([FromBody] Edit_Construction_Site_Data input_obj)
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

                    string result = await _DB_Helper.Edit_Construction_Site(input_obj, session);

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
         * Delete_Construction_Site endpoint
         * This endpoint is used to remove record from the Construction_Site table.
         * 
         * It accepts Delete_Construction_Site_Data object.
         * The given object is handed over to the Delete_Construction_Site method in the Construction_Site_Manager.
         */
        [Route("api/Delete_Construction_Site")]
        [HttpPost]
        public async Task<IActionResult> Delete_Construction_Site([FromBody] Delete_Construction_Site_Data input_obj)
        {
            if (input_obj == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));
            }
            else
            {
                try
                {
                    Session_Data session = await Session_Manager.Active_Session(Request);

                    string result = _DB_Helper.Delete_Construction_Site(input_obj, session);

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
         * Get_Construction_Site_By_ID endpoint
         * This endpoint is used to get a record from to the Construction_Site table by its ID.
         * 
         * It accepts Get_Construction_Site_By_ID object.
         * The given object is handed over to the Get_Construction_Site_By_ID method in the Construction_Site_Manager.
         */
        [Route("api/Get_Construction_Site_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Construction_Site_By_Id([FromBody] Get_Construction_Site_By_Id input_obj)
        {
            if (input_obj == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));
            }
            else
            {
                try
                {
                    Session_Data session = await Session_Manager.Active_Session(Request);

                    Construction_Site_Model result = await _DB_Helper.Get_Construction_Site_By_Id(input_obj, session);

                    if (result == null)
                    {
                        throw new Exception("14");
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
         * Get_All_Construction_Site endpoint
         * This endpoint is used to to all the records from the Construction_Site table.
         * 
         * It accepts Get_All_Construction_Site_Data object.
         * The given object is handed over to the Get_All_Construction_Site method in the Construction_Site_Manager.
         */
        [Route("api/Get_All_Construction_Site")]
        [HttpGet]
        public async Task<IActionResult> Get_All_Construction_Site()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Construction_Site_Model_List> result = await _DB_Helper.Get_All_Construction_Site(session);

                if (result == null)
                {
                    throw new Exception("14");
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
