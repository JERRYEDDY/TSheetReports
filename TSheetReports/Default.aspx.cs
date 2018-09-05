using System;
using System.Collections.Generic;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using TSheets;
using System.IO;
using System.Globalization;
using System.Data;
using QuickType;
using CrystalDecisions.CrystalReports.Engine;
using log4net;

namespace TSheetReports
{
    public partial class _Default : Page
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string _baseUri = "https://rest.tsheets.com/api/v1";

        private static ConnectionInfo _connection;
        private static IOAuth2 _authProvider;

        private static string _clientId;
        private static string _redirectUri;
        private static string _clientSecret;
        private static string _manualToken;

        protected void Page_Load(object sender, EventArgs e)
        {
            // _clientId, _redirectUri, and _clientSecret are needed by the API to connect to your
            // TSheets account.  To get these values for your account, log in to your TSheets account,
            // click on Company Settings -> Add-ons -> API Preferences and use the values for your
            // application. You can specify them through environment variables as shown here, or just
            // paste them into the code here directly.

            //ShowTextPDF();
            log.Info("Hello logging world!");
            Console.WriteLine("Hit enter");
 
            EnvironmentVariableTarget tgt = EnvironmentVariableTarget.Machine;
            _clientId = Environment.GetEnvironmentVariable("TSHEETS_CLIENTID", tgt);
            _clientSecret = Environment.GetEnvironmentVariable("TSHEETS_CLIENTSECRET", tgt);
            _redirectUri = Environment.GetEnvironmentVariable("TSHEETS_REDIRECTURI", tgt);

            // If you want to use simple authentication with a static token (AuthenticateWithManualToken()),
            // click on the properties of your application in API Preferences, and click the Add Token link.
            // Set _manualToken to the value created.
            _manualToken = Environment.GetEnvironmentVariable("TSHEETS_MANUALTOKEN", tgt);

            // set up the ConnectionInfo object which tells the API how to connect to the server
            _connection = new ConnectionInfo(_baseUri, _clientId, _redirectUri, _clientSecret);

            // Choose which authentication method to use. AuthenticateWithBrowser will do a full OAuth2 forms
            // based authentication in a web browser form and prompt the user for credentials.  
            // AuthenticateWithManualToken will do simple AccessToken based authentication using 
            // a manually created token in the API add-on.
            //AuthenticateWithBrowser();


            AuthenticateWithManualToken();
            //GetJobcodesByPageSample();
            //GetScheduleEventsSample();
            //GetUserInfoSample();
            //GetUsersSample();

            //ProjectReportSample();
            //AddEditDeleteTimesheetSample();

            DateTimeOffset _sDate = new DateTime(2018, 8, 26, 0, 0, 0, DateTimeKind.Local);
            DateTimeOffset _eDate = new DateTime(2018, 9, 1, 0, 0, 0, DateTimeKind.Local);

            ReportDocument crystalReport = new ReportDocument();
            crystalReport.Load(Server.MapPath("CrystalReport1.rpt"));

            DataSet1 ds = GetTimesheetsSample(_sDate, _eDate);

            //GetScheduleEventsSample(_sDate,_eDate);

            crystalReport.SetDataSource(ds.Tables[1]);
            CrystalReportViewer1.ReportSource = crystalReport;
            CrystalReportViewer1.RefreshReport();
        }


    /// <summary>
    /// Shows how to set up authentication to use a static/manually created access token.
    /// To create a manual auth token, go to the API Add-on preferences in your TSheets account
    /// and click Add Token.
    /// </summary>
        void AuthenticateWithManualToken()
        {
            _authProvider = new StaticAuthentication(_manualToken);
        }

        /// <summary>
        /// Shows how to set up authentication to authenticate the user in an embedded browser form
        /// and get an OAuth2 token by prompting the user for credentials.
        /// </summary>
        void AuthenticateWithBrowser()
        {
            // The UserAuthentication class will handle the OAuth2 desktop
            // authentication flow using an embedded WebBrowser form, 
            // cache the returned token for later API usage, and handle token refreshes.
            var userAuthProvider = new UserAuthentication(_connection);
            _authProvider = userAuthProvider;

            // optionally register an event handler to be notified if/when the auth
            // token changes
            userAuthProvider.TokenChanged += userAuthProvider_TokenChanged;

            // Retrieve a token from the server
            // Note: the RestApi class will call this as needed so it isn't required
            // to call it before accessing the API. However, manually calling GetToken first
            // is recommended so the app can more gracefully handle authentication errors
            OAuthToken authToken = userAuthProvider.GetToken();

            // OAuth2 tokens can and should be cached across application uses so users
            // don't need to grant access every time they run the application.
            // To do this, call OAuthToken.ToJSon to get a serialized version of
            // the token that can be used later.  Be sure to treat this string as a 
            // user password and store it securely!
            // Note that this token will potentially be refreshed during API usage
            // using the OAuth2 token refresh protocol.  If that happens, your application
            // should overwrite the previously saved token with the new token value.
            // You can register for the TokenChanged event to be notified of any new/changed tokens
            // or you can call UserAuthentication.GetToken().ToJson() after using the API 
            // to manually retrieve the most current token.
            string savedToken = authToken.ToJson();

            // This can be restored into a UserAuthentication object later to reuse:
            OAuthToken restoredToken = OAuthToken.FromJson(savedToken);
            UserAuthentication restoredAuthProvider = new UserAuthentication(_connection, restoredToken);

            // Now the user will not be prompted when we call GetToken
            OAuthToken cachedToken = restoredAuthProvider.GetToken();
        }

