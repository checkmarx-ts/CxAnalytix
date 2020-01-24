using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytics.TransformLogic
{
    internal class ProjectDescriptorExt : ProjectDescriptor
    {

        internal static ProjectDescriptorExt newDetail(ProjectDescriptor src)
        {
            return new ProjectDescriptorExt()
            {
                PresetId = src.PresetId,
                PresetName = src.PresetName,
                ProjectId = src.ProjectId,
                ProjectName = src.ProjectName,
                TeamId = src.TeamId,
                TeamName = src.TeamName,
                LastScanCheckDate = DateTime.MinValue
            };

        }

        public DateTime LastScanCheckDate { get; set; }
    }
}
