using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GratisInc.Tools.FogBugz.WorkingOn
{
    /// <summary>
    /// A utility class for TryGetDescendants that wraps the results and any possible
    /// errors and/or exceptions into one place.
    /// </summary>
    public class XDocumentDescendantsResult
    {
        public IList<XElement> Descendants { get; set; }
        public FogBugzApiError FogBugzError { get; set; }
        public Exception Exception { get; set; }
        public WebException WebException { get { return Exception as WebException; } }
        public Boolean IsFogBugzError { get { return FogBugzError != null; } }
        public Boolean IsWebException { get { return WebException != null; } }
        public Boolean IsException { get { return Exception != null; } }
        public void ShowException(IWin32Window owner, String caption)
        {
            if(IsWebException)
            {
                MessageBox.Show(owner, WebException.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }
    }
}