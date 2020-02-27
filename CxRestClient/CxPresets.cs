using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CxRestClient
{
    public class CxPresets
    {
        private static String URL_SUFFIX = "cxrestapi/sast/presets";

        private CxPresets()
        { }


        public struct Preset
        {
            public int PresetId { get; internal set; }
            public String PresetName { get; internal set; }

            public override string ToString()
            {
                return String.Format("Preset Id: {0} Name: {1}", PresetId, PresetName);
            }
        }

        private class PresetReader : IEnumerable<Preset>, IEnumerator<Preset>
        {

            private JToken _json;
            private JTokenReader _reader;
            internal PresetReader(JToken json)
            {
                _json = json;
                _reader = new JTokenReader(_json);
            }


            public Preset Current => new Preset()
            {
                PresetId = Convert.ToInt32(((JProperty)_reader.CurrentToken).Value.ToString()),
                PresetName = ((JProperty)_reader.CurrentToken.Next).Value.ToString()
            };

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _reader = null;
            }

            public IEnumerator<Preset> GetEnumerator()
            {
                return new PresetReader(_json);
            }

            public bool MoveNext()
            {
                bool read_result = false;

                while (read_result = _reader.Read())
                    if (_reader.CurrentToken.Type == JTokenType.Property)
                        if (((JProperty)_reader.CurrentToken).Name.CompareTo("id") == 0)
                            return true;

                return read_result;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new PresetReader(_json);
            }
        }

        public static IEnumerable<Preset> GetPresets(CxRestContext ctx, CancellationToken token)
        {
            using (var client = ctx.Json.CreateSastClient())
            using (var presets = client.GetAsync(
                CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX), token).Result)
            {
                if (token.IsCancellationRequested)
                    return null;

                if (!presets.IsSuccessStatusCode)
                    throw new InvalidOperationException(presets.ReasonPhrase);

                using (var sr = new StreamReader
                        (presets.Content.ReadAsStreamAsync().Result))
                using (var jtr = new JsonTextReader(sr))
                {
                    JToken jt = JToken.Load(jtr);
                    return new PresetReader(jt);
                }
            }
        }
    }
}
