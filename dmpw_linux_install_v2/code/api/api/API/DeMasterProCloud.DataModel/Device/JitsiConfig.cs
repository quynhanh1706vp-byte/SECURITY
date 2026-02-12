using System;

namespace DeMasterProCloud.DataModel.Device
{
    
    public class ContextTokenModel
    {
        public UserTokenModel user { get; set; }

        public ContextTokenModel(string name = "Anonymous", string email = "", string avatar = "")
        {
            user = new UserTokenModel()
            {
                avatar = avatar,
                email = email,
                name = name
            };
        }
    }

    public class UserTokenModel
    {
        public string avatar { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }

    public class JitsiVideoRoom
    {
        public string Token { get; set; }
        public string MsgId { get; set; }
        public string RoomName { get; set; }
        public int Time { get; set; }
        public string DeviceAddress { get; set; }
        public string Server { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string DoorName { get; set; }
        public string TitleIncomingScreen { get; set; }
        public bool AutoAccept { get; set; }
        public bool EnableVideoCall { get; set; }
        public int PriorityNumber { get; set; }
    }

    public class AcceptCallVideoModel
    {
        public string MsgId { get; set; }
        public bool IsAccept { get; set; }
    }

    public class JitsiTokenModel
    {
        public string Token { get; set; }
    }

    public class JitsiValueToken
    {
        public string Email { get; set; }
        public string RoomName { get; set; }
    }
}