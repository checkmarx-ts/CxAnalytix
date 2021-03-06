﻿using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CxRestClient.SAST
{
	public class CxTeams
	{
		private static ILog _log = LogManager.GetLogger(typeof(CxTeams));

		private static String URL_SUFFIX = "cxrestapi/auth/teams";

		private CxTeams()
		{ }

		private class TeamReader : IEnumerable<Team>, IEnumerator<Team>
		{

			private JToken _json;
			private JTokenReader _reader;
			internal TeamReader(JToken json)
			{
				_json = json;
				_reader = new JTokenReader(_json);
			}

			private Team _curTeam = null;
			public Team Current => _curTeam;


			object IEnumerator.Current => Current;

			public void Dispose()
			{
				_reader = null;
			}

			public IEnumerator<Team> GetEnumerator()
			{
				return new TeamReader(_json);
			}

			int _arrayPos = 0;
			JArray _teamArray;

			public bool MoveNext()
			{
				if (_reader.CurrentToken == null)
				{
					while (_reader.Read() && _reader.CurrentToken.Type != JTokenType.Array) ;
					if (_reader.CurrentToken == null || _reader.CurrentToken.Type != JTokenType.Array)
						return false;

					_teamArray = (JArray)_reader.CurrentToken;
				}
				else
					_arrayPos++;


				if (!(_arrayPos < _teamArray.Count))
					return false;


				_curTeam = (Team)new JsonSerializer().
					Deserialize(new JTokenReader(_teamArray[_arrayPos]), typeof(Team));

				return true;
			}

			public void Reset()
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new TeamReader(_json);
			}
		}

		[JsonObject(MemberSerialization.OptIn)]
		public class Team
		{
			[JsonProperty(PropertyName = "id")]
			public String TeamId { get; internal set; }
			[JsonProperty(PropertyName = "fullName")]
			public String TeamName { get; internal set; }
		}

		public static IEnumerable<Team> GetTeams(CxRestContext ctx, CancellationToken token)
		{
			return WebOperation.ExecuteGet<TeamReader>(
				ctx.Json.CreateSastClient
				, (response) =>
				{
					using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
					using (var jtr = new JsonTextReader(sr))
					{
						JToken jt = JToken.Load(jtr);
						return new TeamReader(jt);
					}
				}
				, CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX)
				, ctx
				, token);
		}

	}
}
