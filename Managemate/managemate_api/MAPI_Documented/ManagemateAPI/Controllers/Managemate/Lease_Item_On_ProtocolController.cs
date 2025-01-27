using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Manager;
using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Lease_Item_On_ProtocolController : ControllerBase
    {
        private Lease_Item_On_Protocol_Manager _DB_Helper;

        public Lease_Item_On_ProtocolController(IConfiguration configuration)
        {
            _DB_Helper = new Lease_Item_On_Protocol_Manager(configuration);
        }


        [Route("api/Add_Lease_Item_On_Offer_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Add_Lease_Item_On_Offer_Protocol([FromBody] Add_Lease_Item_On_Protocol_Data input_obj)
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

                    string result = _DB_Helper.Add_Lease_Item_On_Offer_Protocol(input_obj, session);

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


        [Route("api/Edit_Lease_Item_On_Offer_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Edit_Lease_Item_On_Offer_Protocol([FromBody] Edit_Lease_Item_On_Protocol_Data input_obj)
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

                    string result = await _DB_Helper.Edit_Lease_Item_On_Offer_Protocol(input_obj, session);

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


        [Route("api/Add_Lease_Item_On_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Add_Lease_Item_On_Protocol([FromBody] Add_Lease_Item_On_Protocol_Data input_obj)
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

                    Lease_Item_On_Protocol_Id_Model result = _DB_Helper.Add_Lease_Item_On_Protocol(input_obj, session);

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


        [Route("api/Edit_Lease_Item_On_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Edit_Lease_Item_On_Protocol([FromBody] Edit_Lease_Item_On_Protocol_Data input_obj)
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

                    Lease_Item_On_Protocol_Id_Model result = await _DB_Helper.Edit_Lease_Item_On_Protocol(input_obj, session);

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


        [Route("api/Get_Available_Lease_Items_To_Return")]
        [HttpPost]
        public async Task<IActionResult> Get_Available_Lease_Items_To_Return([FromBody] Get_Available_Lease_Items_To_Return_Data input_obj)
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

                    List<Lease_Item_On_Protocol_Return_Available_Model> result = _DB_Helper.Get_Available_Lease_Items_To_Return(input_obj, session);

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


        [Route("api/Get_Available_Lease_Items_To_Offer")]
        [HttpPost]
        public async Task<IActionResult> Get_Available_Lease_Items_To_Offer([FromBody] Get_Available_Lease_Items_To_Release_Data input_obj)
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

                    List<Lease_Item_On_Protocol_Offer_Model> result = _DB_Helper.Get_Available_Lease_Items_To_Offer(input_obj, session);

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


        [Route("api/Get_Available_Lease_Items_To_Release")]
        [HttpPost]
        public async Task<IActionResult> Get_Available_Lease_Items_To_Release([FromBody] Get_Available_Lease_Items_To_Release_Data input_obj)
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

                    List<Lease_Item_On_Protocol_Release_Available_Model> result = _DB_Helper.Get_Available_Lease_Items_To_Release(input_obj, session);

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


        [Route("api/Get_Lease_Item_On_Protocol_List")]
        [HttpPost]
        public async Task<IActionResult> Get_Lease_Item_On_Protocol_List([FromBody] Get_Lease_Item_From_Protocol_Data input_obj)
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

                    List<Lease_Item_On_Protocol_Model> result = await _DB_Helper.Get_Lease_Item_On_Protocol_List(input_obj, session);

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
