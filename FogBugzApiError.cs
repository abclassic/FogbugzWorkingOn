using System;
using System.Windows.Forms;

namespace GratisInc.Tools.FogBugz.WorkingOn
{
    /// <summary>
    /// A container class for FogBugz API errors.
    /// </summary>
    public class FogBugzApiError
    {
        public Int32 Code { get; set; }
        public String Message { get; set; }
        public void Show(IWin32Window owner)
        {
            MessageBox.Show(owner, Message, String.Format("FogBugz API Error Code {0}", Code), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}