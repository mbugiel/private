using Microsoft.AspNetCore.Mvc;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using ManagemateAPI.Management.M_Lease_Item_Group.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_Group.Manager;
using ManagemateAPI.Management.M_Lease_Item_Group.Table_Model;

/*
 * This is an endpoint controller dedicated to the lease_item_group table.
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
    public class Lease_Item_GroupController : ControllerBase
    {
        private Lease_Item_Group_Manager _DB_Helper;

        public Lease_Item_GroupController(IConfiguration configuration)
        {
            _DB_Helper = new Lease_Item_Group_Manager(configuration);
        }

        /*
         * Add_Lease_Item_Group endpoint
         * This endpoint is used to add a record to the lease_item_group table.
         * 
         * It accepts Add_Lease_Item_Group_Data object.
         * The given object is handed over to the Add_Lease_Item_Group method in the Lease_Item_Group_Manager.
         */
        [Route("api/Add_Lease_Item_Group")]
        [HttpPost]
        public async Task<IActionResult> Add_Lease_Item_Group([FromBody] Add_Lease_Item_Group_Data input_obj)
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

                    string result = _DB_Helper.Add_Lease_Item_Group(input_obj, session);

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
         * Edit_Lease_Item_Group endpoint
         * This endpoint is used to edit a record from the lease_item_group table.
         * 
         * It accepts Edit_Lease_Item_Group_Data object.
         * The given object is handed over to the Edit_Lease_Item_Group method in the Lease_Item_Group_Manager.
         */
        [Route("api/Edit_Lease_Item_Group")]
        [HttpPost]
        public async Task<IActionResult> Edit_Lease_Item_Group([FromBody] Edit_Lease_Item_Group_Data input_obj)
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

                    string result = _DB_Helper.Edit_Lease_Item_Group(input_obj, session);

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
         * Delete_Lease_Item_Group endpoint
         * This endpoint is used to remove record from the lease_item_group table.
         * 
         * It accepts Delete_Lease_Item_Group_Data object.
         * The given object is handed over to the Delete_Lease_Item_Group method in the Lease_Item_Group_Manager.
         */
        [Route("api/Delete_Lease_Item_Group")]
        [HttpPost]
        public async Task<IActionResult> Delete_Lease_Item_Group([FromBody] Delete_Lease_Item_Group_Data input_obj)
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

                    string result = _DB_Helper.Delete_Lease_Item_Group(input_obj, session);

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
         * Get_Lease_Item_Group_By_Id endpoint
         * This endpoint is used to get a record from to the lease_item_group table by its ID.
         * 
         * It accepts Get_Lease_Item_Group_By_Id object.
         * The given object is handed over to the Get_Lease_Item_Group_By_Id method in the Lease_Item_Group_Manager.
         */
        [Route("api/Get_Lease_Item_Group_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Lease_Item_Group_By_Id([FromBody] Get_Lease_Item_Group_By_Id_Data input_obj)
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

                    Lease_Item_Group_Model result = _DB_Helper.Get_Lease_Item_Group_By_Id(input_obj, session);

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
         * Get_All_Lease_Item_Group endpoint
         * This endpoint is used to to all the records from the lease_item_group table.
         * 
         * It accepts Get_All_Lease_Item_Group_Data object.
         * The given object is handed over to the Get_All_Lease_Item_Group method in the Lease_Item_Group_Manager.
         */
        [Route("api/Get_All_Lease_Item_Group")]
        [HttpPost]
        public async Task<IActionResult> Get_All_Lease_Item_Group()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Lease_Item_Group_Model> result = _DB_Helper.Get_All_Lease_Item_Group(session);

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
