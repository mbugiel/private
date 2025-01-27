using ManagemateAPI.Management.M_Service_On_Sale_Protocol.Input_Objects;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using ManagemateAPI.Management.M_Storage.Input_Objects;
using ManagemateAPI.Management.M_Storage.Manager;
using ManagemateAPI.Management.M_Storage.Table_Model;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class StorageController : ControllerBase
    {
        private Storage_Manager _DB_Helper;

        public StorageController(IConfiguration configuration)
        {
            _DB_Helper = new Storage_Manager(configuration);
        }


        [Route("api/Add_Storage")]
        [HttpPost]
        public async Task<IActionResult> Add_Storage([FromBody] Add_Storage_Data input_obj)
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

                    string result = await _DB_Helper.Add_Storage(input_obj, session);

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


        [Route("api/Edit_Storage")]
        [HttpPost]
        public async Task<IActionResult> Edit_Storage([FromBody] Edit_Storage_Data input_obj)
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

                    string result = await _DB_Helper.Edit_Storage(input_obj, session);

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


        [Route("api/Delete_Storage")]
        [HttpPost]
        public async Task<IActionResult> Delete_Storage([FromBody] Delete_Storage_Data input_obj)
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

                    string result = _DB_Helper.Delete_Storage(input_obj, session);

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


        [Route("api/Get_Storage_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Storage_By_Id([FromBody] Get_Storage_By_Id_Data input_obj)
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

                    Storage_Model result = await _DB_Helper.Get_Storage_By_Id(input_obj, session);

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


        [Route("api/Get_All_Storage")]
        [HttpGet]
        public async Task<IActionResult> Get_All_Storage()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Storage_Model_List> result = await _DB_Helper.Get_All_Storage(session);

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
