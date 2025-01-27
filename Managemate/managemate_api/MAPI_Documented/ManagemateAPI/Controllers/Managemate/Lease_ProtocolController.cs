using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_Item_On_Protocol.Table_Model;
using ManagemateAPI.Management.M_Lease_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_Protocol.Manager;
using ManagemateAPI.Management.M_Lease_Protocol.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Lease_ProtocolController : ControllerBase
    {
        private Lease_Protocol_Manager _DB_Helper;

        public Lease_ProtocolController(IConfiguration configuration)
        {
            _DB_Helper = new Lease_Protocol_Manager(configuration);
        }


        [Route("api/Add_Lease_Offer_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Add_Lease_Offer_Protocol([FromBody] Add_Lease_Offer_Protocol_Data input_obj)
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

                    Lease_Protocol_Model_Id result = _DB_Helper.Add_Lease_Offer_Protocol(input_obj, session);

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


        [Route("api/Edit_Lease_Offer_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Edit_Lease_Offer_Protocol([FromBody] Edit_Lease_Protocol_Data input_obj)
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

                    Lease_Offer_Protocol_Model_Id result = _DB_Helper.Edit_Lease_Offer_Protocol(input_obj, session);

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


        [Route("api/Create_Max_Offer_List")]
        [HttpPost]
        public async Task<IActionResult> Create_Max_Offer_List([FromBody] Create_Max_Lease_Offer_Data input_obj)
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

                    Lease_Offer_Protocol_Model_Id result = _DB_Helper.Create_Max_Offer_List(input_obj, session);

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


        [Route("api/Add_Lease_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Add_Lease_Protocol([FromBody] Add_Lease_Protocol_Data input_obj)
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

                    Lease_Protocol_Model_Id result = _DB_Helper.Add_Lease_Protocol(input_obj, session);

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


        [Route("api/Edit_Lease_Protocol_Base")]
        [HttpPost]
        public async Task<IActionResult> Edit_Lease_Protocol_Base([FromBody] Edit_Lease_Protocol_Base_Data input_obj)
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

                    string result = await _DB_Helper.Edit_Lease_Protocol_Base(input_obj, session);

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


        [Route("api/Edit_Lease_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Edit_Lease_Protocol([FromBody] Edit_Lease_Protocol_Data input_obj)
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

                    List<Lease_Item_On_Protocol_Error_Model> result = _DB_Helper.Edit_Lease_Protocol(input_obj, session);

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


        [Route("api/Delete_Lease_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Delete_Lease_Protocol([FromBody] Delete_Lease_Protocol_Data input_obj)
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

                    string result = _DB_Helper.Delete_Lease_Protocol(input_obj, session);

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


        [Route("api/Get_Lease_Protocol_Base_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Lease_Protocol_Base_By_Id([FromBody] Get_Lease_Protocol_By_Id_Data input_obj)
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

                    Lease_Protocol_Base_Model result = await _DB_Helper.Get_Lease_Protocol_Base_By_Id(input_obj, session);

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


        [Route("api/Get_Lease_Protocol_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Lease_Protocol_By_Id([FromBody] Get_Lease_Protocol_By_Id_Data input_obj)
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

                    Lease_Protocol_Model result = await _DB_Helper.Get_Lease_Protocol_By_Id(input_obj, session);

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


        [Route("api/Get_All_Lease_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Get_All_Lease_Protocol([FromBody] Get_All_Lease_Protocol_Data input_obj)
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

                    List<Lease_Protocol_Model_List> result = await _DB_Helper.Get_All_Lease_Protocol(input_obj, session);

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


        [Route("api/Print_Lease_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Print_Lease_Protocol([FromBody] Print_Lease_Protocol_Data input_obj)
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

                    Lease_Protocol_Print_Model result = await _DB_Helper.Print_Lease_Protocol(input_obj, session);

                    if (result == null)
                    {
                        throw new Exception("14");//_14_NULL_ERROR
                    }

                    return File(result.protocol_bytes, "application/pdf", result.protocol_file_name);

                }
                catch (Exception e)
                {
                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }
            }
        }



    }
}
