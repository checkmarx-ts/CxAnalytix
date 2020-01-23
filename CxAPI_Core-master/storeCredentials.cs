using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAPI_Core
{
    class storeCredentials
    {
        public resultClass save_credentials(resultClass token)
        {
            secure encrypt = new secure(token);
            if (String.IsNullOrEmpty(token.user_name) || String.IsNullOrEmpty(token.credential))
            {
                Console.Error.WriteLine("username and/or credential cannot be empty strings.");
                token.status = -1;
                return token;
            }
            encrypt.encrypt_Credentials();
            Console.WriteLine("Credentials stored successfully.");
            token.status = 0;
            return token;
        }


    }
}