        /// <summary>
        /// Event handler that will be called when the UserAuthentication OAuthToken changes
        /// </summary>
        void userAuthProvider_TokenChanged(object sender, TokenChangedEventArgs e)
        {
            if (e.CurrentToken != null)
            {
                Response.Write("<p>Received new auth token:</p>");
                Response.Write(e.CurrentToken.ToJson());
            }
            else
            {
                Response.Write("<p>Token no longer valid</p>");
            }
        }

        /// <summary>
        /// Shows how to get current logged in user information
        /// </summary>
        void GetUserInfoSample()
        {
            var tsheetsApi = new RestClient(_connection, _authProvider);
            var userData = tsheetsApi.Get(ObjectType.CurrentUser);
            var responseObject = JObject.Parse(userData);

            var userObject = responseObject.SelectToken("results.users.*");

            //Response.Write(string.Format("<p>Current User: {0} {1}, email = {2}, client url = {3}</p>",
            //    userObject["first_name"],
            //    userObject["last_name"],
            //    userObject["email"],
            //    userObject["client_url"]));
        }

        /// <summary>
        /// Shows how to get all users for the company
        /// </summary>
        void GetUsersSample()
        {
            var tsheetsApi = new RestClient(_connection, _authProvider);
            var userData = tsheetsApi.Get(ObjectType.Users);
            var responseObject = JObject.Parse(userData);

            var users = responseObject.SelectTokens("results.users.*");
            foreach (var userObject in users)
            {
                //Response.Write(string.Format("<p>Current User: {0} {1}, email = {2}, client url = {3}</p>",
                //    userObject["first_name"],
                //    userObject["last_name"],
                //    userObject["email"],
                //    userObject["client_url"]));
            }
        }

        /// <summary>
        /// Shows how to receive all timesheets for a given timeframe by using filter arguments
        /// and how to access the supplemental data in the response.
        /// Supplemental data will contain all of the user/jobcode/etc related data
        /// about the selected timesheets. API users should use the supplemental data when available
        /// rather than making additional calls to the server to receive that information.
        /// </summary>
        DataSet1 GetTimesheetsSample(DateTimeOffset sDate, DateTimeOffset eDate)
        {
            var tsheetsApi = new RestClient(_connection, _authProvider);
            var filters = new Dictionary<string, string>
            {
                { "start_date", sDate.ToString("yyyy-MM-dd") },
                { "end_date", eDate.ToString("yyyy-MM-dd") }
            };

            // Here we create a DataTable with four columns.
            DataTable table = new DataTable();
            table.Columns.Add("Consumer Name", typeof(string));
            table.Columns.Add("Jobcode", typeof(string));
            table.Columns.Add("Date", typeof(string));
            table.Columns.Add("Act. Hours", typeof(string));

            var timesheetData = tsheetsApi.Get(ObjectType.Timesheets, filters);
            var timesheetsObject = JObject.Parse(timesheetData);
            //var timesheet = Timesheet.FromJson(timesheetData);

            var allTimeSheets = timesheetsObject.SelectTokens("results.timesheets.*");
            foreach (var tsheet in allTimeSheets)

            //foreach (Timesheet wTimesheet in timesheet.Results.Timesheets.Values)
            {
                Utility ut = new Utility();
                double seconds = (double)tsheet["duration"];
                string hours = ut.DurationToHours(seconds);

                var tsUser = timesheetsObject.SelectToken("supplemental_data.users." + tsheet["user_id"]);
                string consumerName = tsUser["last_name"].ToString() + ", " + tsUser["first_name"].ToString();

                var tsJobcode = timesheetsObject.SelectToken("supplemental_data.jobcodes." + tsheet["jobcode_id"]);

                table.Rows.Add(consumerName, tsJobcode["name"], tsheet["date"].ToString(), hours);
            }

            //var timesheetsObject = JObject.Parse(timesheetData);
            //var allTimeSheets = timesheetsObject.SelectTokens("results.timesheets.*");
            //foreach (var tsheet in allTimeSheets)
            //{
            //    int seconds = (int)tsheet["duration"];
            //    TimeSpan t = TimeSpan.FromSeconds(seconds);
            //    string _duration = string.Format("{0:N2}", t.TotalHours);

            //    string jobCodeName;
            //    if (jobcodeList.TryGetValue((int)tsheet["jobcode_id"], out jobCodeName)) // Returns true.
            //    {
            //        //Console.WriteLine(test); // This is the value at cat.
            //    }
            //    var tsUser = timesheetsObject.SelectToken("supplemental_data.users." + tsheet["user_id"]);

            //    string consumerName = tsUser["last_name"].ToString() + ", " + tsUser["first_name"].ToString();
            //    table.Rows.Add(consumerName, jobCodeName, tsheet["date"].ToString(), _duration);

            //    Response.Write(string.Format("<p>User: {0} {1}, Date={2},JobCodeName={3},_Duration={4}",
            //        tsUser["first_name"], tsUser["last_name"], tsheet["date"], jobCodeName, _duration));
            //}

            DataSet1 ds = new DataSet1();
            ds.Tables.Add(table);

            return ds;
        }

