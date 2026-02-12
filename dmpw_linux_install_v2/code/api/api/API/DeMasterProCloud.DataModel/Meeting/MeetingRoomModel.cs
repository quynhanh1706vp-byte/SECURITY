using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Meeting;

public class MeetingRoomModel
{
    public int Id { get; set; }
    public string RoomName { get; set; }
    public string Image { get; set; }
    public List<int> DoorIds { get; set; }
}

public class MeetingRoomListModel
{
    public int Id { get; set; }
    public string RoomName { get; set; }
    public string Image { get; set; }
    public List<DoorModel> Doors { get; set; }
}

public class DoorModel
{
    public int Id { get; set; }
    public string DoorName { get; set; }
}