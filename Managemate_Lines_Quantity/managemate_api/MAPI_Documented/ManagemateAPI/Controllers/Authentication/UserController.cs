using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Management.M_User.AuthAPI_Input_Objects;
using ManagemateAPI.Management.M_User.Input_Objects;
using Microsoft.Extensions.Configuration;
using ManagemateAPI.Management.M_Session.Manager;
using ManagemateAPI.Management.Shared.Static;

namespace ManagemateAPI.Controllers.Authentication
{

    [ApiController]
    public class UserController : ControllerBase
    {


        [Route("api/User_Exist")]
        [HttpPost]
        public async Task<IActionResult> UserExist([FromBody] User_Exist_Data new_user_check)
        {
            if (new_user_check == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {

                try
                {

                    AuthAPI_User_Exist_Data authAPI_new_user_check = new AuthAPI_User_Exist_Data
                    {
                        Username = new_user_check.Username,
                        Email = new_user_check.Email
                    };


                    using (var httpClient = new HttpClient())
                    {

                        StringContent content = new StringContent(JsonSerializer.Serialize(authAPI_new_user_check), Encoding.UTF8, "application/json");

                        using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "User_Exist", content))
                        {

                            var responseAsString = await authResponse.Content.ReadAsStringAsync();

                            if (responseAsString == null)
                            {
                                throw new Exception("14");//_14_NULL_ERROR
                            }
                            else
                            {

                                Api_Response apiResponse = JsonSerializer.Deserialize<Api_Response>(responseAsString);


                                if (apiResponse != null)
                                {

                                    if (apiResponse.code.Equals("0"))
                                    {
                                        ResponseType responseType = ResponseType.Success;


                                        return Ok(Response_Handler.GetAppResponse(responseType, apiResponse.responseData));
                                    }
                                    else
                                    {
                                        throw new Exception(apiResponse.code);
                                    }

                                }
                                else
                                {
                                    throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                                }

                            }


                        }



                    }

                }
                catch (Exception e)
                {

                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }

            }

        }

