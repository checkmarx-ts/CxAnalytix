using System;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json.Converters;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace CxAPI_Core
{
    public class secure : IDisposable
    {
        public readonly IConfigurationRoot _configuration;
        public CipherService _cipherService;
        public string _os;
        public bool _debug = false;
        public resultClass _token;
        public settingClass _settings;

        public secure()
        {
            _configuration = Configuration.configuration();
            _cipherService = new CipherService();
            _os = RuntimeInformation.OSDescription;
            _settings = get_settings();

        }
        public secure(resultClass token)
        {
            _configuration = Configuration.configuration();
            _cipherService = new CipherService();
            _os = RuntimeInformation.OSDescription;
            _token = token;
            _settings = get_settings();
            _debug = token.debug;
            if (_debug && _token.verbosity > 1) Console.WriteLine("exe path: {0}", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

        }
        public resultClass encrypt_Credentials()
        {
            if (_token == null)
            {
                _token = new resultClass();
            }
            encryptClass decrypt = get_decrypted_file();
            decrypt.user_name = _token.user_name;
            decrypt.credential = _token.credential;
            set_encrypted_file(decrypt);
            return _token;
        }

        public resultClass decrypt_Credentials()
        {
            if (_token == null)
            {
                _token = new resultClass();
            }
            encryptClass decrypt = get_decrypted_file();


            // Decrypt username & credential and print to the console
            _token.user_name = decrypt.user_name;
            _token.credential = decrypt.credential;

            //Console.WriteLine(string.Format("Decrypted username: {0}", token.user_name));
            //Console.WriteLine(string.Format("Decrypted credential: {0}", token.credential));
            return _token;
        }

        public void encrypt_Token(resultClass token)
        {
            encryptClass decrypt = get_decrypted_file();
            decrypt.token = token.bearer_token;
            decrypt.token_creation = token.timestamp.ToString();
            decrypt.token_expires = token.expiration.ToString();
            set_encrypted_file(decrypt);
            return;
        }


        public resultClass decrypt_Token(resultClass token)
        {
            if (token == null)
            {
                token = new resultClass();
            }

            encryptClass decrypt = get_decrypted_file();
            DateTime timestamp = DateTime.MinValue;
            Int32 expiration = 0;

            token.user_name = (decrypt.user_name == null) ? String.Empty : decrypt.user_name;
            token.credential = (decrypt.credential == null) ? String.Empty : decrypt.credential;
            token.bearer_token = (decrypt.token == null) ? String.Empty : decrypt.token;

            Int32.TryParse(decrypt.token_expires, out expiration);
            token.expiration = 60;
            DateTime.TryParse(decrypt.token_creation, out timestamp);
            token.timestamp = timestamp;

            //Console.WriteLine(string.Format("Decrypted username: {0}", token.user_name));
            //Console.WriteLine(string.Format("Decrypted credential: {0}", token.credential));
            //Console.WriteLine(string.Format("Decrypted bearer_token: {0}", token.bearer_token));
            //Console.WriteLine(string.Format("Decrypted token_expires: {0}", token.expiration));
            //Console.WriteLine(string.Format("Decrypted token_creation: {0}", token.timestamp));

            return token;
        }

        public string get_rest_Uri(string op)
        {
            settingClass settings = get_settings();
            string path = settings.CxUrl + op;
            return path;
        }
        public string post_rest_Uri(string op)
        {
            settingClass settings = get_settings();
            string path = settings.CxUrl + op;
            return path;
        }

        public resultClass findToken(resultClass token)
        {
            fetchToken ftoken = new fetchToken();
            secure secure_token = new secure(token);
            //            if (((String.IsNullOrEmpty(token.user_name) || String.IsNullOrEmpty(token.credential))) && (String.IsNullOrEmpty(token.bearer_token)))
            //            {
            // see if token is available in configuration
            token = decrypt_Token(token);
            if (!String.IsNullOrEmpty(token.bearer_token))
            {
                if (token.timestamp.AddMinutes(token.expiration + 1) < DateTime.UtcNow)
                {
                    //token has expired by now. See if credentials were kept
                    Console.Error.WriteLine("Token has expired. Using username and credential");

                    if (String.IsNullOrEmpty(token.user_name) || String.IsNullOrEmpty(token.credential))
                    {
                        Console.Error.WriteLine("No credentials stored or provided. Please provide username and credential");
                        return token;
                    }

                    token = ftoken.get_token(token);
                }
                else
                {
                    token = secure_token.decrypt_Token(token);
                }
            }
            else
            {
                if (String.IsNullOrEmpty(token.user_name) || String.IsNullOrEmpty(token.credential))
                {
                    Console.Error.WriteLine("No credentials stored or provided. Please provide username and credential");
                    return token;
                }
                token = ftoken.get_token(token);
            }
            //           }
            //           else
            //           {
            //                token = ftoken.get_token(token);
            //            }

            return token;
        }
        public settingClass get_settings()
        {
            settingClass _settings = new settingClass();
            _configuration.GetSection("CxRest").Bind(_settings);

            _settings.CxAPIResolver = _settings.CxUrl + _settings.CxAPIResolver;
            _settings.CxSDKWebService = _settings.CxUrl + _settings.CxSDKWebService;
            if (_token != null && _token.debug)
            {
                _settings.debug = "true";
            }

            if (_debug && _token.verbosity > 1)
            {
                foreach (PropertyInfo propertyInfo in _settings.GetType().GetProperties())
                {
                    Console.WriteLine("{0}:{1}", propertyInfo.Name, propertyInfo.GetValue(_settings, null));
                }
            }
            return _settings;
        }
        public bool set_encrypted_file(encryptClass encrypt)
        {
            string folder = _os.Contains("Windows") ? "\\" : "/";
            try
            {
                settingClass settings = get_settings();
                string Json_string = Newtonsoft.Json.JsonConvert.SerializeObject(encrypt);
                string encrypted = _cipherService.Encrypt(Json_string);
                if (_debug && _token.verbosity > 1) Console.WriteLine("Setting file: {0} : {1}", settings.CxDataFilePath, settings.CxDataFileName);
                string path = Path.Combine(settings.CxDataFilePath, settings.CxDataFileName);
                File.WriteAllText(path, encrypted);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex.ToString());
                return false;
            }

        }
        public encryptClass get_decrypted_file()
        {
            try
            {
                if (_debug && _token.verbosity > 1) { Console.WriteLine("Debug settings: {0} : {1}", _settings.CxDataFilePath, _settings.CxDataFileName); }
                string path = Path.Combine(_settings.CxDataFilePath, _settings.CxDataFileName);
                string encrypted = File.ReadAllText(path);
                encryptClass encrypt = Newtonsoft.Json.JsonConvert.DeserializeObject<encryptClass>(_cipherService.Decrypt(encrypted));
                return encrypt;
            }
            catch (Exception ex)
            {
                if (_token.api_action != api_action.storeCredentials)
                {
                    Console.Error.Write(ex.ToString());
                }
                return new encryptClass();
            }

        }
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }

    }
}
