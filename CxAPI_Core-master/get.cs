using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;


namespace CxAPI_Core
{
    class get
    {
        public bool get_Http(resultClass token, string path)
        {
            token.status = -1;
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;v=1.0");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.bearer_token);
                var response = client.GetAsync(path).Result;
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        if (token.debug && token.verbosity > 0)
                        {
                            Console.WriteLine("Results found for {0}", path);
                        }
                        token.byte_result = response.Content.ReadAsByteArrayAsync().Result;
                        token.op_result = response.Content.ReadAsStringAsync().Result;
                    }
                    else
                    {
                        Console.Error.WriteLine("Failure to find results {0}", path);
                        Console.Error.Write(response);
                    }

                    token.status = 0;
                    return true;
                }
                else
                {
                    Console.Error.Write(response);
                    return false;
                }
            }
            catch (Exception ex)
            {
                token.status = -1;
                token.statusMessage = ex.Message;
            }
            return false;
        }

    }
    class post
    {
        public bool post_Http(resultClass token, string path, object JsonObject)
        {
            token.status = -1;
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json;v=1.0");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.bearer_token);
                var content = new StringContent(JsonConvert.SerializeObject(JsonObject), Encoding.UTF8, "application/json");
                var result = client.PostAsync(path, content).Result;
                if (result != null)
                {
                    if (result.IsSuccessStatusCode)
                    {
                        if (token.debug && token.verbosity > 0)
                        {
                            Console.WriteLine("Results found for {0}", path);
                        }

                        token.op_result = result.Content.ReadAsStringAsync().Result;
                        token.status = 0;
                        return true;
                    }
                    else
                    {
                        Console.Error.WriteLine("Failure to find results {0}", path);
                        Console.Error.Write(result);
                    }
                }
            }
            catch (Exception ex)
            {
                token.status = -1;
                token.statusMessage = ex.Message;
            }
            return false;
        }
    }

}
