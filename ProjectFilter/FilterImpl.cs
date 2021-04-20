using CxAnalytix.Interfaces.Transform;
using System;
using System.Text.RegularExpressions;

namespace ProjectFilter
{
	public class FilterImpl : IProjectFilter
	{
		public FilterImpl(String teamRegex, String projectRegex)
		{
			if (teamRegex != null)
				Team = new Regex(teamRegex);
			if (projectRegex != null)
				Project = new Regex(projectRegex);
		}

		public Regex Team { get; private set; }
		public Regex Project { get; private set; }


		private bool Matches (Regex regex, String matchValue)
		{
			if (regex == null)
				return true;
			else if (matchValue == null)
				return false;

			return regex.IsMatch(matchValue);
		}

		public bool Matches(string teamName, string projectName)
		{
			return Matches(Team, teamName) && Matches(Project, projectName);
		}
	}
}
