namespace DeMasterProCloud.DataModel.Visit
{
    public class SymptomData
    {
        public string Field { get; set; }
        public object Value { get; set; }
    }
    
    public class SymptomDetail
    {
        public string Field { get; set; }
        public string Symptom { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
    }

    public class SymptomDetailModel
    {
        public bool HealthStatus { get; set; }
        public bool ContactWithPatient { get; set; }
        public string InEpidemicArea { get; set; }
    }

    public class VisitWebhookData
    {
        public string Token { get; set; }
        public string Data { get; set; }
    }

    public class BkavMessagePayload
    {
        public string Status { get; set; }
        public BkavDataInVisitWebhook Data { get; set; }
    }
    
    public class BkavDataInVisitWebhook
    {
        public int Type { get; set; }
        public BkavUserInVisitWebhook User { get; set; }
        public string CreateDate { get; set; }
    }

    public class BkavUserInVisitWebhook
    {
        public string FullName { get; set; }
        public int YearOfBirthday { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Town { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string HealthInsuranceNumber { get; set; }
        public string Identify { get; set; }
        public int HealthStatus { get; set; }
        public bool ContactWithPatient { get; set; }
        public string InEpidemicArea { get; set; }
    }
}