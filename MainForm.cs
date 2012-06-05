using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace GratisInc.Tools.FogBugz.WorkingOn
{
   public partial class MainForm : Form
   {
      private const String SERVER_PATH_EXPRESSION = @"^(?:(?<protocol>http[s]?)://)?(?<server>[\w-_\.]+)(?:\:(?<port>\d{1,5}))?/?(?<path>[\w-_/%]+?)?/?$";
      private Regex reServerPath = new Regex(SERVER_PATH_EXPRESSION);
      private String serverUrl;
      private String commandScript;

      private FogBugzCase lastWorkedCase;
      private FogBugzCase workingCase;
      private List<FogBugzCase> cases;
      private List<FogBugzCase> caseHistory;
      private List<FogBugzCase> recentCases;
      private DateTime startedOn;

      public MainForm()
      {
         InitializeComponent();

         Font = SystemFonts.DialogFont;

         tbManualCase.TextBox.LostFocus += new EventHandler(TextBox_LostFocus);

         // Position the form to the bottom right corner.
         Int32 x = SystemInformation.WorkingArea.Right - this.Width - SystemInformation.VerticalResizeBorderThickness;
         Int32 y = SystemInformation.WorkingArea.Bottom - this.Height - SystemInformation.HorizontalResizeBorderThickness;
         this.SetDesktopLocation(x, y);

         // Load any saved settings into the form.
         tbServer.Text = Settings.Default.Server;
         cbSSL.Checked = Settings.Default.UseSSL;
         tbUser.Text = Settings.Default.User;
         tbPassword.Text = Settings.Default.Password;

         // If the settings are all present, automatically initiate the logon process.
         if (reServerPath.IsMatch(tbServer.Text) && !String.IsNullOrEmpty(tbUser.Text) && !String.IsNullOrEmpty(tbPassword.Text))
         {
            Logon();
         }
         else if (!reServerPath.IsMatch(tbServer.Text))
         {
            MessageBox.Show(this, "Could not determine the FogBugz server and/or path to use. Please check your input and try again.", "Invalid server or path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
         }
         else if (tbUser.Text == String.Empty)
         {
            MessageBox.Show(this, "Please enter your user name.", "Authentication error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
         }
         else if (tbPassword.Text == String.Empty)
         {
            MessageBox.Show(this, "Please enter your password.", "Authentication error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
         }

         //test
         startTimer();
      }

      #region [ FogBugz API Calls ]

      /// <summary>
      /// Initiates the logon process. If it is successful, the form values
      /// are saved to the application settings.
      /// </summary>
      private void Logon()
      {
         serverUrl = null;
         commandScript = null;

         // Parse the input controls to get the protocol, server and path
         Match match = reServerPath.Match(tbServer.Text.ToLower());
         var serverInfo = new
         {
            // If the user checked the ssl checkbox, use https, otherwise get the value from
            // the server textbox, defaulting to http if the user did not specify a protocol
            Protocol = cbSSL.Checked ? "https" :
                (match.Groups["protocol"].Success ? match.Groups["protocol"].Value : "http"),
            Server = match.Groups["server"].Value,
            Port = match.Groups["port"].Success ? match.Groups["port"].Value : String.Empty,
            Path = match.Groups["path"].Success ? match.Groups["path"].Value : String.Empty
         };

         // Build the server url
         serverUrl = String.Format("{0}://{1}{2}/{3}",
             serverInfo.Protocol,
             serverInfo.Server,
             String.IsNullOrEmpty(serverInfo.Port) ? String.Empty : String.Format(":{0}", serverInfo.Port),
             String.IsNullOrEmpty(serverInfo.Path) ? String.Empty : String.Format("{0}/", serverInfo.Path));

         // Get the command url from the api.xml document
         try
         {
            XDocument apiDoc = LoadDoc(String.Format("{0}api.xml", serverUrl));
            XDocumentDescendantsResult apiResult;
            FogBugzApiError error;
            if (apiDoc.IsFogBugzError(out error))
            {
               error.Show(this);
            }
            else if (apiDoc.TryGetDescendants("response", out apiResult))
            {
               commandScript = apiResult.Descendants.First().Element("url").Value;
            }
         }
         catch (Exception ex)
         {
            MessageBox.Show(this, String.Format("Error connecting to server: {0}. Please check your connection and try again.", ex.Message), "Server error", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }

         if (!String.IsNullOrEmpty(commandScript))
         {
            XDocument doc = LoadDoc(String.Format("{0}{1}cmd=logon&email={2}&password={3}", serverUrl, commandScript, tbUser.Text, tbPassword.Text));
            XDocumentDescendantsResult result;
            if (doc.TryGetDescendants("token", out result))
            {
               if (result.Descendants.Count() == 1)
               {
                  Settings.Default.Server = String.Format("{0}{1}{2}",
                      serverInfo.Server,
                      String.IsNullOrEmpty(serverInfo.Port) ? String.Empty : String.Format(":{0}", serverInfo.Port),
                      String.IsNullOrEmpty(serverInfo.Path) ? String.Empty : String.Format("/{0}", serverInfo.Path));
                  Settings.Default.UseSSL = cbSSL.Checked || serverInfo.Protocol == "https";
                  Settings.Default.User = tbUser.Text;
                  Settings.Default.Password = tbPassword.Text;
                  Settings.Default.Token = result.Descendants.First<XElement>().Value;
                  Settings.Default.Save();

                  tbServer.Text = Settings.Default.Server;
                  cbSSL.Checked = Settings.Default.UseSSL;

                  HideForm();
                  tray.ShowBalloonTip(0, String.Format("Logged into {0}", serverInfo.Server), "Now get crackin!", ToolTipIcon.Info);
                  updateTimer.Start();
                  UpdateName();
                  UpdateFogBugzData();
               }
            }
            else if (result.IsFogBugzError) result.FogBugzError.Show(this);
            else if (result.IsWebException) result.ShowException(this, String.Format("An error occurred while attempting to contact {0}", Settings.Default.Server));
            else if (result.IsException) result.ShowException(this, "An error occurred");
         }
      }

      /// <summary>
      /// Updates the user's full name from FogBugz, which is used when listing cases.
      /// </summary>
      private void UpdateName()
      {
         if (IsLoggedIn)
         {
            XDocument doc = LoadDoc(GetCommandUrlWithToken("cmd=viewPerson"));
            XDocumentDescendantsResult result;
            if (doc.TryGetDescendants("person", out result))
            {
               IEnumerable<String> results = (
                   from p in result.Descendants
                   select p.Element("sFullName").Value);
               if (results.Count() == 1)
               {
                  Settings.Default.Name = results.ToArray()[0];
                  Settings.Default.Save();
               }
               else
               {
                  MessageBox.Show("Invalid response from viewPerson. The application will now exit.", "Error updating from FogBugz", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  Application.Exit();
               }
            }
            else if (result.IsFogBugzError) result.FogBugzError.Show(this);
            else if (result.IsWebException) result.ShowException(this, String.Format("An error occurred while attempting to contact {0}", Settings.Default.Server));
            else if (result.IsException) result.ShowException(this, "An error occurred");
         }
      }

      /// <summary>
      /// Updates an internal list of cases that the user has resolved in the past week,
      /// which assists in determining which cases are displayed in the menu.
      /// </summary>
      private Boolean UpdateCaseHistory()
      {
         if (IsLoggedIn)
         {
            String command = String.Format("cmd=search&q=resolvedby:\"{0}\" resolved:\"{1}-{2}\"&cols=ixProject,sFixFor", Settings.Default.Name, DateTime.Now.AddDays(-7).ToShortDateString().Replace("-", "/"), DateTime.Now.ToShortDateString().Replace("-", "/"));
            XDocument doc = LoadDoc(GetCommandUrlWithToken(command));
            XDocumentDescendantsResult result;
            if (doc.TryGetDescendants("case", out result))
            {
               caseHistory = new List<FogBugzCase>((
                   from c in result.Descendants
                   select new FogBugzCase
                   {
                      ProjectId = (int)c.Element("ixProject"),
                      FixFor = (string)c.Element("sFixFor")
                   }));
               return true;
            }
            else if (result.IsFogBugzError) result.FogBugzError.Show(this);
            else if (result.IsWebException) result.ShowException(this, String.Format("An error occurred while attempting to contact {0}", Settings.Default.Server));
            else if (result.IsException) result.ShowException(this, "An error occurred");
         }
         return false;
      }

      /// <summary>
      /// Updates the internal working case record.
      /// </summary>
      private Boolean UpdateWorkingCase()
      {
         if (IsLoggedIn)
         {
            XDocument doc = LoadDoc(GetCommandUrlWithToken("cmd=listIntervals"));
            XDocumentDescendantsResult result;
            if (doc.TryGetDescendants("interval", out result))
            {
               // Get the current working case
               IEnumerable<FogBugzCase> cases = (
                   from c in result.Descendants
                   where c.Element("dtEnd").Value == ""
                   select new FogBugzCase
                   {
                      Id = Int32.Parse(c.Element("ixBug").Value),
                      Title = c.Element("sTitle").Value
                   });
               if (cases.Count() > 0) workingCase = cases.First();
               else workingCase = null;

               // Get the most recently worked on case
               cases = (
                   from c in result.Descendants
                   orderby c.Element("dtEnd").Value == "" ? DateTime.MinValue : DateTime.Parse(c.Element("dtEnd").Value) descending
                   select new FogBugzCase
                   {
                      Id = Int32.Parse(c.Element("ixBug").Value),
                      Title = c.Element("sTitle").Value
                   });
               if (cases.Count() > 0) lastWorkedCase = cases.First();
               else lastWorkedCase = null;

               if (workingCase == null)
               {
                  tray.Text = "Not working on anything.";
                  if (lastWorkedCase == null)
                  {
                     stopWorkToolStripMenuItem.Text = "&Stop Work";
                     stopWorkToolStripMenuItem.Enabled = false;
                  }
                  else
                  {
                     stopWorkToolStripMenuItem.Text = String.Format("&Resume Work on Case {0}", lastWorkedCase.Id);
                     stopWorkToolStripMenuItem.Enabled = true;
                  }
               }
               else
               {
                  tray.Text = String.Format("Working on Case {0} - {1}.", workingCase.Id, workingCase.Title).TruncateByLetters(64);
                  stopWorkToolStripMenuItem.Text = String.Format("&Stop Work on Case {0}", workingCase.Id);
                  stopWorkToolStripMenuItem.Enabled = true;
               }
               return true;
            }
            else if (result.IsFogBugzError) result.FogBugzError.Show(this);
            else if (result.IsWebException) result.ShowException(this, String.Format("An error occurred while attempting to contact {0}", Settings.Default.Server));
            else if (result.IsException) result.ShowException(this, "An error occurred");
         }
         return false;
      }

      /// <summary>
      /// Updates an internal list of cases the user is likely to be working on.
      /// </summary>
      private Boolean UpdateCases()
      {
         if (IsLoggedIn)
         {
            XDocument doc = LoadDoc(GetCommandUrlWithToken(String.Format("cmd=search&q=assignedto:%22{0}%22%20status:active&cols=sTitle,ixProject,sFixFor,sProject,dtFixFor,ixPriority", System.Web.HttpUtility.UrlEncode(Settings.Default.Name).Replace("+", "%20"))));
            XDocumentDescendantsResult result;
            if (doc.TryGetDescendants("case", out result))
            {
               // Get a list of recent project ids
               List<Int32> recentProjects = new List<Int32>((
                   from p in caseHistory.GroupBy<FogBugzCase, Int32>(p => p.ProjectId)
                   select p.Key));

               // Get a list of recent Fix Fors
               List<FogBugzFixFor> recentFixFors = new List<FogBugzFixFor>();
               foreach (Int32 projectId in recentProjects)
               {
                  recentFixFors.AddRange((
                      from f in caseHistory.Where<FogBugzCase>(f => f.ProjectId == projectId).GroupBy<FogBugzCase, String>(f => f.FixFor)
                      select new FogBugzFixFor
                      {
                         ProjectId = projectId,
                         Name = f.Key
                      }));
               }

               cases = new List<FogBugzCase>();
               // Get the 10 most recently submitted cases.
               cases.AddRange((
                   from c in result.Descendants
                   orderby Int32.Parse(c.Attribute("ixBug").Value) descending
                   select new FogBugzCase
                   {
                      Id = Int32.Parse(c.Attribute("ixBug").Value),
                      Title = c.Element("sTitle").Value,
                      Project = c.Element("sProject").Value,
                      ProjectId = Int32.Parse(c.Element("ixProject").Value),
                      FixFor = c.Element("sFixFor").Value,
                      FixForDateString = c.Element("dtFixFor").Value,
                      Priority = Int32.Parse(c.Element("ixPriority").Value)
                   }
                   ).Take(10));

               // Get cases based on recent case resolution actions.
               cases.AddRange((
                   from c in doc.Descendants("case")
                   where recentProjects.Contains(Int32.Parse(c.Element("ixProject").Value))
                       && recentFixFors.Where<FogBugzFixFor>(f => f.ProjectId == Int32.Parse(c.Element("ixProject").Value)).Where<FogBugzFixFor>(f => f.Name == c.Element("sFixFor").Value).Count() > 0
                       && cases.Where<FogBugzCase>(ec => ec.Id == Int32.Parse(c.Attribute("ixBug").Value)).Count() == 0
                   select new FogBugzCase
                   {
                      Id = Int32.Parse(c.Attribute("ixBug").Value),
                      Title = c.Element("sTitle").Value,
                      Project = c.Element("sProject").Value,
                      ProjectId = Int32.Parse(c.Element("ixProject").Value),
                      FixFor = c.Element("sFixFor").Value,
                      FixForDateString = c.Element("dtFixFor").Value,
                      Priority = Int32.Parse(c.Element("ixPriority").Value)
                   }));

               return true;
            }
            else if (result.IsFogBugzError) result.FogBugzError.Show(this);
            else if (result.IsWebException) result.ShowException(this, String.Format("An error occurred while attempting to contact {0}", Settings.Default.Server));
            else if (result.IsException) result.ShowException(this, "An error occurred");
         }
         return false;
      }

      private Boolean UpdateRecentCases()
      {
         if (IsLoggedIn)
         {
            XDocument intDoc = LoadDoc(GetCommandUrlWithToken("cmd=listIntervals&dtStart={0}", DateTime.Now.AddDays(-14).ToShortDateString().Replace("/", "-")));
            XDocument caseDoc = LoadDoc(GetCommandUrlWithToken("cmd=search&q=assignedto:%22{0}%22%20status:active&cols=sTitle,ixProject,sFixFor,sProject,dtFixFor,ixPriority", HttpUtility.UrlEncode(Settings.Default.Name).Replace("+", "%20")));
            XDocumentDescendantsResult intResult;
            XDocumentDescendantsResult caseResult;
            Boolean intSuccess = intDoc.TryGetDescendants("interval", out intResult);
            Boolean caseSuccess = caseDoc.TryGetDescendants("case", out caseResult);

            if (intSuccess && caseSuccess)
            {
               var recent = (
                   from i in intResult.Descendants
                   where !String.IsNullOrEmpty(i.Element("dtEnd").Value)
                   group i by i.Element("ixBug").Value into intervalCases
                   select new
                   {
                      Id = intervalCases.Key,
                      LastAction = intervalCases.Max(d => DateTime.Parse(d.Element("dtEnd").Value))
                   }).ToList();

               // Get the current working case
               recentCases = (
                   from r in recent
                   join c in caseResult.Descendants on r.Id equals c.Attribute("ixBug").Value
                   select new FogBugzCase
                   {
                      Id = Int32.Parse(c.Attribute("ixBug").Value),
                      Title = c.Element("sTitle").Value,
                      FixFor = c.Element("sFixFor").Value,
                      Project = c.Element("sProject").Value,
                      ProjectId = Int32.Parse(c.Element("ixProject").Value),
                      Priority = Int32.Parse(c.Element("ixPriority").Value),
                      ResolvedOn = r.LastAction
                   }).ToList();
               return true;
            }
            else if (intResult.IsFogBugzError) intResult.FogBugzError.Show(this);
            else if (intResult.IsWebException) intResult.ShowException(this, String.Format("An error occurred while attempting to contact {0}", Settings.Default.Server));
            else if (intResult.IsException) intResult.ShowException(this, "An error occurred");
            else if (caseResult.IsFogBugzError) caseResult.FogBugzError.Show(this);
            else if (caseResult.IsWebException) caseResult.ShowException(this, String.Format("An error occurred while attempting to contact {0}", Settings.Default.Server));
            else if (caseResult.IsException) caseResult.ShowException(this, "An error occurred");
         }
         return false;
      }

      /// <summary>
      /// Attempts to start the time tracking timer for the specified case.
      /// </summary>
      /// <param name="caseId">The FogBugz ID of the case to start work on.</param>
      /// <param name="error">The FogBugz error, if any occurs.</param>
      /// <returns>A Boolean value specifying whether the API call to start the time tracking timer succeeded.</returns>
      private Boolean TryStartWork(Int32 caseId, out FogBugzApiError error)
      {
         if (IsLoggedIn)
         {
            XDocument doc = LoadDoc(GetCommandUrlWithToken(String.Format("cmd=startWork&ixBug={0}", caseId)));
            if (doc.IsFogBugzError(out error))
            {
               startedOn = DateTime.MinValue;
               if (error.Code == 7)
               {
                  this.OpenUrl(String.Format("{0}/?pg=pgEditBug&ixBug={1}&command=edit", serverUrl, caseId));
               }
               return false;
            }
            else
            {
               startedOn = DateTime.Now;
               UpdateFogBugzData();
               tray.ShowBalloonTip(0, String.Format("Work started on Case {0}", caseId), workingCase.Title, ToolTipIcon.Info);
               return true;
            }
         }
         else
         {
            error = null;
            return false;
         }
      }

      /// <summary>
      /// Starts the time tracking timer for the specified case.
      /// </summary>
      /// <param name="caseId">The FogBugz ID of the case to start work on.</param>
      private void StartWork(Int32 caseId)
      {
         if (IsLoggedIn)
         {
            FogBugzApiError error;
            TryStartWork(caseId, out error);
         }
      }

      /// <summary>
      /// Stops the time tracking timer for the case currently in progress.
      /// </summary>
      private void StopWork()
      {
         if (IsLoggedIn)
         {
            XDocument doc = LoadDoc(GetCommandUrlWithToken("cmd=stopWork"));
            FogBugzApiError error;
            Int32 totalMinutes = (Int32)Math.Round(((TimeSpan)(DateTime.Now - startedOn)).TotalMinutes);
            String title = String.Format("Work stopped on Case {0}.", workingCase.Id);
            String text = workingCase.Title + Environment.NewLine + String.Format("{0} minute(s) logged.", totalMinutes);
            if (doc.IsFogBugzError(out error)) error.Show(this);
            else
            {
               tray.ShowBalloonTip(0, title, text, ToolTipIcon.Info);
               UpdateFogBugzData();
            }
         }
      }

      #endregion

      #region [ Event Handlers ]

      /// <summary>
      /// Handles the click event of any case menu item.
      /// </summary>
      private void Case_Click(Object sender, EventArgs e)
      {
         ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;

         // If the menu item was checked, stop the work.
         if (menuItem.Checked) StopWork();
         // Otherwise, start the work.
         else
         {
            Int32 caseId = (Int32)menuItem.Tag;
            FogBugzApiError error;
            if (!TryStartWork(caseId, out error)) error.Show(this);
         }
      }

      /// <summary>
      /// Handles the click event of the form's OK button.
      /// </summary>
      private void btnOk_Click(object sender, EventArgs e)
      {
         Logon();
      }


      private void btnCancel_Click(object sender, EventArgs e)
      {
         HideForm();
      }

      /// <summary>
      /// Handles the click event of the "Log In" menu item.
      /// </summary>
      private void logInToolStripMenuItem_Click(object sender, EventArgs e)
      {
         ShowForm();
      }

      /// <summary>
      /// Handles the form's Closing event and forces the form to minimize
      /// instead of close when the "x" button is clicked on the toolbar.
      /// </summary>
      private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
      {
         if (e.CloseReason == CloseReason.UserClosing)
         {
            e.Cancel = true;
            HideForm();
         }
      }

      /// <summary>
      /// Handles the click event of the Exit menu item and causes the
      /// application to exit.
      /// </summary>
      private void exitToolStripMenuItem_Click(object sender, EventArgs e)
      {
         Application.Exit();
      }

      /// <summary>
      /// Handles the click event of the Stop Work menu item.
      /// </summary>
      private void stopWorkToolStripMenuItem_Click(object sender, EventArgs e)
      {
         if (workingCase == null && lastWorkedCase != null) StartWork(lastWorkedCase.Id);
         else if (workingCase != null) StopWork();
      }

      /// <summary>
      /// Handles the Tick event of the form's updateTimer component, which
      /// periodically updates data from the FogBugz API.
      /// </summary>
      private void updateTimer_Tick(object sender, EventArgs e)
      {
         UpdateFogBugzData();
      }

      /// <summary>
      /// Handles the KeyPress event of the manual case entry textbox in the menu.
      /// </summary>
      private void tbManualCase_KeyPress(object sender, KeyPressEventArgs e)
      {
         if (e.KeyChar == (Char)Keys.Enter)
         {
            Int32 caseId;
            if (Int32.TryParse(tbManualCase.Text, out caseId))
            {
               menu.Visible = false;
               tbManualCase.Text = String.Empty;
               FogBugzApiError error;
               if (!TryStartWork(caseId, out error))
               {
                  tbManualCase.Text = caseId.ToString();
                  tbManualCase.SelectAll();
                  error.Show(this);
               }
            }
            else MessageBox.Show(this, "Invalid Entry", "You value you entered is not a valid case number.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            e.Handled = true;
         }
      }

      /// <summary>
      /// Handles the enter event of the manual case entry textbox, and causes
      /// the default text and styling in the textbox to vanish when the user enters it.
      /// </summary>
      private void tbManualCase_Enter(object sender, EventArgs e)
      {
         if (tbManualCase.Text == "Case #")
         {
            tbManualCase.Clear();
            tbManualCase.Font = new Font(tbManualCase.Font, FontStyle.Regular);
            tbManualCase.ForeColor = Color.FromKnownColor(KnownColor.WindowText);
         }
      }

      /// <summary>
      /// Handles the lost focus event of the manual case entry textbox and
      /// causes the default text and styling to reappear if the textbox is empty
      /// when the user leaves it.
      /// </summary>
      void TextBox_LostFocus(object sender, EventArgs e)
      {
         if (tbManualCase.Text == String.Empty)
         {
            tbManualCase.TextBox.Text = "Case #";
            tbManualCase.Font = new Font(tbManualCase.Font, FontStyle.Italic);
            tbManualCase.ForeColor = Color.Gray;
         }
      }

      /// <summary>
      /// Handles the click event of the menu. This is used as a workaround to
      /// cause the manual case entry textbox to lose focus when the user clicks
      /// elsewhere.
      /// </summary>
      private void menu_Click(object sender, EventArgs e)
      {
         menu.Focus();
      }

      /// <summary>
      /// Handles the click event of the refresh menu item, which updates
      /// all the case data.
      /// </summary>
      private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
      {
         UpdateFogBugzData();
      }

      /// <summary>
      /// Handles the doubleclick event of the system tray icon, which
      /// opens the FogBugz server url in the user's browser.
      /// </summary>
      private void tray_MouseDoubleClick(object sender, MouseEventArgs e)
      {
         if (!String.IsNullOrEmpty(Settings.Default.Server))
         {
            this.OpenUrl(String.Format("{0}{1}", serverUrl, workingCase == null ? String.Empty : String.Format("?{0}", workingCase.Id)));
         }
      }

      /// <summary>
      /// Handles the click event of the about menu item, which displays a modal dialog
      /// with version and other information.
      /// </summary>
      private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
      {
         new AboutBox().ShowDialog(this);
      }

      #endregion

      #region [ Utility Methods ]

      /// <summary>
      /// Attempts to load an XML document. If there is an exception, the method
      /// returns a document with an error response.
      /// </summary>
      private XDocument LoadDoc(String format, params Object[] args)
      {
         String url = String.Format(format, args);
         try { return XDocument.Load(url); }
         catch (Exception ex)
         {
            String errorXml = String.Format("<error code=\"-1\">{0}</error>",
                String.Format("An error occurred while connecting to {0}: {1}{2}", HttpUtility.HtmlEncode(url), ex.Message, ex.Message.EndsWith(".") ? String.Empty : "."));
            return XDocument.Parse(errorXml);
         }
      }

      /// <summary>
      /// Gets the FogBugz api request uri with the given command and the current session's authentication token.
      /// </summary>
      private String GetCommandUrlWithToken(String commandFormat, params Object[] args)
      {
         String command = String.Format(commandFormat, args);
         return String.Format("{0}{1}{2}&token={3}", serverUrl, commandScript, command, Settings.Default.Token);
      }

      /// <summary>
      /// Adds a new menu item to the given menu item.
      /// </summary>
      /// <param name="parent">The menu item to add the child to.</param>
      /// <param name="text">The text of the menu item.</param>
      private ToolStripMenuItem AddMenuItem(ToolStripMenuItem parent, String text)
      {
         return AddMenuItem(parent, null, text, null, false);
      }
      /// <summary>
      /// Adds a new menu item to the given menu item.
      /// </summary>
      /// <param name="parent">The menu item to add the child to.</param>
      /// <param name="tag">Any relevant data, such as an id.</param>
      /// <param name="text">The text of the menu item.</param>
      /// <param name="clickHandler">The click handler for the menu item.</param>
      /// <param name="isSelected">Whether or not the item should be checked.</param>
      private ToolStripMenuItem AddMenuItem(ToolStripMenuItem parent, Object tag, String text, EventHandler clickHandler, Boolean isSelected)
      {
         ToolStripMenuItem menuItem = new ToolStripMenuItem();
         menuItem.Tag = tag;
         menuItem.Text = text.Replace("&", "&&"); // don't interpret & as accelerator prefix
         if (clickHandler != null)
            menuItem.Click += clickHandler;
         menuItem.Checked = isSelected;
         if (isSelected) menuItem.Font = new Font(menuItem.Font, FontStyle.Bold);
         parent.DropDownItems.Add(menuItem);
         return menuItem;
      }

      /// <summary>
      /// Normalizes and shows the logon form.
      /// </summary>
      private void ShowForm()
      {
         this.Visible = true;
         this.ShowInTaskbar = true;
         this.WindowState = FormWindowState.Normal;
      }

      /// <summary>
      /// Minimizes and hides the logon form.
      /// </summary>
      private void HideForm()
      {
         this.WindowState = FormWindowState.Minimized;
         this.ShowInTaskbar = false;
         this.Visible = false;
      }

      /// <summary>
      /// Gets whether the user has a valid session.
      /// </summary>
      private Boolean IsLoggedIn
      {
         get { return !String.IsNullOrEmpty(Settings.Default.Token); }
      }

      /// <summary>
      /// Calls all methods that update case and case-related data.
      /// </summary>
      private void UpdateFogBugzData()
      {
         Boolean continueUpdates = true;
         if (continueUpdates) continueUpdates = UpdateCaseHistory();
         if (continueUpdates) continueUpdates = UpdateWorkingCase();
         if (continueUpdates) continueUpdates = UpdateMenu();
         updateTimer.Enabled = continueUpdates;
      }

      /// <summary>
      /// Forces an update of case data and updates the menu's lists of cases.
      /// </summary>
      private Boolean UpdateMenu()
      {
         if (IsLoggedIn)
         {
            if (UpdateCases() && UpdateRecentCases())
            {
               // Update the "Cases" menu.
               casesToolStripMenuItem.DropDownItems.Clear();
               foreach (FogBugzCase cs in cases.OrderByDescending(c => c.Id).Take(10))
               {
                  Boolean isSelected = workingCase == null ? false : workingCase.Id == cs.Id;
                  AddMenuItem(casesToolStripMenuItem, cs.Id, String.Format("{0} - {1} ({2} {3})", cs.Id, cs.Title, cs.Project, cs.FixFor ?? String.Empty), Case_Click, isSelected);
               }
               if (casesToolStripMenuItem.DropDownItems.Count == 0)
               {
                  ToolStripMenuItem menuItem = new ToolStripMenuItem();
                  menuItem.Text = "No Cases";
                  menuItem.Enabled = false;
                  casesToolStripMenuItem.DropDownItems.Add(menuItem);
               }

               // Update the "Recent Cases" menu.
               recentCasesToolStripMenuItem.DropDownItems.Clear();
               foreach (FogBugzCase cs in recentCases.OrderByDescending(c => c.ResolvedOn))
               {
                  Boolean isSelected = workingCase == null ? false : workingCase.Id == cs.Id;
                  AddMenuItem(recentCasesToolStripMenuItem, cs.Id, String.Format("{0} - {1} ({2} {3})", cs.Id, cs.Title, cs.Project, cs.FixFor ?? String.Empty), Case_Click, isSelected);
               }
               if (recentCasesToolStripMenuItem.DropDownItems.Count == 0)
               {
                  ToolStripMenuItem menuItem = new ToolStripMenuItem();
                  menuItem.Text = "No Recent Cases";
                  menuItem.Enabled = false;
                  recentCasesToolStripMenuItem.DropDownItems.Add(menuItem);
               }

               // Update the "Projects" menu.
               projectsToolStripMenuItem.DropDownItems.Clear();
               // Create a menu item for each project.
               foreach (IGrouping<String, FogBugzCase> p in cases.GroupBy(c => c.Project))
               {
                  ToolStripMenuItem projectMenu = AddMenuItem(projectsToolStripMenuItem, p.Key);

                  // Create a menu item for each "Fix For" with the project.
                  foreach (IGrouping<String, FogBugzCase> f in cases.Where(c => c.Project == p.Key).OrderBy<FogBugzCase, DateTime>(c => c.FixForDate).GroupBy(c => c.FixFor))
                  {
                     ToolStripMenuItem fixForMenu = new ToolStripMenuItem();
                     fixForMenu.Text = f.Key;
                     projectMenu.DropDownItems.Add(fixForMenu);

                     // Create a menu item for each case in the "Fix For".
                     foreach (FogBugzCase cs in cases.Where(c => c.Project == p.Key && c.FixFor == f.Key).OrderBy<FogBugzCase, Int32>(c => c.Priority).ThenByDescending<FogBugzCase, Int32>(c => c.Id).Distinct<FogBugzCase>().Take(10))
                     {
                        Boolean isSelected = workingCase == null ? false : workingCase.Id == cs.Id;
                        AddMenuItem(fixForMenu, cs.Id, String.Format("{0} - {1}", cs.Id, cs.Title), Case_Click, isSelected);
                     }
                  }
               }
               return true;
            }
         }
         return false;
      }

      #endregion

      private void tray_MouseClick(object sender, MouseEventArgs e)
      {
         // Show menu when left-clicked. Right-click is taken care if automatically by NotifyIcon.ContextMenuStrip
         if (e.Button == MouseButtons.Left)
            showMenuInTaskBar(menu);
      }

      void showMenuInTaskBar(ContextMenuStrip menu)
      {
         // See http://support.microsoft.com/kb/135788
         SetForegroundWindow(menu.Handle);

         Point menuPos = Control.MousePosition - menu.Size;

         // Handle taskbar being docked in locations other than bottom of screen.
         Rectangle screen = Screen.FromPoint(Control.MousePosition).Bounds;
         if (menuPos.X < screen.X)
            menuPos.X += menu.Width;
         if (menuPos.Y < screen.Y)
            menuPos.Y += menu.Height;

         // Note: this constrains the window within the desktop, NOT overlapping the taskbar
         menu.Show(menuPos);
         // ... so now set the pos explicitly so we overlap taskbar
         SetWindowPos(menu.Handle, IntPtr.Zero, menuPos.X, menuPos.Y, 0, 0, SWP_NOSIZE | SWP_NOOWNERZORDER);
      }

     private System.Windows.Forms.Timer timer = new Timer();

      private void startTimer()
      {
         timer.Interval = 1500;
         timer.Tick += (o, e) => {
            if (workingCase == null && IsLoggedIn)
            {
               tray.ShowBalloonTip(0, "No Active Case.", "why you no work?", ToolTipIcon.Warning);
               timer.Interval = 1000; // *60 * 15;
            }
            else if (IsLoggedIn)
            {
               tray.ShowBalloonTip(0, "Reminder: Working On " + workingCase.Id, workingCase.Title, ToolTipIcon.Warning);
               timer.Interval = 3000; // *60 * 60;
            }
            else
            {
               tray.ShowBalloonTip(0, "Not Logged In", "you no loggy in", ToolTipIcon.Warning);
               timer.Interval = 1000; // *60 * 15;
            }

         };
         timer.Start();
      }

      #region [ API calls for showMenuInTaskBar() ]
      const int SWP_NOSIZE = 0x0001;
      const int SWP_NOOWNERZORDER = 0x0200;

      [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
      static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

      [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
      static extern bool SetForegroundWindow(IntPtr hWnd);
      #endregion


   }
}
