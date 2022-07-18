using System;


namespace SDK.Modules.Transformer.Data
{
    public class ProjectDescriptorExt : ProjectDescriptor
    {
        internal ProjectDescriptorExt (ProjectDescriptor src, ProjectDescriptorExt merge) : base (src)
        {
            this.LastScanCheckDate = merge.LastScanCheckDate;
        }

        public ProjectDescriptorExt ()
        {

        }

        internal ProjectDescriptorExt(ProjectDescriptor src) : base(src)
        {
            LastScanCheckDate = DateTime.MinValue;
        }


        internal static ProjectDescriptorExt newDetail(ProjectDescriptor src)
        {
            return new ProjectDescriptorExt(src)
            {
                LastScanCheckDate = DateTime.MinValue
            };

        }

        public DateTime LastScanCheckDate { get; set; }
    }
}
