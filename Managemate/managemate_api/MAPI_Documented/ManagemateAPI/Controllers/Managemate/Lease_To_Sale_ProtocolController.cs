using ManagemateAPI.Management.M_Lease_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_Protocol.Table_Model;
using ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Manager;
using ManagemateAPI.Management.M_Lease_To_Sale_Protocol.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Lease_To_Sale_ProtocolController : ControllerBase
    {
        private Lease_To_Sale_Protocol_Manager _DB_Helper;

        public Lease_To_Sale_ProtocolController(IConfiguration configuration)
        {
            _DB_Helper = new Lease_To_Sale_Protocol_Manager(configuration);
        }


        [Route("api/Create_Lease_To_Sale_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Create_Lease_To_Sale_Protocol([FromBody] Create_Lease_To_Sale_Protocol_Data input_obj)
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

                    Lease_To_Sale_Protocol_Id_Model result = _DB_Helper.Create_Lease_To_Sale_Protocol(input_obj, session);

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


        [Route("api/Remove_Lease_To_Sale_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Remove_Lease_To_Sale_Protocol([FromBody] Remove_Lease_To_Sale_Protocol_Data input_obj)
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

                    Lease_Protocol_Id_Model_After_Remove_Data result = _DB_Helper.Remove_Lease_To_Sale_Protocol(input_obj, session);

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

                    Lease_To_Sale_Protocol_Model result = await _DB_Helper.Get_Lease_Protocol_By_Id(input_obj, session);

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


        [Route("api/Print_Lease_To_Sale_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Print_Lease_To_Sale_Protocol([FromBody] Print_Lease_To_Sale_Protocol_Data input_obj)
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

                    Lease_To_Sale_Protocol_Print_Model result = await _DB_Helper.Print_Lease_To_Sale_Protocol(input_obj, session);

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


        [Route("api/Get_All_Lease_To_Sale_Protocol")]
        [HttpPost]
        public async Task<IActionResult> Get_All_Lease_To_Sale_Protocol([FromBody] Get_All_Lease_To_Sale_Protocol_Data input_obj)
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

                    List<Lease_To_Sale_Protocol_Model_List> result = await _DB_Helper.Get_All_Lease_To_Sale_Protocol(input_obj, session);

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
