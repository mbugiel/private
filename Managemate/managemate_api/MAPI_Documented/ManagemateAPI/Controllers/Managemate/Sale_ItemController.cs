using ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Table_Model;
using ManagemateAPI.Management.M_Sale_Item.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item.Manager;
using ManagemateAPI.Management.M_Sale_Item.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Sale_ItemController : ControllerBase
    {
        private Sale_Item_Manager _DB_Helper;

        public Sale_ItemController(IConfiguration configuration)
        {
            _DB_Helper = new Sale_Item_Manager(configuration);
        }


        [Route("api/Add_Sale_Item")]
        [HttpPost]
        public async Task<IActionResult> Add_Sale_Item([FromBody] Add_Sale_Item_Data input_obj)
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

                    string result = await _DB_Helper.Add_Sale_Item(input_obj, session);

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


        [Route("api/Edit_Sale_Item")]
        [HttpPost]
        public async Task<IActionResult> Edit_Sale_Item([FromBody] Edit_Sale_Item_Data input_obj)
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

                    Sale_Item_Error_Model result = await _DB_Helper.Edit_Sale_Item(input_obj, session);

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


        [Route("api/Delete_Sale_Item")]
        [HttpPost]
        public async Task<IActionResult> Delete_Sale_Item([FromBody] Delete_Sale_Item_Data input_obj)
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

                    Sale_Item_Error_Model result = _DB_Helper.Delete_Sale_Item(input_obj, session);

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


        [Route("api/Get_Sale_Item_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Sale_Item_By_Id([FromBody] Get_Sale_Item_By_Id_Data input_obj)
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

                    Sale_Item_Model result = await _DB_Helper.Get_Sale_Item_By_Id(input_obj, session);

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


        [Route("api/Get_Sale_Item_Details")]
        [HttpPost]
        public async Task<IActionResult> Get_Sale_Item_Details([FromBody] Get_Sale_Item_By_Id_Data input_obj)
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

                    Sale_Item_Model_Details result = await _DB_Helper.Get_Sale_Item_Details(input_obj, session);

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


        [Route("api/Get_All_Sale_Item")]
        [HttpGet]
        public async Task<IActionResult> Get_All_Sale_Item()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Sale_Item_Model_List> result = await _DB_Helper.Get_All_Sale_Item(session);

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
