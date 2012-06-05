using System;

namespace GratisInc.Tools.FogBugz.WorkingOn
{
    /// <summary>
    /// A container class for a FogBugz case.
    /// </summary>
    public class FogBugzCase
    {
        public Int32 Id { get; set; }
        public String Title { get; set; }
        public String FixFor { get; set; }
        public DateTime FixForDate { get; set; }
        public String FixForDateString
        {
            set
            {
                DateTime ffd;
                if (DateTime.TryParse(value, out ffd)) FixForDate = ffd;
                else FixForDate = DateTime.MaxValue;
            }
        }
        public String Project { get; set; }
        public DateTime ResolvedOn { get; set; }
        public Int32 ResolvedBy { get; set; }
        public Int32 ProjectId { get; set; }
        public Int32 Priority { get; set; }
    }
}
