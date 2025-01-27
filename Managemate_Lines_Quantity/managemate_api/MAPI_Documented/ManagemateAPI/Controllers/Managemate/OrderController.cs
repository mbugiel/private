using Microsoft.AspNetCore.Mvc;
using ManagemateAPI.Management.M_Order.Input_Objects;
using ManagemateAPI.Management.M_Order.Manager;
using ManagemateAPI.Management.M_Order.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;

/*
 * This is an endpoint controller dedicated to the Order table.
 * 
 * It contains methods for endpoints
 * - Add 
 * - Edit
 * - Delete
 * - Get by ID
 * - Get all 
 */
namespace ManagemateAPI.Controllers.Managemate
{

    [ApiController]
    public class OrderController : ControllerBase
    {
        private Order_Manager _DB_Helper;

        public OrderController(IConfiguration configuration)
        {
            _DB_Helper = new Order_Manager(configuration);
        }

        /*
         * Add_Order endpoint
         * This endpoint is used to add a record to the Order table.
         * 
         * It accepts Add_Order_Data object.
         * The given object is handed over to the Add_Order method in the Order_Manager.
         */
        [Route("api/Add_Order")]
        [HttpPost]
        public async Task<IActionResult> Add_Order([FromBody] Add_Order_Data input_obj)
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

                    string result = await _DB_Helper.Add_Order(input_obj, session);

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


        /*
         * Edit_Order endpoint
         * This endpoint is used to edit a record from the Order table.
         * 
         * It accepts Edit_Order_Data object.
         * The given object is handed over to the Edit_Order method in the Order_Manager.
         */
        [Route("api/Edit_Order")]
        [HttpPost]
        public async Task<IActionResult> Edit_Order([FromBody] Edit_Order_Data input_obj)
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

                    List<Order_Error_Model> result = await _DB_Helper.Edit_Order(input_obj, session);

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


        /*
         * Delete_Order endpoint
         * This endpoint is used to remove record from the Order table.
         * 
         * It accepts Delete_Order_Data object.
         * The given object is handed over to the Delete_Order method in the Order_Manager.
         */
        [Route("api/Delete_Order")]
        [HttpPost]
        public async Task<IActionResult> Delete_Order([FromBody] Delete_Order_Data input_obj)
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

                    string result = _DB_Helper.Delete_Order(input_obj, session);

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


        /*
         * Get_Order_By_Id endpoint
         * This endpoint is used to get a record from to the Order table by its ID.
         * 
         * It accepts Get_Order_By_Id_Data object.
         * The given object is handed over to the Get_Order_By_Id method in the Order_Manager.
         */
        [Route("api/Get_Order_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Order_By_Id([FromBody] Get_Order_By_Id_Data input_obj)
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

                    Order_Model result = await _DB_Helper.Get_Order_By_Id(input_obj, session);

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


        /*
         * Get_All_Order endpoint
         * This endpoint is used to to all the records from the Order table.
         * 
         * It accepts Get_All_Order_Data object.
         * The given object is handed over to the Get_All_Order method in the Order_Manager.
         */
        [Route("api/Get_All_Order")]
        [HttpPost]
        public async Task<IActionResult> Get_All_Order()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Order_Model_List> result = await _DB_Helper.Get_All_Order(session);

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
