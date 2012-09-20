using System;
using System.IO;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Common;
using Microsoft.TeamFoundation.Client;

    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write("<html>");
            string teamFoundationServerUrl = Request.Params["TeamFoundationServerUrl"];
            string buildUri = Request.Params["BuildUri"];

            if (String.IsNullOrEmpty(teamFoundationServerUrl))
            {
                teamFoundationServerUrl = "http://localhost:8080";
            }

            if (String.IsNullOrEmpty(buildUri))
            {
                Response.Write("<title>LogViewer Error</title><body>A valid BuildUri must be passed</body></html>");
                Response.End();
                return;
            }

            TeamFoundationServer tfs = new TeamFoundationServer(teamFoundationServerUrl);
            IBuildServer buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));

            IBuildDetail buildDetail = buildServer.GetBuild(new Uri(buildUri));

            String logFile = Path.Combine(buildDetail.DropLocation, BuildConstants.BuildLogFileName);

            Response.Write("<title>Build Log: " + buildDetail.BuildNumber + "</title><body>\r\n<pre>");

            StreamReader reader = File.OpenText(logFile);
            String line = reader.ReadLine();

            while (line != null)
            {
                WriteLine(line);
                line = reader.ReadLine();
            }
            reader.Close();

            Response.Write("</pre></html>");

            Response.End();

        }

        private void WriteLine(string line)
        {
            line = Server.HtmlEncode(line);
            if (line.StartsWith("Target &quot;"))
            {
                line = "<strong>" + line + "</strong>";
            }
            Response.Write(line);
            Response.Write("\r\n");
        }
    }
