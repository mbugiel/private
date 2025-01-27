﻿using ManagemateAPI.Management.M_Company_Logo.Input_Objects;
using ManagemateAPI.Management.M_Company_Logo.Manager;
using ManagemateAPI.Management.M_Company_Logo.Table_Model;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_Session.Manager;
using ManagemateAPI.Management.Shared.Static;
using Microsoft.AspNetCore.Mvc;

namespace ManagemateAPI.Controllers.Managemate
{

    [ApiController]
    public class Company_LogoController : ControllerBase
    {

        private Company_Logo_Manager _DB_Helper;

        public Company_LogoController(IConfiguration configuration)
        {
            _DB_Helper = new Company_Logo_Manager(configuration);
        }

        /*
         * Add_Company_Logo endpoint
         * This endpoint is used to add a record to the company_logo table.
         * 
         * It accepts Add_Company_Data object.
         * The given object is handed over to the Add_Company_Data method in the Company_Logo_Manager.
         */


        [Route("api/Add_Company_Logo")]
        [HttpPost]
        public async Task<IActionResult> Add_Company_Logo([FromForm] Add_Company_Logo_Data input_obj)
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

                    string result = await _DB_Helper.Add_Company_Logo(input_obj, session);

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
         * Edit_Company endpoint
         * This endpoint is used to edit a record from the company_logo table.
         * 
         * It accepts Edit_Company_Logo_Data object.
         * The given object is handed over to the Edit_Company_Logo method in the Company_Logo_Manager.
         */
        [Route("api/Edit_Company_Logo")]
        [HttpPost]
        public async Task<IActionResult> Edit_Company_Logo([FromForm] Edit_Company_Logo_Data input_obj)
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

                    string result = await _DB_Helper.Edit_Company_Logo(input_obj, session);

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
         * Get_Company_By_ID endpoint
         * This endpoint is used to get a record from to the company_logo table by its ID.
         * 
         * It accepts Get_Company_Data object.
         * The given object is handed over to the Get_Company_Data method in the Company_Logo_Manager.
         */
        [Route("api/Get_Company_Logo")]
        [HttpGet]
        public async Task<IActionResult> Get_Company_Logo()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                Company_Logo_Model result = await _DB_Helper.Get_Company_Logo(session);

                if (result == null)
                {
                    throw new Exception("14");//_14_NULL_ERROR
                }


                return File(result.company_logo, result.file_type, System_Path.COMPANY_LOGO_NAME);
            }
            catch (Exception e)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(e));
            }
        }


    }
}
