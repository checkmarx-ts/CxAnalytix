using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CxRestClient.Utility
{
    public class JsonResponseArrayReader<T> : IEnumerable<T>, IEnumerator<T>, IDisposable
    {
        private StreamReader _sr;
        private JsonTextReader _jtr;
        private JToken _jt;
        private JTokenReader _reader;

        public JsonResponseArrayReader(Stream contentStream)
        {
            _sr = new StreamReader(contentStream);
            _jtr = new JsonTextReader(_sr);
            _jt = JToken.Load(_jtr);
            _reader = new JTokenReader(_jt);
        }

        internal JsonResponseArrayReader(JToken json)
        {
            _jt = json;
            _reader = new JTokenReader(_jt);
        }


        private T _cur = default(T);

        public T Current => _cur;

        object IEnumerator.Current => Current;


        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                (_reader as IDisposable).Dispose();
                _reader = null;
            }

            if (_sr != null)
            {
                _sr.Close();
                _sr.Dispose();
                _sr = null;
            }

            if (_jtr != null)
            {
                _jtr.Close();
                _jtr = null;   
            }

        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => this;

        int _arrayPos = 0;
        int _arrayElems = 0;

        public bool MoveNext()
        {

            if (_reader.CurrentToken == null)
            {
                while (_reader.Read() && _reader.CurrentToken.Type != JTokenType.Array);

                if (_reader.CurrentToken == null || _reader.CurrentToken.Type != JTokenType.Array)
                    return false;

                _arrayElems = (_reader.CurrentToken as JArray).Count;
            }
            else
                _arrayPos++;


            if (!(_arrayPos < _arrayElems))
                return false;

            using (var jtr = new JTokenReader(_reader.CurrentToken[_arrayPos]))
                _cur = (T)(new JsonSerializer().Deserialize(jtr, typeof(T)));

            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

    }
}
