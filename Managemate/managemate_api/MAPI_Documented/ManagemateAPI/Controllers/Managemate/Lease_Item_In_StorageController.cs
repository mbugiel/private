using ManagemateAPI.Management.M_Lease_Item.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_In_Storage.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_In_Storage.Manager;
using ManagemateAPI.Management.M_Lease_Item_In_Storage.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Lease_Item_In_StorageController : ControllerBase
    {
        private Lease_Item_In_Storage_Manager _DB_Helper;

        public Lease_Item_In_StorageController(IConfiguration configuration)
        {
            _DB_Helper = new Lease_Item_In_Storage_Manager(configuration);
        }


        [Route("api/Add_Lease_Item_In_Storage")]
        [HttpPost]
        public async Task<IActionResult> Add_Lease_Item_In_Storage([FromBody] Add_Lease_Item_In_Storage_Data input_obj)
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

                    Lease_Item_In_Storage_Error_Model result = _DB_Helper.Add_Lease_Item_In_Storage(input_obj, session);

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


        [Route("api/Remove_Lease_Item_In_Storage")]
        [HttpPost]
        public async Task<IActionResult> Remove_Lease_Item_In_Storage([FromBody] Remove_Lease_Item_In_Storage_Data input_obj)
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

                    Lease_Item_In_Storage_Error_Model result = _DB_Helper.Remove_Lease_Item_In_Storage(input_obj, session);

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


        [Route("api/Get_Lease_Item_In_Storage_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Lease_Item_In_Storage_By_Id([FromBody] Get_Lease_Item_In_Storage_By_Id_Data input_obj)
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

                    Lease_Item_In_Storage_Model result = _DB_Helper.Get_Lease_Item_In_Storage_By_Id(input_obj, session);

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


        [Route("api/Get_All_Lease_Item_In_Storage")]
        [HttpPost]
        public async Task<IActionResult> Get_All_Lease_Item_In_Storage([FromBody] Get_All_Lease_Item_In_Storage_Data input_obj)
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

                    List<Lease_Item_In_Storage_Model_List> result = _DB_Helper.Get_All_Lease_Item_In_Storage(input_obj, session);

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
}
