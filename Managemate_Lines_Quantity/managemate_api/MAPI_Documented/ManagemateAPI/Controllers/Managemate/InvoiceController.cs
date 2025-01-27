using Microsoft.AspNetCore.Mvc;
using ManagemateAPI.Management.M_Invoice.Manager;
using ManagemateAPI.Management.M_Invoice.Table_Model;
using ManagemateAPI.Management.M_Invoice.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using ManagemateAPI.Management.M_Session.Input_Objects;

/*
 * This is an endpoint controller dedicated to the Invoice table.
 * 
 * It contains methods for endpoints
 * - Add 
 * - Edit
 * - Delete
 * - Get by ID
 */
namespace ManagemateAPI.Controllers.Managemate
{

    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private Invoice_Manager _DB_Helper;
        public InvoiceController(IConfiguration _configuration)
        {
            _DB_Helper = new Invoice_Manager(_configuration);
        }


        [Route("api/Get_Order_List_With_Pending_Invoices")]
        [HttpGet]
        public async Task<IActionResult> Get_Order_List_With_Pending_Invoices()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                List<Order_With_Pending_Invoices_Model> result = await _DB_Helper.Get_Order_List_With_Pending_Invoices(session);

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




        [Route("api/Get_Order_Pending_Invoice_List")]
        [HttpPost]
        public async Task<IActionResult> Get_Order_Pending_Invoice_List([FromBody] Get_Order_Pending_Invoice_List_Data input_obj)
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

                    Order_Pending_Invoice_List_Model result = await _DB_Helper.Get_Order_Pending_Invoice_List(input_obj, session);

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
         * Create_Invoice endpoint
         * This endpoint is used to add a record to the Invoice table.
         * 
         * It accepts Create_Invoice_Data object.
         * The given object is handed over to the Create_Invoice method in the Invoice_Manager.
         */
        [Route("api/Create_Invoice")]
        [HttpPost]
        public async Task<IActionResult> Create_Invoice([FromBody] Create_Invoice_Data input_obj)
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

                    Invoice_Id_Model result = await _DB_Helper.Create_Invoice(input_obj, session);

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
         * Edit_Invoice endpoint
         * This endpoint is used to edit a record from the Invoice table.
         * 
         * It accepts Edit_Invoice_Data object.
         * The given object is handed over to the Edit_Invoice method in the Invoice_Manager.
         */
        [Route("api/Edit_Invoice")]
        [HttpPost]
        public async Task<IActionResult> Edit_Invoice([FromBody] Edit_Invoice_Data input_obj)
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

                    Invoice_Id_Model result = await _DB_Helper.Edit_Invoice(input_obj, session);

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
         * Delete_Invoice endpoint
         * This endpoint is used to remove record from the Invoice table.
         * 
         * It accepts Delete_Invoice_Data object.
         * The given object is handed over to the Delete_Invoice method in the Invoice_Manager.
         */
        [Route("api/Delete_Invoice")]
        [HttpPost]
        public async Task<IActionResult> Delete_Invoice([FromBody] Delete_Invoice_Data input_obj)
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

                    string result = _DB_Helper.Delete_Invoice(input_obj, session);

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
         * Get_Invoice_By_Id endpoint
         * This endpoint is used to get a record from to the Invoice table by its ID.
         * 
         * It accepts Get_Invoice_By_Id object.
         * The given object is handed over to the Get_Invoice_By_Id method in the Invoice_Manager.
         */
        [Route("api/Get_Invoice_By_Id")]
        [HttpPost]
        public async Task<IActionResult> Get_Invoice_By_Id([FromBody] Get_Invoice_By_Id_Data input_obj)
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

                    Invoice_Model result = await _DB_Helper.Get_Invoice_By_Id(input_obj, session);

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
         * Get_Invoice_List endpoint
         * This endpoint is used to .......
         * 
         * It accepts Get_Invoice_List_Data object.
         * The given object is handed over to the Get_Invoice_List method in the Invoice_Manager.
         */
        [Route("api/Get_Invoice_List")]
        [HttpPost]
        public async Task<IActionResult> Get_Invoice_List([FromBody] Get_Invoice_List_Data input_obj)
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

                    Order_With_Invoice_List_Model result = await _DB_Helper.Get_Invoice_List(input_obj, session);

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
          * Print_Invoice endpoint
          * This endpoint is used to print an invoice based on the input.
          * 
          * It accepts Print_Invoice_Data object.
          * The given object is handed over to the Issue_An_Invoice method in the Invoice_Manager.
          */
        [Route("api/Print_Invoice")]
        [HttpPost]
        public async Task<IActionResult> Print_Invoice([FromBody] Print_Invoice_Data input_obj)
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

                    Invoice_Print_Model result = await _DB_Helper.Print_Invoice(input_obj, session);

                    if (result == null)
                    {
                        throw new Exception("14");//_14_NULL_ERROR
                    }

                    return File(result.invoice_bytes, "application/pdf", result.invoice_file_name);

                }
                catch (Exception e)
                {
                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }
            }
        }


    }
}