        /// <summary>
        /// The Get api calls can potentially return many records from the server. The TSheets rest APIs
        /// support a paging request model so API clients can request records in smaller chunks.
        /// This sample shows how to request all available jobcodes using paging filters to retrieve
        /// the records this way.
        /// </summary>
        void GetJobcodesByPageSample()
        {
            var tsheetsApi = new RestClient(_connection, _authProvider);
            var filters = new Dictionary<string, string>();

            // start by requesting the first page
            int currentPage = 1;

            // and set our items per page to be 2
            // Note: 50 is the recommended per_page value for normal usage. This sample
            // is using a smaller number to make the sample more clear. Be sure to 
            // manually create >2 jobcodes in your account to see the paging happen.
            filters["per_page"] = "2";

            bool moreData = true;
            while (moreData)
            {
                filters["page"] = currentPage.ToString();

                var getResponse = tsheetsApi.Get(ObjectType.Jobcodes, filters);
                var responseObject = JObject.Parse(getResponse);

                // see if we have more pages to retrieve
                moreData = bool.Parse(responseObject.SelectToken("more").ToString());

                // increment to the next page
                currentPage++;

                var jobcodes = responseObject.SelectTokens("results.jobcodes.*");
                foreach (var jobcode in jobcodes)
                {
                    //jobcodeList.Add((int)jobcode["id"], (string)jobcode["name"]);

                    //Response.Write(string.Format("<p>Jobcode  ID: {0}, Name: {1}, type = {2}, shortcode = {3}</p>",
                    //    jobcode["id"],
                    //    jobcode["name"],
                    //    jobcode["type"],
                    //    jobcode["short_code"]));
                }
            }
        }





        void GetScheduleEventsSample(DateTimeOffset sDate, DateTimeOffset eDate)
        {
            var tsheetsApi = new RestClient(_connection, _authProvider);

            var filters = new Dictionary<string, string>
            {
                { "start_date", sDate.ToString("yyyy-MM-dd") },
                { "end_date", eDate.ToString("yyyy-MM-dd") },
                { "schedule_calendar_ids", "145533" },
                { "user_ids", "1444085"}
            };

            //DateTime modified = new DateTime(2018, 7, 15, 19, 32, 0);
            //DateTimeOffset localTimeAndOffset = new DateTimeOffset(modified, TimeZoneInfo.Local.GetUtcOffset(modified));
            //string s1 = modified.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ssK");
            //string ss3 = localTimeAndOffset.ToString("yyyy-MM-ddTHH:mm:ssK");

            // ISO8601 with 3 decimal places
            //string s2 = modified.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
            //=> "2017-06-26T20:45:00.070Z"

            var scheduleEventData = tsheetsApi.Get(ObjectType.ScheduleEvents, filters);

            var scheduleEventsObject = JObject.Parse(scheduleEventData);
            var allScheduleEvents = scheduleEventsObject.SelectTokens("results.schedule_events.*");
            foreach (var scheduleEvent in allScheduleEvents)
            {
 
                //int seconds = (int)scheduleEvent["duration"];
                //TimeSpan t = TimeSpan.FromSeconds(seconds);
                //string _duration = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",t.Hours,t.Minutes,t.Seconds);


                //string jobCodeName;
                //if (jobcodeList.TryGetValue((int)scheduleEvent["jobcode_id"], out jobCodeName)) // Returns true.
                //{
                //    Console.WriteLine();
                //}


                //Response.Write(string.Format("<p>Schedule Event: ID={0}, User_ID={1}, Jobcode={2}, JobCodeName={3}, Startdate={4}, _Enddate={5}, Title={6}",
                //scheduleEvent["id"], scheduleEvent["user_id"], scheduleEvent["jobcode_id"], jobCodeName, scheduleEvent["start"], scheduleEvent["end"], scheduleEvent["title"]));

                // get the associated user for this timesheet
                //var tsUser = timesheetsObject.SelectToken("supplemental_data.users." + timesheet["user_id"]);
                //Response.Write(string.Format("\tUser: {0} {1}</p>", tsUser["first_name"], tsUser["last_name"]));
            }
        }
    }
}