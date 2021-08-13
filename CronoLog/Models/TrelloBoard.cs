using System.Collections.Generic;

namespace CronoLog.Models
{
    public class TrelloBoard
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Cards { get => cards; set => cards = value; }
        private List<string> cards = new();
        public List<TrelloMember> Members { get => members; set => members = value; }
        private List<TrelloMember> members = new();
        public string StopwatchList { get; set; }
        public TrelloBoard(string id, string name)
        {
            Id = id;
            Name = name;
            StopwatchList = DefaultStopwatchList;
        }
        public TrelloBoard(string id, string name, List<TrelloMember> members)
        {
            Id = id;
            Name = name;
            Members = members;
            StopwatchList = DefaultStopwatchList;
        }
        public TrelloBoard(string id, string name, List<TrelloMember> members, List<string> cards)
        {
            Id = id;
            Name = name;
            Members = members;
            Cards = cards;
            StopwatchList = DefaultStopwatchList;
        }
        public TrelloBoard() { }

        public const string DefaultStopwatchList = "fazendo";

    }
}