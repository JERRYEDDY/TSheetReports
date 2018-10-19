using System;
using System.Collections.Generic;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using TSheets;
using System.Globalization;
using System.Data;
using System.Linq;
using CrystalDecisions.CrystalReports.Engine;
using log4net;
using Newtonsoft.Json;
using QuickType;


namespace TSheetReports
{
    public partial class _Default : Page
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

            //DataTable schDataTable = GetScheduleCalendars();

            //GetJobcodesByPageSample();
            //GetScheduleEventsSample();
            //GetUserInfoSample();
            //GetUsersSample();

            DateTimeOffset sDate = new DateTime(2018, 10, 14, 0, 0, 0, DateTimeKind.Local);
            DateTimeOffset eDate = new DateTime(2018, 10, 20, 0, 0, 0, DateTimeKind.Local);

            PayrollByJobcodeReportSample(sDate,eDate);

            //ProjectReportSample(sDate,eDate);
            //AddEditDeleteTimesheetSample();

            //DateTimeOffset _sDate = new DateTime(2018, 8, 26, 0, 0, 0, DateTimeKind.Local);
            //DateTimeOffset _eDate = new DateTime(2018, 9, 1, 0, 0, 0, DateTimeKind.Local);

            //ReportDocument crystalReport = new ReportDocument();
            //crystalReport.Load(Server.MapPath("CrystalReport1.rpt"));

            //DataTable dt1 = GetTimesheetsSample(_sDate, _eDate);
            //DataTable dt2 = GetScheduleEventsSample(_sDate,_eDate);
            //dt1.Merge(dt2);
            //DataSet1 ds = new DataSet1();
            //ds.Tables.Add(dt1);

