using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.User
{
    public class UserEbknModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string Avatar { get; set; }
        public string PWD { get; set; }
        public int AccountType { get; set; }
        public List<string> FaceImage { get; set; }
        public List<FingerEbknModel> Finger { get; set; }
    }
    public class FingerEbknModel
    {
        public string Note { get; set; }
        public string Templates { get; set; }
    }
}