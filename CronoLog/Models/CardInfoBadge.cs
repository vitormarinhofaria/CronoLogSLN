using System.Collections.Generic;

namespace CronoLog.Models
{
    public class CardInfoBadge
    {
        public string Id { get; set; }
        public List<string> Descriptions { get => descriptions; set => descriptions = value; }
        private List<string> descriptions = new();
        public CardInfoBadge(string id, string description)
        {
            Id = id;
            Descriptions.Add(description);
        }
        public CardInfoBadge(string id, List<string> descriptions)
        {
            Id = id;
            Descriptions.AddRange(descriptions);
        }
        public CardInfoBadge() { }
    }
}