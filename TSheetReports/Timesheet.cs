using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuickType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Timesheet
    {
        [JsonProperty("results")]
        public Results Results { get; set; }

        [JsonProperty("more")]
        public bool More { get; set; }

        [JsonProperty("supplemental_data")]
        public SupplementalData SupplementalData { get; set; }
    }

    public partial class Results
    {
        [JsonProperty("timesheets")]
        public Dictionary<string, Timesheet> Timesheets { get; set; }
    }

    public partial class Timesheet
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("jobcode_id")]
        public long JobcodeId { get; set; }

        [JsonProperty("start")]
        public DateTimeOffset Start { get; set; }

        [JsonProperty("end")]
        public DateTimeOffset End { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("tz")]
        public long Tz { get; set; }

        [JsonProperty("tz_str")]
        public string TzStr { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("on_the_clock")]
        public bool OnTheClock { get; set; }

        [JsonProperty("locked")]
        public long Locked { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("customfields")]
        public TimesheetCustomfields Customfields { get; set; }

        [JsonProperty("last_modified")]
        public DateTimeOffset LastModified { get; set; }

        [JsonProperty("attached_files")]
        public object[] AttachedFiles { get; set; }
    }

    public partial class TimesheetCustomfields
    {
        [JsonProperty("145171")]
        public string The145171 { get; set; }
    }

    public partial class SupplementalData
    {
        [JsonProperty("jobcodes")]
        public Dictionary<string, Jobcode> Jobcodes { get; set; }

        [JsonProperty("users")]
        public Users Users { get; set; }

        [JsonProperty("customfields")]
        public SupplementalDataCustomfields Customfields { get; set; }
    }

    public partial class SupplementalDataCustomfields
    {
        [JsonProperty("145171")]
        public The145171 The145171 { get; set; }
    }

    public partial class The145171
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("global")]
        public bool Global { get; set; }

        [JsonProperty("required")]
        public bool The145171_Required { get; set; }

        [JsonProperty("applies_to")]
        public string AppliesTo { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("short_code")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long ShortCode { get; set; }

        [JsonProperty("regex_filter")]
        public string RegexFilter { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("last_modified")]
        public DateTimeOffset LastModified { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("ui_preference")]
        public string UiPreference { get; set; }

        [JsonProperty("required_customfields")]
        public object[] RequiredCustomfields { get; set; }
    }

    public partial class Jobcode
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("parent_id")]
        public long ParentId { get; set; }

        [JsonProperty("assigned_to_all")]
        public bool AssignedToAll { get; set; }

        [JsonProperty("billable")]
        public bool Billable { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("has_children")]
        public bool HasChildren { get; set; }

        [JsonProperty("billable_rate")]
        public long BillableRate { get; set; }

        [JsonProperty("short_code")]
        public string ShortCode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("last_modified")]
        public DateTimeOffset LastModified { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("filtered_customfielditems")]
        public string FilteredCustomfielditems { get; set; }

        [JsonProperty("required_customfields")]
        public object[] RequiredCustomfields { get; set; }

        [JsonProperty("locations")]
        public object[] Locations { get; set; }
    }

    public partial class Users
    {
        [JsonProperty("1444085")]
        public The1444085 The1444085 { get; set; }
    }

    public partial class The1444085
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("group_id")]
        public long GroupId { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("employee_number")]
        public long EmployeeNumber { get; set; }

        [JsonProperty("salaried")]
        public bool Salaried { get; set; }

        [JsonProperty("exempt")]
        public bool Exempt { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonProperty("payroll_id")]
        public string PayrollId { get; set; }

        [JsonProperty("hire_date")]
        public string HireDate { get; set; }

        [JsonProperty("term_date")]
        public string TermDate { get; set; }

        [JsonProperty("last_modified")]
        public DateTimeOffset LastModified { get; set; }

        [JsonProperty("last_active")]
        public DateTimeOffset LastActive { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("client_url")]
        public string ClientUrl { get; set; }

        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty("mobile_number")]
        public string MobileNumber { get; set; }

        [JsonProperty("pto_balances")]
        public Dictionary<string, long> PtoBalances { get; set; }

        [JsonProperty("submitted_to")]
        public DateTimeOffset SubmittedTo { get; set; }

        [JsonProperty("approved_to")]
        public DateTimeOffset ApprovedTo { get; set; }

        [JsonProperty("manager_of_group_ids")]
        public object[] ManagerOfGroupIds { get; set; }

        [JsonProperty("require_password_change")]
        public bool RequirePasswordChange { get; set; }

        [JsonProperty("pay_rate")]
        public long PayRate { get; set; }

        [JsonProperty("pay_interval")]
        public string PayInterval { get; set; }

        [JsonProperty("permissions")]
        public Dictionary<string, bool> Permissions { get; set; }

        [JsonProperty("customfields")]
        public string Customfields { get; set; }
    }

    public partial class Timesheet
    {
        public static Timesheet FromJson(string json) => JsonConvert.DeserializeObject<Timesheet>(json, QuickType.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Timesheet self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();

        //This should get the user for the UID from the dictionary:
        public Jobcode GetJobcodeForUid(Dictionary<string, Jobcode> dictionary, int uid)
        {
            return dictionary.Values.FirstOrDefault(j => j.Id == uid);
        }

    }
}
