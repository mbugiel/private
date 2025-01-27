using ManagemateAPI.Controllers;
using ManagemateAPI.Mail;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using ManagemateAPI.Mail.Input_Objects;
using ManagemateAPI.Management.Shared.Static;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?Linkid=397860

namespace ManagemateAPI.Controllers.Authentication
{

    [ApiController]
    public class Send_EmailController : ControllerBase
    {

        private readonly IMailService _mailService;
        public Send_EmailController(IMailService _MailService)
        {
            _mailService = _MailService;
        }


        // POST api/Users
        [Route("api/Send_Email")]
        [HttpPost]
        public async Task<IActionResult> SendEmail([FromBody] Send_Email_Data userData)
        {
            if (userData == null)
            {

                return BadRequest(Response_Handler.GetExceptionResponse(new Exception("14")));//_14_NULL_ERROR

            }
            else
            {

                try
                {

                    string code;

                    using (var httpClient = new HttpClient())
                    {

                        Save_Confirmation_Code_Data model = new Save_Confirmation_Code_Data
                        {
                            email = userData.Email,
                            validationCode = Info.VALidATION_CODE
                        };

                        StringContent content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                        using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Save_Code", content))
                        {

                            var resp = await authResponse.Content.ReadAsStringAsync();

                            if (resp == null)
                            {
                                throw new Exception("14");//_14_NULL_ERROR
                            }
                            else
                            {

                                Api_Response apiResponse = JsonSerializer.Deserialize<Api_Response>(resp);


                                if (apiResponse != null)
                                {

                                    if (apiResponse.code.Equals("0"))
                                    {
                                        code = apiResponse.responseData.ToString();
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


                    if (code == null)
                    {
                        throw new Exception("14");//_14_NULL_ERROR
                    }

                    MailData mailData = new MailData
                    {
                        EmailToid = userData.Email,
                        EmailToName = userData.Username,
                        EmailCode = code,
                        EmailTemplate = userData.Template

                    };

                    var response = await _mailService.SendMailAsync(mailData);

                    if (response == null)
                    {
                        throw new Exception("14");//_14_NULL_ERROR
                    }

                    ResponseType responseType = ResponseType.Success;

                    return Ok(Response_Handler.GetAppResponse(responseType, response));

                }
                catch (Exception e)
                {

                    return BadRequest(Response_Handler.GetExceptionResponse(e));
                }

            }

        }



    }
}
