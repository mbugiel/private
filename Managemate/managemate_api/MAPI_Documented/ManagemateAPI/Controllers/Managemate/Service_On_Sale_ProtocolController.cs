using ManagemateAPI.Management.M_Service_On_Lease_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Service_On_Sale_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Service_On_Sale_Protocol.Manager;
using ManagemateAPI.Management.M_Service_On_Sale_Protocol.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Service_On_Sale_ProtocolController : ControllerBase
    {
        private Service_On_Sale_Protocol_Manager _DB_Helper;

        public Service_On_Sale_ProtocolController(IConfiguration configuration)
        {
            _DB_Helper = new Service_On_Sale_Protocol_Manager(configuration);
        }


        [Route("api/Add_Service_On_Sale_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Add_Service_On_Sale_Protocol([FromBody] Add_Service_On_Sale_Protocol_Data input_obj)
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

                    string result = await _DB_Helper.Add_Service_On_Sale_Protocol(input_obj, session);

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


        [Route("api/Edit_Service_On_Sale_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Edit_Service_On_Sale_Protocol([FromBody] Edit_Service_On_Sale_Protocol_Data input_obj)
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

                    string result = await _DB_Helper.Edit_Service_On_Sale_Protocol(input_obj, session);

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


        [Route("api/Delete_Service_On_Sale_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Delete_Service_On_Sale_Protocol([FromBody] Delete_Service_On_Sale_Protocol_Data input_obj)
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

                    string result = _DB_Helper.Delete_Service_On_Sale_Protocol(input_obj, session);

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


        [Route("api/Get_Service_On_Sale_Protocol_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Service_On_Sale_Protocol_By_Id([FromBody] Get_Service_On_Sale_Protocol_By_Id_Data input_obj)
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

                    Service_On_Sale_Protocol_Model result = await _DB_Helper.Get_Service_On_Sale_Protocol_By_Id(input_obj, session);

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


        [Route("api/Get_All_Service_On_Sale_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Get_All_Service_On_Sale_Protocol([FromBody] Get_All_Service_On_Sale_Protocol_Data input_obj)
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

                    List<Service_On_Sale_Protocol_Model> result = await _DB_Helper.Get_All_Service_On_Sale_Protocol(input_obj, session);

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
