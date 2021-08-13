namespace CronoLog.Models
{
    public class TrelloMember
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TrelloMember(string id, string name)
        {
            Id = id;
            Name = name;
        }
        public TrelloMember() { }
    }
}