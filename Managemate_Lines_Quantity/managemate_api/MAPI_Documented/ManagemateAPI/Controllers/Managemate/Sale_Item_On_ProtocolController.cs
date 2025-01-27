using ManagemateAPI.Management.M_Sale_Item_On_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item_On_Protocol.Manager;
using ManagemateAPI.Management.M_Sale_Item_On_Protocol.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Sale_Item_On_ProtocolController : ControllerBase
    {
        private Sale_Item_On_Protocol_Manager _DB_Helper;

        public Sale_Item_On_ProtocolController(IConfiguration configuration)
        {
            _DB_Helper = new Sale_Item_On_Protocol_Manager(configuration);
        }


        [Route("api/Add_Sale_Item_On_Offer_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Add_Sale_Item_On_Offer_Protocol([FromBody] Add_Sale_Item_On_Protocol_Data input_obj)
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

                    string result = _DB_Helper.Add_Sale_Item_On_Offer_Protocol(input_obj, session);

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


        [Route("api/Edit_Sale_Item_On_Offer_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Edit_Sale_Item_On_Offer_Protocol([FromBody] Edit_Sale_Item_On_Protocol_Data input_obj)
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

                    string result = await _DB_Helper.Edit_Sale_Item_On_Offer_Protocol(input_obj, session);

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


        [Route("api/Add_Sale_Item_On_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Add_Sale_Item_On_Protocol([FromBody] Add_Sale_Item_On_Protocol_Data input_obj)
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

                    Sale_Item_On_Protocol_Id_Model result = _DB_Helper.Add_Sale_Item_On_Protocol(input_obj, session);

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


        [Route("api/Edit_Sale_Item_On_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Edit_Sale_Item_On_Protocol([FromBody] Edit_Sale_Item_On_Protocol_Data input_obj)
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

                    Sale_Item_On_Protocol_Id_Model result = await _DB_Helper.Edit_Sale_Item_On_Protocol(input_obj, session);

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


        [Route("api/Get_Available_Sale_Items_To_Offer")]
        [HttpPost]
        public async Task<IActionResult> Get_Available_Sale_Items_To_Offer([FromBody] Get_Available_Sale_Items_To_Release_Data input_obj)
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

                    List<Sale_Item_On_Protocol_Offer_Model> result = _DB_Helper.Get_Available_Sale_Items_To_Offer(input_obj, session);

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


        [Route("api/Get_Available_Sale_Items_To_Release")]
        [HttpPost]
        public async Task<IActionResult> Get_Available_Sale_Items_To_Release([FromBody] Get_Available_Sale_Items_To_Release_Data input_obj)
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

                    List<Sale_Item_On_Protocol_Release_Available_Model> result = _DB_Helper.Get_Available_Sale_Items_To_Release(input_obj, session);

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


        [Route("api/Get_Sale_Item_On_Protocol_List")]
        [HttpPost]
        public async Task<IActionResult> Get_Sale_Item_On_Protocol_List([FromBody] Get_Sale_Item_From_Protocol_Data input_obj)
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

                    List<Sale_Item_On_Protocol_Model> result = await _DB_Helper.Get_Sale_Item_On_Protocol_List(input_obj, session);

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
