using System;
using System.Collections.Generic;



namespace CxAPI_Core
{

    internal class Start
    {
        private static void Main(string[] args)
        {
            try
            {
                Configuration.configuration(args);
                dispatcher dsp = new dispatcher();

                resultClass token = dsp.dispatch();
                Console.WriteLine("CxAPI_Core processing completed {0}", DateTime.UtcNow);
                if (_options.debug)
                {
                    Console.ReadKey();
                }

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                if (_options.debug)
                {
                    Console.ReadKey();
                }
            }
        }
    }

}
