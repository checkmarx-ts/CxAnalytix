using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CxAPI_Core
{
    class fetchToken
    {
        
        public resultClass get_token(resultClass token)
        {
            secure secure_token = new secure(token);
            HttpClient client = new HttpClient();
            string CxUrl = secure_token.post_rest_Uri(CxConstant.CxToken);
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("username", token.user_name));
            nvc.Add(new KeyValuePair<string, string>("password", token.credential));
            nvc.Add(new KeyValuePair<string, string>("grant_type", token.grant_type));
            nvc.Add(new KeyValuePair<string, string>("scope", token.scope));
            nvc.Add(new KeyValuePair<string, string>("client_id", token.client_id));
            nvc.Add(new KeyValuePair<string, string>("client_secret", token.client_secret));
            var request = new HttpRequestMessage(HttpMethod.Post,CxUrl) { Content = new FormUrlEncodedContent(nvc) };
            var response = client.SendAsync(request).Result;
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    if (token.debug)
                    {
                        Console.WriteLine("Token created and stored");
                    }
                    var jsonstring = response.Content.ReadAsStringAsync().Result;
                    resultToken json_response = JsonConvert.DeserializeObject<resultToken>(jsonstring);
                    token.bearer_token = json_response.access_token;
                    token.expiration = Convert.ToInt32(json_response.expires_in);
                    token.timestamp = DateTime.UtcNow;
                    secure_token.encrypt_Token(token);
                    token.status = 0;
                }
                else
                {
                    Console.Error.Write(response);
                    token.status = -1;
                }
                   
            }
            return token;
        }
    }
}
