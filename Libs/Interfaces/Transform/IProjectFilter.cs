using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Interfaces.Transform
{
	public interface IProjectFilter
	{
		bool Matches(String teamName, String projectName);
		bool Matches(String projectName);
	}
}
