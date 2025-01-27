using Microsoft.AspNetCore.Mvc;
using ManagemateAPI.Management.M_Counting_Unit.Input_Objects;
using ManagemateAPI.Management.M_Counting_Unit.Manager;
using ManagemateAPI.Management.M_Counting_Unit.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;

/*
 * This is an endpoint controller dedicated to the counting_unit table.
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
    public class Counting_UnitController : ControllerBase
    {
        private Counting_Unit_Manager _DB_Helper;

        public Counting_UnitController(IConfiguration configuration)
        {
            _DB_Helper = new Counting_Unit_Manager(configuration);
        }

        /*
         * Add_Counting_Unit endpoint
         * This endpoint is used to add a record to the counting_unit table.
         * 
         * It accepts Add_Counting_Unit_Data object.
         * The given object is handed over to the Add_Counting_Unit method in the Counting_Unit_Manager.
         */
        [Route("api/Add_Counting_Unit")]
        [HttpPost]
        public async Task<IActionResult> Add_Counting_Unit([FromBody] Add_Counting_Unit_Data input_obj)
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

                    string result = _DB_Helper.Add_Counting_Unit(input_obj, session);

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
         * Edit_Counting_Unit endpoint
         * This endpoint is used to edit a record from the counting_unit table.
         * 
         * It accepts Edit_Counting_Unit_Data object.
         * The given object is handed over to the Edit_Counting_Unit method in the Counting_Unit_Manager.
         */
        [Route("api/Edit_Counting_Unit")]
        [HttpPost]
        public async Task<IActionResult> Edit_Counting_Unit([FromBody] Edit_Counting_Unit_Data input_obj)
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

                    string result = _DB_Helper.Edit_Counting_Unit(input_obj, session);

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
         * Delete_Counting_Unit endpoint
         * This endpoint is used to remove record from the counting_unit table.
         * 
         * It accepts Delete_Counting_Unit_Data object.
         * The given object is handed over to the Delete_Counting_Unit method in the Counting_Unit_Manager.
         */
        [Route("api/Delete_Counting_Unit")]
        [HttpPost]
        public async Task<IActionResult> Delete_Counting_Unit([FromBody] Delete_Counting_Unit_Data input_obj)
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

                    string result = _DB_Helper.Delete_Counting_Unit(input_obj, session);

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
         * Get_Counting_Unit_By_Id endpoint
         * This endpoint is used to get a record from to the counting_unit table by its ID.
         * 
         * It accepts Get_Counting_Unit_By_Id object.
         * The given object is handed over to the Get_Counting_Unit_By_Id method in the Counting_Unit_Manager.
         */
        [Route("api/Get_Counting_Unit_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Counting_Unit_By_Id([FromBody] Get_Counting_Unit_By_Id_Data input_obj)
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

                    Counting_Unit_Model result = _DB_Helper.Get_Counting_Unit_By_Id(input_obj, session);

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
         * Get_All_Counting_Unit endpoint
         * This endpoint is used to to all the records from the counting_unit table.
         * 
         * It accepts Get_All_Counting_Unit_Data object.
         * The given object is handed over to the Get_All_Counting_Unit method in the Counting_Unit_Manager.
         */
        [Route("api/Get_All_Item_Counting_Type")]
        [HttpGet]
        public async Task<IActionResult> Get_All_Item_Counting_Type()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Counting_Unit_Model> result = _DB_Helper.Get_All_Item_Counting_Type(session);

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
