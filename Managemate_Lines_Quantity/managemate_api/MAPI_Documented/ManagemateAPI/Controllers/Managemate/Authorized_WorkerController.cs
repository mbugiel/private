using Microsoft.AspNetCore.Mvc;
using ManagemateAPI.Management.M_Authorized_Worker.Input_Objects;
using ManagemateAPI.Management.M_Authorized_Worker.Table_Model;
using ManagemateAPI.Management.M_Authorized_Worker.Manager;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;


/*
 * This is an endpoint controller dedicated to the Authorized_Worker table.
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
    public class Authorized_WorkerController : ControllerBase
    {
        private Authorized_Worker_Manager _DB_Helper;
        public Authorized_WorkerController(IConfiguration configuration)
        {
            _DB_Helper = new Authorized_Worker_Manager(configuration);
        }

        /*
         * Add_Authorized_Worker endpoint
         * This endpoint is used to add a record to the Authorized_Worker table.
         * 
         * It accepts Add_Authorized_Worker_Data object.
         * The given object is handed over to the Add_Authorized_Worker method in the Authorized_Worker_Manager.
         */
        [Route("api/Add_Authorized_Worker")]
        [HttpPost]
        public async Task<IActionResult> Add_Authorized_Worker([FromBody] Add_Authorized_Worker_Data input_obj)
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

                    string result = await _DB_Helper.Add_Authorized_Worker(input_obj, session);

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
         * Edit_Authorized_Worker endpoint
         * This endpoint is used to edit a record from the Authorized_Worker table.
         * 
         * It accepts Edit_Authorized_Worker_Data object.
         * The given object is handed over to the Edit_Authorized_Worker method in the Authorized_Worker_Manager.
         */
        [Route("api/Edit_Authorized_Worker")]
        [HttpPost]
        public async Task<IActionResult> Edit_Authorized_Worker([FromBody] Edit_Authorized_Worker_Data input_obj)
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

                    string result = await _DB_Helper.Edit_Authorized_Worker(input_obj, session);

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
         * Delete_Authorized_Worker endpoint
         * This endpoint is used to remove record from the Authorized_Worker table.
         * 
         * It accepts Delete_Authorized_Worker_Data object.
         * The given object is handed over to the Delete_Authorized_Worker method in the Authorized_Worker_Manager.
         */
        [Route("api/Delete_Authorized_Worker")]
        [HttpPost]
        public async Task<IActionResult> Delete_Authorized_Worker([FromBody] Delete_Authorized_Worker_Data input_obj)
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

                    string result = _DB_Helper.Delete_Authorized_Worker(input_obj, session);

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
         * Get_Authorized_Worker_By_ID endpoint
         * This endpoint is used to get a record from to the Authorized_Worker table by its ID.
         * 
         * It accepts Get_Authorized_Worker_By_ID object.
         * The given object is handed over to the Get_Authorized_Worker_By_ID method in the Authorized_Worker_Manager.
         */
        [Route("api/Get_Authorized_Worker_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Authorized_Worker_By_Id([FromBody] Get_Authorized_Worker_By_Id_Data input_obj)
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

                    Authorized_Worker_Model result = await _DB_Helper.Get_Authorized_Worker_By_Id(input_obj, session);

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
         * Get_All_Authorized_Worker endpoint
         * This endpoint is used to to all the records from the Authorized_Worker table.
         * 
         * It accepts Get_All_Authorized_Worker_Data object.
         * The given object is handed over to the Get_All_Authorized_Worker method in the Authorized_Worker_Manager.
         */
        [Route("api/Get_All_Authorized_Worker")]
        [HttpPost]
        public async Task<IActionResult> Get_All_Authorized_Worker([FromBody] Get_All_Authorized_Worker_Data input_obj)
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

                    List<Authorized_Worker_Model_List> result = await _DB_Helper.Get_All_Authorized_Worker(input_obj, session);

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
}
