using System.Collections.Generic;
public class Level
{
    public List<Room> Rooms { get; set; }
    public Room StartRoom { get; set; } 
    public Room EndRoom { get; set; }
    public List<RoomManager> RoomManagers { get; set; }  
    public Level()
    {
        Rooms = new List<Room>();
    }
    public RoomManager GetRoomManager(Room room)
    {
        if (Rooms == null || RoomManagers == null) return null;
        int index = Rooms.IndexOf(room);
        return (index >= 0 && index < RoomManagers.Count) ? RoomManagers[index] : null;
    }
}
