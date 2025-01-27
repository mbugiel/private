using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using ManagemateAPI.Management.M_Lease_Item.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item.Manager;
using ManagemateAPI.Management.M_Lease_Item.Table_Model;

/*
 * This is an endpoint controller dedicated to the Item table.
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
    public class Lease_ItemController : ControllerBase
    {
        private Lease_Item_Manager _DB_Helper;

        public Lease_ItemController(IConfiguration configuration)
        {
            _DB_Helper = new Lease_Item_Manager(configuration);
        }


        /*
         * Add_Lease_Item endpoint
         * This endpoint is used to add a record to the lease_item table.
         * 
         * It accepts Add_Lease_Item_Data object.
         * The given object is handed over to the Add_Lease_Item method in the Lease_Item_Manager.
         */
        [Route("api/Add_Lease_Item")]
        [HttpPost]
        public async Task<IActionResult> Add_Lease_Item([FromBody] Add_Lease_Item_Data input_obj)
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

                    string result = await _DB_Helper.Add_Lease_Item(input_obj, session);

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
         * Edit_Lease_Item endpoint
         * This endpoint is used to edit a record from the lease_item table.
         * 
         * It accepts Edit_Lease_Item_Data object.
         * The given object is handed over to the Edit_Lease_Item method in the Lease_Item_Manager.
         */
        [Route("api/Edit_Lease_Item")]
        [HttpPost]
        public async Task<IActionResult> Edit_Lease_Item([FromBody] Edit_Lease_Item_Data input_obj)
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

                    Lease_Item_Error_Model result = await _DB_Helper.Edit_Lease_Item(input_obj, session);

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
         * Delete_Lease_Item endpoint
         * This endpoint is used to remove record from the lease_item table.
         * 
         * It accepts Delete_Lease_Item_Data object.
         * The given object is handed over to the Delete_Lease_Item method in the Lease_Item_Manager.
         */
        [Route("api/Delete_Lease_Item")]
        [HttpPost]
        public async Task<IActionResult> Delete_Lease_Item([FromBody] Delete_Lease_Item_Data input_obj)
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

                    Lease_Item_Error_Model result = _DB_Helper.Delete_Lease_Item(input_obj, session);

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
         * Get_Lease_Item_By_Id endpoint
         * This endpoint is used to get a record from to the lease_item table by its ID.
         * 
         * It accepts Get_Lease_Item_By_Id object.
         * The given object is handed over to the Get_Lease_Item_By_Id method in the Lease_Item_Manager.
         */
        [Route("api/Get_Lease_Item_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Lease_Item_By_Id([FromBody] Get_Lease_Item_By_Id_Data input_obj)
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

                    Lease_Item_Model result = await _DB_Helper.Get_Lease_Item_By_Id(input_obj, session);

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


        [Route("api/Get_Lease_Item_Details")]
        [HttpPost]
        public async Task<IActionResult> Get_Lease_Item_Details([FromBody] Get_Lease_Item_By_Id_Data input_obj)
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

                    Lease_Item_Model_Details result = await _DB_Helper.Get_Lease_Item_Details(input_obj, session);

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
         * Get_All_Lease_Item endpoint
         * This endpoint is used to to all the records from the lease_item table.
         * 
         * It accepts Get_All_Lease_Item_Data object.
         * The given object is handed over to the Get_All_Lease_Item method in the Lease_Item_Manager.
         */
        [Route("api/Get_All_Lease_Item")]
        [HttpPost]
        public async Task<IActionResult> Get_All_Lease_Item()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Lease_Item_Model_List> result = await _DB_Helper.Get_All_Lease_Item(session);

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
