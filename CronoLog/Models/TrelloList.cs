namespace CronoLog.Models
{
    public class TrelloList
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TrelloList(string id, string name)
        {
            Id = id;
            Name = name;
        }
        public TrelloList() { }
    }
}