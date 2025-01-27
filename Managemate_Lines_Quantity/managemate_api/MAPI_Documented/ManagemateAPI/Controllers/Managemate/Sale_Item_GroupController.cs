using ManagemateAPI.Management.M_Sale_Item.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item_Group.Input_Objects;
using ManagemateAPI.Management.M_Sale_Item_Group.Manager;
using ManagemateAPI.Management.M_Sale_Item_Group.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{
    [ApiController]
    public class Sale_Item_GroupController : ControllerBase
    {
        private Sale_Item_Group_Manager _DB_Helper;

        public Sale_Item_GroupController(IConfiguration configuration)
        {
            _DB_Helper = new Sale_Item_Group_Manager(configuration);
        }



        [Route("api/Add_Sale_Item_Group")]
        [HttpPost]
        public async Task<IActionResult> Add_Sale_Item_Group([FromBody] Add_Sale_Item_Group_Data input_obj)
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

                    string result = _DB_Helper.Add_Sale_Item_Group(input_obj, session);

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


        [Route("api/Edit_Sale_Item_Group")]
        [HttpPost]
        public async Task<IActionResult> Edit_Sale_Item_Group([FromBody] Edit_Sale_Item_Group_Data input_obj)
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

                    string result = _DB_Helper.Edit_Sale_Item_Group(input_obj, session);

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


        [Route("api/Delete_Sale_Item_Group")]
        [HttpPost]
        public async Task<IActionResult> Delete_Sale_Item_Group([FromBody] Delete_Sale_Item_Group_Data input_obj)
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

                    string result = _DB_Helper.Delete_Sale_Item_Group(input_obj, session);

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


        [Route("api/Get_Sale_Item_Group_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Sale_Item_Group_By_Id([FromBody] Get_Sale_Item_Group_By_Id_Data input_obj)
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

                    Sale_Item_Group_Model result = _DB_Helper.Get_Sale_Item_Group_By_Id(input_obj, session);

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


        [Route("api/Get_All_Sale_Item_Group")]
        [HttpGet]
        public async Task<IActionResult> Get_All_Sale_Item_Group()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Sale_Item_Group_Model> result = _DB_Helper.Get_All_Sale_Item_Group(session);

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
