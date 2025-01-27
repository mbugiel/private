using ManagemateAPI.Management.M_Sale_Item_Group.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item_Group.Table_Model;
using ManagemateAPI.Management.M_Sale_Item_In_Storage.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item_In_Storage.Manager;
using ManagemateAPI.Management.M_Sale_Item_In_Storage.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Sale_Item_In_StorageController : ControllerBase
    {
        private Sale_Item_In_Storage_Manager _DB_Helper;

        public Sale_Item_In_StorageController(IConfiguration configuration)
        {
            _DB_Helper = new Sale_Item_In_Storage_Manager(configuration);
        }


        [Route("api/Add_Sale_Item_In_Storage")]
        [HttpPost]
        public async Task<IActionResult> Add_Sale_Item_In_Storage([FromBody] Add_Sale_Item_In_Storage_Data input_obj)
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

                    Sale_Item_In_Storage_Error_Model result = _DB_Helper.Add_Sale_Item_In_Storage(input_obj, session);

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


        [Route("api/Remove_Sale_Item_In_Storage")]
        [HttpPost]
        public async Task<IActionResult> Remove_Sale_Item_In_Storage([FromBody] Remove_Sale_Item_In_Storage_Data input_obj)
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

                    Sale_Item_In_Storage_Error_Model result = _DB_Helper.Remove_Sale_Item_In_Storage(input_obj, session);

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


        [Route("api/Get_Sale_Item_In_Storage_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Sale_Item_In_Storage_By_Id([FromBody] Get_Sale_Item_In_Storage_By_Id_Data input_obj)
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

                    Sale_Item_In_Storage_Model result = _DB_Helper.Get_Sale_Item_In_Storage_By_Id(input_obj, session);

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


        [Route("api/Get_All_Sale_Item_In_Storage")]
        [HttpPost]
        public async Task<IActionResult> Get_All_Sale_Item_In_Storage([FromBody] Get_All_Sale_Item_In_Storage_Data input_obj)
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

                    List<Sale_Item_In_Storage_Model_List> result = _DB_Helper.Get_All_Sale_Item_In_Storage(input_obj, session);

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
