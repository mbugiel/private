using ManagemateAPI.Controllers;
using System.Text.Json;
using System.Text;
using ManagemateAPI.Management.M_Session.Input_Objects;
using ManagemateAPI.Encryption;
using System.Net;
using Org.BouncyCastle.Asn1.Ocsp;
using ManagemateAPI.Management.Shared.Static;

namespace ManagemateAPI.Management.M_Session.Manager
{
    public static class Session_Manager
    {


        //public static string cookie_key = "GwfTB45McnU98h3vJKx5p6Dz";
        //public static string cookie_iv = "HwfTh85McBo18h3v9FmqL0cA";



        public static async Task<Session_Data> Active_Session(HttpRequest request)
        {

            if (request == null)
            {
                throw new Exception("14");//_14_NULL_ERROR
            }
            else
            {

                Session_Data session = Get_Cookies(request);


                using (var httpClient = new HttpClient())
                {

                    StringContent content = new StringContent(JsonSerializer.Serialize(session), Encoding.UTF8, "application/json");

                    using (var authResponse = await httpClient.PostAsync(System_Path.AUTHENTICATION_API + "Active_Session", content))
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
                                    return session;
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

        }


        public static Session_Data Get_Cookies(HttpRequest request)
        {

            if(request != null)
            {

                byte[] encrypt_key;
                byte[] encrypt_iv;

                try
                {

                    Keys key_pair = Crypto.GetKeys();
                    encrypt_key = key_pair.Key;
                    encrypt_iv = key_pair.Iv;

                }
                catch (Exception)
                {
                    throw new Exception("26");//_26_FILE_READ_ERROR
                }


                Session_Data session = new Session_Data();

                try
                {

                    foreach (var cookie in request.Cookies)
                    {

                        if (cookie.Key.Equals(System_Path.COOKIE_TOKEN_NAME))
                        {

                            session.token = Crypto.SystemDecrypt(
                                Convert.FromBase64String(cookie.Value),
                                encrypt_key,
                                encrypt_iv
                            );

                        }

                        if (cookie.Key.Equals(System_Path.COOKIE_USERID_NAME))
                        {

                            session.userId = Int64.Parse(
                                Crypto.SystemDecrypt(
                                    Convert.FromBase64String(cookie.Value),
                                    encrypt_key,
                                    encrypt_iv
                                )
                            );

                        }

                    }


                }
                catch (Exception)
                {

                    throw new Exception("32");//_32_COOKIE_ACCESS_ERROR
                }


                if (session.token != null && session.userId > 0)
                {

                    return session;

                }
                else
                {
                    throw new Exception("1"); // _1_SESSION_NOT_FOUND
                }



            }
            else
            {
                throw new Exception("14");// null error
            }

        }


        public static void Set_Cookies(Session_Data session_data, HttpResponse response)
        {


            if (session_data != null && response != null)
            {

                byte[] encrypt_key;
                byte[] encrypt_iv;

                try
                {
                       
                    Keys key_pair = Crypto.GetKeys();
                    encrypt_key = key_pair.Key;
                    encrypt_iv = key_pair.Iv;

                }
                catch (Exception) 
                {
                    throw new Exception("26");//_26_FILE_READ_ERROR
                }


                byte[] encrypted_token;
                byte[] encrypted_userId;

                try
                {

                    encrypted_token = Crypto.SystemEncrypt(session_data.token, encrypt_key, encrypt_iv);

                    encrypted_userId = Crypto.SystemEncrypt(session_data.userId.ToString(), encrypt_key, encrypt_iv);

                }
                catch (Exception)
                {

                    throw new Exception("2");//_2_ENCRYPTION_ERROR
                }


                try
                {

                    response.Cookies.Append(System_Path.COOKIE_TOKEN_NAME, Convert.ToBase64String(encrypted_token),
                        new CookieOptions
                        {
                            Expires = DateTime.UtcNow + TimeSpan.FromDays(7),
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Extensions ={ "Partitioned" }
                        }
                    );


                    response.Cookies.Append(System_Path.COOKIE_USERID_NAME, Convert.ToBase64String(encrypted_userId),
                        new CookieOptions
                        {
                            Expires = DateTime.UtcNow + TimeSpan.FromDays(7),
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Extensions ={ "Partitioned" }
                        }
                    );

                }
                catch (Exception)
                {

                    throw new Exception("32");//_32_COOKIE_ACCESS_ERROR
                }

            }
            else
            {
                throw new Exception("16");//_16_SERVER_RESPONSE_ERROR
            }

        }


        public static void Delete_Cookies(HttpResponse response)
        {

            if(response != null)
            {

                try
                {
                    response.Cookies.Delete(System_Path.COOKIE_TOKEN_NAME,
                        new CookieOptions()
                        {
                            Secure = true,
                            Extensions ={ "Partitioned" }
                        }
                    );

                    response.Cookies.Delete(System_Path.COOKIE_USERID_NAME,
                        new CookieOptions()
                        {
                            Secure = true,
                            Extensions ={ "Partitioned" }
                        }
                    );

                }
                catch (Exception)
                {

                    throw new Exception("32");//_32_COOKIE_ACCESS_ERROR
                }


            }
            else
            {

                throw new Exception("16");//_16_SERVER_RESPONSE_ERROR

            }

        }


    }
}