        [Route("api/Session_Check")]
        [HttpPost]
        public async Task<IActionResult> Session_Check()
        {
            try
            {
                Session_Data session = await Session_Manager.Active_Session(Request);

                ResponseType responseType = ResponseType.Success;

                return Ok(Response_Handler.GetAppResponse(responseType, "Success"));
            }
            catch (Exception e)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(e));
            }

        }


        [Route("api/Has_Two_Step_Login")]
        [HttpPost]
        public async Task<IActionResult> HasTwoStepLogin([FromBody] Has_Two_Step_Login_Data login_data)
        {
            if (login_data == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {

                try
                {

                    AuthAPI_Has_Two_Step_Login_Data authAPI_login_data = new AuthAPI_Has_Two_Step_Login_Data
                    {
                        Username = login_data.Username,
                        Password = login_data.Password
                    };


                    using (var httpClient = new HttpClient())
                    {

                        StringContent content = new StringContent(JsonSerializer.Serialize(authAPI_login_data), Encoding.UTF8, "application/json");

                        using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Has_Two_Step_Login", content))
                        {

                            var responseAsString = await authResponse.Content.ReadAsStringAsync();

                            if (responseAsString == null)
                            {
                                throw new Exception("14");//_14_NULL_ERROR
                            }
                            else
                            {

                                Api_Response apiResponse = JsonSerializer.Deserialize<Api_Response>(responseAsString);


                                if (apiResponse != null)
                                {

                                    if (apiResponse.code.Equals("0"))
                                    {
                                        ResponseType responseType = ResponseType.Success;
                                        return Ok(Response_Handler.GetAppResponse(responseType, apiResponse.responseData));
                                    }
                                    else
                                    {
                                        throw new Exception(apiResponse.code);
                                    }

                                }
                                else
                                {
                                    throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                                }

                            }


                        }



                    }

                }
                catch (Exception e)
                {

                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }

            }


        }




        [Route("api/Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] Login_Data login_data)
        {
            if (login_data == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {

                try
                {

                    AuthAPI_Login_Data authAPI_Login_Data = new AuthAPI_Login_Data
                    {
                        Username = login_data.Username,
                        Password = login_data.Password,
                        EmailToken = login_data.EmailToken
                    };


                    using (var httpClient = new HttpClient())
                    {

                        StringContent content = new StringContent(JsonSerializer.Serialize(authAPI_Login_Data), Encoding.UTF8, "application/json");

                        using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Login", content))
                        {

                            var responseAsString = await authResponse.Content.ReadAsStringAsync();

                            if (responseAsString == null)
                            {
                                throw new Exception("14");//_14_NULL_ERROR
                            }
                            else
                            {

                                Api_Response_Session apiResponse = JsonSerializer.Deserialize<Api_Response_Session>(responseAsString);


                                if (apiResponse != null)
                                {

                                    if (apiResponse.code.Equals("0"))
                                    {
                                        ResponseType responseType = ResponseType.Success;



                                        Session_Manager.Set_Cookies(apiResponse.responseData, Response);



                                        return Ok(Response_Handler.GetAppResponse(responseType, Info.SUCCESSFULLY_LOGGED_IN));
                                    }
                                    else
                                    {
                                        throw new Exception(apiResponse.code);
                                    }

                                }
                                else
                                {
                                    throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                                }

                            }


                        }



                    }

                }
                catch (Exception e)
                {

                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }

            }


        }






        [Route("api/Logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            
            try
            {

                Session_Data session = await Session_Manager.Active_Session(Request);


                using (var httpClient = new HttpClient())
                {

                    StringContent content = new StringContent(JsonSerializer.Serialize(session), Encoding.UTF8, "application/json");

                    using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Logout", content))
                    {

                        var responseAsString = await authResponse.Content.ReadAsStringAsync();

                        if (responseAsString == null)
                        {
                            throw new Exception("14");//_14_NULL_ERROR
                        }
                        else
                        {

                            Api_Response apiResponse = JsonSerializer.Deserialize<Api_Response>(responseAsString);


                            if (apiResponse != null)
                            {

                                if (apiResponse.code.Equals("0"))
                                {
                                    ResponseType responseType = ResponseType.Success;
                                    Session_Manager.Delete_Cookies(Response);

                                    return Ok(Response_Handler.GetAppResponse(responseType, apiResponse.responseData));
                                }
                                else
                                {
                                    throw new Exception(apiResponse.code);
                                }

                            }
                            else
                            {
                                throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                            }

                        }


                    }



                }


            }
            catch (Exception e)
            {

                return BadRequest(Response_Handler.GetExceptionResponse(e));

            }


        }




        [Route("api/Update_Password")]
        [HttpPost]
        public async Task<IActionResult> UpdatePassword([FromBody] Update_Passwd_Data update_data)
        {

            if (update_data == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {
                try
                {


                    Session_Data session = await Session_Manager.Active_Session(Request);

                    AuthAPI_Update_Passwd_Data authAPI_update_data = new AuthAPI_Update_Passwd_Data 
                    {
                            
                        SessionData = session,
                        newPassword = update_data.newPassword,
                        Password = update_data.Password
                        
                    };


                    using (var httpClient = new HttpClient())
                    {

                        StringContent content = new StringContent(JsonSerializer.Serialize(authAPI_update_data), Encoding.UTF8, "application/json");

                        using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Update_Password", content))
                        {

                            var responseAsString = await authResponse.Content.ReadAsStringAsync();

                            if (responseAsString == null)
                            {
                                throw new Exception("14");//_14_NULL_ERROR
                            }
                            else
                            {

                                Api_Response apiResponse = JsonSerializer.Deserialize<Api_Response>(responseAsString);


                                if (apiResponse != null)
                                {

                                    if (apiResponse.code.Equals("0"))
                                    {
                                        ResponseType responseType = ResponseType.Success;
                                        return Ok(Response_Handler.GetAppResponse(responseType, apiResponse.responseData));
                                    }
                                    else
                                    {
                                        throw new Exception(apiResponse.code);
                                    }

                                }
                                else
                                {
                                    throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                                }

                            }


                        }



                    }

                    

                }
                catch (Exception e)
                {

                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }


            }

        }



        // POST api/ConfirmEmail
        [Route("api/Set_Password")]
        [HttpPost]
        public async Task<IActionResult> SetPassword([FromBody] Set_Passwd_Data set_data)
        {

            if (set_data == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {
                try
                {

                    AuthAPI_Set_Passwd_Data authAPI_set_data = new AuthAPI_Set_Passwd_Data
                    {
                        Email = set_data.Email,
                        EmailToken = set_data.EmailToken,
                        newPassword = set_data.newPassword
                    };


                    using (var httpClient = new HttpClient())
                    {


                        StringContent content = new StringContent(JsonSerializer.Serialize(authAPI_set_data), Encoding.UTF8, "application/json");

                        using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Set_Password", content))
                        {

                            var responseAsString = await authResponse.Content.ReadAsStringAsync();

                            if (responseAsString == null)
                            {
                                throw new Exception("14");//_14_NULL_ERROR
                            }
                            else
                            {

                                Api_Response apiResponse = JsonSerializer.Deserialize<Api_Response>(responseAsString);


                                if (apiResponse != null)
                                {

                                    if (apiResponse.code.Equals("0"))
                                    {
                                        ResponseType responseType = ResponseType.Success;
                                        return Ok(Response_Handler.GetAppResponse(responseType, apiResponse.responseData));
                                    }
                                    else
                                    {
                                        throw new Exception(apiResponse.code);
                                    }

                                }
                                else
                                {
                                    throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                                }

                            }


                        }



                    }

                }
                catch (Exception e)
                {

                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }


            }

        }




        [Route("api/Validate_Password")]
        [HttpPost]
        public async Task<IActionResult> ValidatePassword([FromBody] Validate_Password_Data validate_data)
        {

            if (validate_data == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {
                try
                {

                    Session_Data session = await Session_Manager.Active_Session(Request);

                    AuthAPI_Validate_Password_Data authAPI_validate_data = new AuthAPI_Validate_Password_Data
                    {
                        SessionData = session,
                        Password = validate_data.Password
                    };


                    using (var httpClient = new HttpClient())
                    {

                        StringContent content = new StringContent(JsonSerializer.Serialize(authAPI_validate_data), Encoding.UTF8, "application/json");

                        using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Validate_Password", content))
                        {

                            var responseAsString = await authResponse.Content.ReadAsStringAsync();

                            if (responseAsString == null)
                            {
                                throw new Exception("14");//_14_NULL_ERROR
                            }
                            else
                            {

                                Api_Response apiResponse = JsonSerializer.Deserialize<Api_Response>(responseAsString);


                                if (apiResponse != null)
                                {

                                    if (apiResponse.code.Equals("0"))
                                    {
                                        ResponseType responseType = ResponseType.Success;
                                        return Ok(Response_Handler.GetAppResponse(responseType, apiResponse.responseData));
                                    }
                                    else
                                    {
                                        throw new Exception(apiResponse.code);
                                    }

                                }
                                else
                                {
                                    throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                                }

                            }


                        }



                    }


                }
                catch (Exception e)
                {

                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }


            }

        }



        // POST api/ConfirmEmail
        [Route("api/Update_Username")]
        [HttpPost]
        public async Task<IActionResult> UpdateUsername([FromBody] Update_Username_Data update_data)
        {

            if (update_data == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {
                try
                {

                    Session_Data session = await Session_Manager.Active_Session(Request);


                    AuthAPI_Update_Username_Data authAPI_update_data = new AuthAPI_Update_Username_Data
                    {
                        SessionData = session,
                        newUsername = update_data.newUsername
                    };


                    using (var httpClient = new HttpClient())
                    {

                        StringContent content = new StringContent(JsonSerializer.Serialize(authAPI_update_data), Encoding.UTF8, "application/json");

                        using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Update_Username", content))
                        {

                            var responseAsString = await authResponse.Content.ReadAsStringAsync();

                            if (responseAsString == null)
                            {
                                throw new Exception("14");//_14_NULL_ERROR
                            }
                            else
                            {

                                Api_Response apiResponse = JsonSerializer.Deserialize<Api_Response>(responseAsString);


                                if (apiResponse != null)
                                {

                                    if (apiResponse.code.Equals("0"))
                                    {
                                        ResponseType responseType = ResponseType.Success;
                                        return Ok(Response_Handler.GetAppResponse(responseType, apiResponse.responseData));
                                    }
                                    else
                                    {
                                        throw new Exception(apiResponse.code);
                                    }

                                }
                                else
                                {
                                    throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                                }

                            }


                        }



                    }



                }
                catch (Exception e)
                {

                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }


            }

        }



        // POST api/ConfirmEmail
        [Route("api/Update_Email")]
        [HttpPost]
        public async Task<IActionResult> UpdateEmail([FromBody] Update_Email_Data update_data)
        {

            if (update_data == null)
            {
                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR
            }
            else
            {

                try
                {

                    Session_Data session = Session_Manager.Get_Cookies(Request);

                    AuthAPI_Update_Email_Data authAPI_update_data = new AuthAPI_Update_Email_Data
                    {
                        SessionData = session,
                        EmailToken = update_data.EmailToken,
                        newEmail = update_data.newEmail,
                    };


                    using (var httpClient = new HttpClient())
                    {

                        StringContent content = new StringContent(JsonSerializer.Serialize(authAPI_update_data), Encoding.UTF8, "application/json");

                        using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Update_Email", content))
                        {

                            var responseAsString = await authResponse.Content.ReadAsStringAsync();

                            if (responseAsString == null)
                            {
                                throw new Exception("14");//_14_NULL_ERROR
                            }
                            else
                            {

                                Api_Response apiResponse = JsonSerializer.Deserialize<Api_Response>(responseAsString);


                                if (apiResponse != null)
                                {

                                    if (apiResponse.code.Equals("0"))
                                    {
                                        ResponseType responseType = ResponseType.Success;
                                        return Ok(Response_Handler.GetAppResponse(responseType, apiResponse.responseData));
                                    }
                                    else
                                    {
                                        throw new Exception(apiResponse.code);
                                    }

                                }
                                else
                                {
                                    throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
                                }

                            }


                        }



                    }


                }
                catch (Exception e)
                {

                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }


            }

        }



    }
}