using System;
using System.Collections.Generic;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using TSheets;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using log4net;
using Newtonsoft.Json;
using PBJReport;

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

        //public class DataClass
        //{
        //    public string Column1 { get; set; }
        //    public List<DataRow> Properties { get; set; }
        //}

        protected void Page_Load(object sender, EventArgs e)
        {
            log.Info("Hello logging world!");
            Console.WriteLine("Hit enter");
 
            EnvironmentVariableTarget tgt = EnvironmentVariableTarget.Machine;
            _clientId = Environment.GetEnvironmentVariable("TSHEETS_CLIENTID", tgt);
            _clientSecret = Environment.GetEnvironmentVariable("TSHEETS_CLIENTSECRET", tgt);
            _redirectUri = Environment.GetEnvironmentVariable("TSHEETS_REDIRECTURI", tgt);
            _manualToken = Environment.GetEnvironmentVariable("TSHEETS_MANUALTOKEN", tgt);

            // set up the ConnectionInfo object which tells the API how to connect to the server
            _connection = new ConnectionInfo(_baseUri, _clientId, _redirectUri, _clientSecret);

            AuthenticateWithManualToken();

            DateTimeOffset sDate = new DateTime(2018, 10, 14, 0, 0, 0, DateTimeKind.Local);
            DateTimeOffset eDate = new DateTime(2018, 10, 20, 0, 0, 0, DateTimeKind.Local);

            DataTable dataTable = PayrollByJobcodeReportSample(sDate,eDate);

            dataTable.DefaultView.Sort = "ConsumerName, Jobcode";

            DataView dv = dataTable.DefaultView;

            var dataView = new DataView(dataTable)
            {
                Sort = "ConsumerName ASC, Jobcode ASC"
            };

        }

        protected void SubmitButton_OnClick(object sender, EventArgs e)
        {
            Utility ut = new Utility();

            DateTimeOffset sDate = ut.FromString(txtStartDate.Text);
            DateTimeOffset eDate = ut.FromString(txtEndDate.Text);

            ReportDocument crystalReport = new ReportDocument();
            crystalReport.Load(Server.MapPath("CrystalReport1.rpt"));

            DataTable dataTable1 = PayrollByJobcodeReportSample(sDate, eDate);



            DataSet1 ds = new DataSet1();
            ds.Tables.Add(dataTable1);

            crystalReport.SetDataSource(ds.Tables[1]);
            CrystalReportViewer1.ReportSource = crystalReport;
            CrystalReportViewer1.RefreshReport();

            //throw new NotImplementedException();
        }

        void AuthenticateWithManualToken()
        {
            _authProvider = new StaticAuthentication(_manualToken);
        }

        void AuthenticateWithBrowser()
        {
            var userAuthProvider = new UserAuthentication(_connection);
            _authProvider = userAuthProvider;

            userAuthProvider.TokenChanged += userAuthProvider_TokenChanged;

            OAuthToken authToken = userAuthProvider.GetToken();

            string savedToken = authToken.ToJson();

            // This can be restored into a UserAuthentication object later to reuse:
            OAuthToken restoredToken = OAuthToken.FromJson(savedToken);
            UserAuthentication restoredAuthProvider = new UserAuthentication(_connection, restoredToken);

            // Now the user will not be prompted when we call GetToken
            OAuthToken cachedToken = restoredAuthProvider.GetToken();
        }

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
 
        public DataTable PayrollByJobcodeReportSample(DateTimeOffset sDate, DateTimeOffset eDate)
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
          
            DataTable table = new DataTable();
            table.Columns.Add("Start", typeof(string));
            table.Columns.Add("End", typeof(string));
            table.Columns.Add("ConsumerName", typeof(string));
            table.Columns.Add("Jobcode", typeof(string));
            table.Columns.Add("Hours", typeof(string));
            table.Columns.Add("Units", typeof(string));
            table.Columns.Add("Ratio %", typeof(string));

            Utility utility = new Utility();

            var pbjReportObject = JsonConvert.DeserializeObject<PayrollByJobcode>(payrollByJobcodeData);
            foreach (KeyValuePair<string, ByUser> userObject in pbjReportObject.Results.PayrollByJobcodeReport.ByUser)
            {
                if (pbjReportObject.SupplementalData.Users.TryGetValue(userObject.Key, out PBJReport.User user))
                {
                    string start = sDate.ToString("MM-dd-yyyy");
                    string end = eDate.ToString("MM-dd-yyyy");
                    string consumer = user.FirstName + ", " + user.LastName;

                    long totalHours = 0;
                    foreach (KeyValuePair<string, Total> totals in userObject.Value.Totals)
                    {
                        totalHours += totals.Value.TotalReSeconds;
                    }
                    string tHours = utility.DurationToHours(totalHours).ToString("N");
                    string tUnits = utility.DurationToUnits(totalHours).ToString();


                    string jobcode = null;
                    foreach (KeyValuePair<string, Total> totals in userObject.Value.Totals)
                    {
                        var jobcodes = pbjReportObject.SupplementalData.Jobcodes;
                        if (jobcodes.TryGetValue(totals.Value.JobcodeId.ToString(), out PBJReport.Jobcode jc))
                        {
                            jobcode = jc.Name;
                        }
                            
                        string hours = utility.DurationToHours(totals.Value.TotalReSeconds).ToString("N");
                        string units = utility.DurationToUnits(totals.Value.TotalReSeconds).ToString();

                        double ratio = (double)totals.Value.TotalReSeconds / totalHours;

                        table.Rows.Add(start, end, consumer, jobcode, hours, units, ratio.ToString("0.00%"));
                    }
                    //table.Rows.Add("", "TOTAL", tHours, tUnits,"");
                }
            }
            return table;
        }
    }
}