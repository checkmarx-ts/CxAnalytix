using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.IO
{
    public class LoginToken
    {
        public String TokenType { get; internal set; }
        public DateTime ExpireTime { get; internal set; }
        public String Token { get; internal set; }
        internal HttpContent ReauthContent { get; set; }

        public override string ToString()
        {
            return $"LoginToken: Type: {TokenType} Len: {Token.Length} Expires: {ExpireTime.ToLongDateString()} {ExpireTime.ToLongTimeString()}";
        }
    }
}