            //crystalReport.SetDataSource(ds.Tables[1]);
            //CrystalReportViewer1.ReportSource = crystalReport;
            //CrystalReportViewer1.RefreshReport();
        }

        protected void SubmitButton_OnClick(object sender, EventArgs e)
        {
            Utility ut = new Utility();

            DateTimeOffset sDate = ut.DTOFromString(txtStartDate.Text);
            DateTimeOffset eDate = ut.DTOFromString(txtEndDate.Text);

            ReportDocument crystalReport = new ReportDocument();
            crystalReport.Load(Server.MapPath("CrystalReport1.rpt"));

            DataTable dataTable2 = GetTimesheetsSample(sDate, eDate);
            DataTable dataTable1 = GetScheduleEventsSample(sDate, eDate, "145533");

            dataTable1.Merge(dataTable2);

            //Select the rows where columns 1-4 have repeated same values
            var distinctRows = dataTable1.AsEnumerable()
                .Select(s => new
                {
                    unique1 = s.Field<string>("ConsumerName"),
                    unique2 = s.Field<string>("Jobcode"),
                    //unique3 = s.Field<int>("Column3"),
                    //unique4 = s.Field<int>("Column4"),
                })
                .Distinct();

            //Create a new datatable for the result
            DataTable resultDataTable = dataTable1.Clone();
            resultDataTable.Columns.Add("Remaining", typeof(double), "IIF(Scheduled-Actual<0,0.00, Scheduled-Actual)");
            resultDataTable.Columns.Add("Total", typeof(double), "Actual + Remaining");

            //Temporary variables
            DataRow newDataRow;
            IEnumerable<DataRow> results;
            double tempSched;
            double tempActual;

            //Go through each distinct rows to gather column5 and column6 values
            foreach (var item in distinctRows)
            {
                //create a new row for the result datatable
                newDataRow = resultDataTable.NewRow();

                //select all rows in original datatable with this distinct values
                results = dataTable1.Select().Where(
                    p => p.Field<string>("ConsumerName") == item.unique1
                         && p.Field<string>("Jobcode") == item.unique2
                         //&& p.Field<int>("Column3") == item.unique3
                         //&& p.Field<int>("Column4") == item.unique4
                         );

                //Preserve column1 - 4 values
                newDataRow["ConsumerName"] = item.unique1;
                newDataRow["Jobcode"] = item.unique2;
                //newDataRow["Column3"] = item.unique3;
                //newDataRow["Column4"] = item.unique4;

                //store here the sumns of column 5 and 6
                tempSched = 0;
                tempActual = 0;
                foreach (DataRow dr in results)
                {
                    tempSched += (double)dr["Scheduled"];
                    tempActual += (double)dr["Actual"];
                }

                //save those sumns in the new row
                newDataRow["Scheduled"] = tempSched;
                newDataRow["Actual"] = tempActual;

                //add the row to the result dataTable
                resultDataTable.Rows.Add(newDataRow);
            }

            DataSet1 ds = new DataSet1();
            ds.Tables.Add(dataTable1);

            crystalReport.SetDataSource(ds.Tables[1]);
            CrystalReportViewer1.ReportSource = crystalReport;
            CrystalReportViewer1.RefreshReport();

            //throw new NotImplementedException();
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
        DataTable GetTimesheetsSample(DateTimeOffset sDate, DateTimeOffset eDate)
        {
            var tsheetsApi = new RestClient(_connection, _authProvider);
            var filters = new Dictionary<string, string>
            {
                { "start_date", sDate.ToString("yyyy-MM-dd") },
                { "end_date", eDate.ToString("yyyy-MM-dd") }
            };

            // Here we create a DataTable with four columns.
            DataTable table = new DataTable();
            table.Columns.Add("ConsumerName", typeof(string));
            table.Columns.Add("Jobcode", typeof(string));
            //table.Columns.Add("Date", typeof(string));
            table.Columns.Add("Scheduled", typeof(double));
            table.Columns.Add("Actual", typeof(double));

            var timesheetData = tsheetsApi.Get(ObjectType.Timesheets, filters);


            //var timesheet = Timesheets.FromJson(timesheetData);
            ///foreach(Timesheet tSheet in timesheet.Results.Timesheets.Values)


            var timesheetsObject = JObject.Parse(timesheetData);
            var allTimeSheets = timesheetsObject.SelectTokens("results.timesheets.*");
            foreach (var tSheet in allTimeSheets)
            {
                Utility ut = new Utility();
                DateTimeOffset date = DateTimeOffset.ParseExact(tSheet["date"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);

                double scheduled = 0.00;
                double seconds = (double)tSheet["duration"];
                double actual = ut.DurationToHours(seconds);

                var tsUser = timesheetsObject.SelectToken("supplemental_data.users." + tSheet["user_id"]);
                string consumerName = tsUser["last_name"] + ", " + tsUser["first_name"];
                var tsJobcode = timesheetsObject.SelectToken("supplemental_data.jobcodes." + tSheet["jobcode_id"]);

                table.Rows.Add(consumerName, tsJobcode["name"], scheduled, actual);
            }

            return table;
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
        DataTable GetScheduleCalendars()
        {
            //Utility ut = new Utility();
            //var sd = ut.FormatIso8601(sDate);
            //var ed = ut.FormatIso8601(eDate);

            var tsheetsApi = new RestClient(_connection, _authProvider);

            var filters = new Dictionary<string, string>
            {
                //{ "start", sDate.ToString("yyyy-MM-ddTHH:mm:ssK") },
                //{ "end", eDate.ToString("yyyy-MM-ddTHH:mm:ssK") }
                //{ "schedule_calendar_ids", "145533" }
                //,
                //{ "user_ids", "1444085"}
            };

            DataTable table = new DataTable();
            //table.Columns.Add("ConsumerName", typeof(string));
            //table.Columns.Add("Jobcode", typeof(string));
            //table.Columns.Add("Date", typeof(string));
            //table.Columns.Add("Scheduled", typeof(double));
            //table.Columns.Add("Actual", typeof(double));
            //DateTime modified = new DateTime(2018, 7, 15, 19, 32, 0);
            //DateTimeOffset localTimeAndOffset = new DateTimeOffset(modified, TimeZoneInfo.Local.GetUtcOffset(modified));
            //string s1 = modified.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ssK");
            //string ss3 = localTimeAndOffset.ToString("yyyy-MM-ddTHH:mm:ssK");

            // ISO8601 with 3 decimal places
            //string s2 = modified.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
            //=> "2017-06-26T20:45:00.070Z"

            var scheduleCalendarData = tsheetsApi.Get(ObjectType.ScheduleCalendars, filters);
            var scheduleCalendarsObject = JObject.Parse(scheduleCalendarData);
            var allScheduleCalendars = scheduleCalendarsObject.SelectTokens("results.schedule_calendars.*");
            foreach (var scheduleCalendar in allScheduleCalendars)
            {
                Utility ut = new Utility();

                //DateTimeOffset start = (DateTimeOffset)scheduleEvent["start"];
                //DateTimeOffset end = (DateTimeOffset)scheduleEvent["end"];

                //double seconds = (double)ut.FormatIso8601Duration(start, end);
                //double scheduled = ut.DurationToHours(seconds);
                //double actual = 0.00;

                //var tsUser = scheduleEventsObject.SelectToken(" al_data.users." + scheduleEvent["user_id"]);
                //string consumerName = tsUser["last_name"] + ", " + tsUser["first_name"];

                //var tsJobcode = scheduleEventsObject.SelectToken("supplemental_data.jobcodes." + scheduleEvent["jobcode_id"]);

                //table.Rows.Add(consumerName, tsJobcode["name"], scheduled, actual);


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

            return table;
        }

        DataTable GetScheduleEventsSample(DateTimeOffset sDate, DateTimeOffset eDate,string scheduleCalendarIds)
        {
            //Utility ut = new Utility();
            //var sd = ut.FormatIso8601(sDate);
            //var ed = ut.FormatIso8601(eDate);

            var tsheetsApi = new RestClient(_connection, _authProvider);

            var filters = new Dictionary<string, string>
            {
                { "start", sDate.ToString("yyyy-MM-ddTHH:mm:ssK") },
                { "end", eDate.ToString("yyyy-MM-ddTHH:mm:ssK") },
                { "schedule_calendar_ids", scheduleCalendarIds }
                //,
                //{ "user_ids", "1444085"}
            };

            DataTable table = new DataTable();
            table.Columns.Add("ConsumerName", typeof(string));
            table.Columns.Add("Date", typeof(string));
            table.Columns.Add("Jobcode", typeof(string));

            table.Columns.Add("Scheduled", typeof(double));
            table.Columns.Add("Actual", typeof(double));
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
                Utility ut = new Utility();

                DateTimeOffset start = (DateTimeOffset)scheduleEvent["start"];
                DateTimeOffset end = (DateTimeOffset) scheduleEvent["end"];

                double seconds = (double) ut.FormatIso8601Duration(start, end);
                double scheduled = ut.DurationToHours(seconds);
                double actual = 0.00;

                var tsUser = scheduleEventsObject.SelectToken("supplemental_data.users." + scheduleEvent["user_id"]);
                string consumerName = tsUser["last_name"] + ", " + tsUser["first_name"];

                var tsJobcode = scheduleEventsObject.SelectToken("supplemental_data.jobcodes." + scheduleEvent["jobcode_id"]);

                table.Rows.Add(consumerName, start.ToString(), tsJobcode["name"], scheduled, actual);


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

            return table;
        }

        /// <summary>
        /// Shows how to create a user, create a jobcode, log time against it, and then run a project report
        /// that shows them
        /// </summary>
        public static void ProjectReportSample(DateTimeOffset sDate, DateTimeOffset eDate)
        {
            var tsheetsApi = new RestClient(_connection, _authProvider);

            string startDate = sDate.ToString("yyyy-MM-dd");
            string endDate = eDate.ToString("yyyy-MM-dd");
            dynamic reportOptions = new JObject();
            reportOptions.data = new JObject();
            reportOptions.data.start_date = startDate;
            reportOptions.data.end_date = endDate;

            var projectReport = tsheetsApi.GetReport(ReportType.Project, reportOptions.ToString());

            Console.WriteLine(projectReport);

        }

        public static void PayrollByJobcodeReportSample(DateTimeOffset sDate, DateTimeOffset eDate)
        {
            var tsheetsApi = new RestClient(_connection, _authProvider);

            string startDate = sDate.ToString("yyyy-MM-dd");
            string endDate = eDate.ToString("yyyy-MM-dd");
            dynamic reportOptions = new JObject();
            reportOptions.data = new JObject();
            reportOptions.data.start_date = startDate;
            reportOptions.data.end_date = endDate;

            var payrollByJobcodeData = tsheetsApi.GetReport(ReportType.PayrollByJobcode, reportOptions.ToString());
            var payrollByJobcode = PayrollByJobcode.FromJson(payrollByJobcodeData);

            PayrollByJobcode pbj = new PayrollByJobcode();


            var message = JsonConvert.DeserializeObject<PayrollByJobcode>(payrollByJobcodeData);
            foreach (KeyValuePair<string, ByUser> usr in message.Results.PayrollByJobcodeReport.ByUser)
            {
                foreach (KeyValuePair<string, Total> tot in usr.Value.Totals)
                {

                }
                    
            }



            string sdate = message.Results.Filters.StartDate.ToString();

            var pbjObject = JObject.Parse(payrollByJobcodeData);

            string start = payrollByJobcode.Results.Filters.StartDate.ToString();
            foreach (var aUser in payrollByJobcode.Results.PayrollByJobcodeReport.ByUser)
            {

                //string u = aUser.Value.user_id;

                foreach (var total in aUser.Values.Totals)
                {

                }

            }

            var users = pbjObject.SelectTokens("results.payroll_by_jobcode_report.by_user.*");
            foreach (var user in users)
            {
                Utility ut = new Utility();

                string theUser = user["user_id"].ToString();

                foreach (var total in user["totals"])
                {

                    string jc = total["jobcode_id"].ToString();

                }
                
                //DateTimeOffset date = DateTimeOffset.ParseExact(user["date"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);

                //double scheduled = 0.00;
                //double seconds = (double)user["duration"];
                //double actual = ut.DurationToHours(seconds);

                //var tsUser = timesheetsObject.SelectToken("supplemental_data.users." + tSheet["user_id"]);
                //string consumerName = tsUser["last_name"] + ", " + tsUser["first_name"];
                //var tsJobcode = timesheetsObject.SelectToken("supplemental_data.jobcodes." + tSheet["jobcode_id"]);

                //table.Rows.Add(consumerName, tsJobcode["name"], scheduled, actual);
            }






            //Console.WriteLine(payrollByJobcodeReport);

        }
    }
}