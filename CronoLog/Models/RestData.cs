using System;
using System.Collections.Generic;

namespace CronoLog.Models
{
    public struct CardData
    {
        public IdName Card { get; set; }
        public IdName Member { get; set; }
        public IdName List { get; set; }
        public IdName Board { get; set; }
    }
    public struct BoardData
    {
        public IdName Member { get; set; }
        public IdName List { get; set; }
        public IdName Board { get; set; }

    }
    public struct BoardLoadData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<IdName> Members { get; set; }
        public List<IdName> Cards { get; set; }
    }
    public struct IdName
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IdName(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public void Get(out TrelloList list)
        {
            list = new TrelloList(Id, Name);
        }
        public void Get(out TrelloBoard board)
        {
            board = new TrelloBoard(Id, Name);
        }
        public void Get(out TrelloMember member)
        {
            member = new TrelloMember(Id, Name);
        }
    }
    public struct UpdateCardTimeData
    {
        /*{"timeId":"e3e87d41-ae52-408c-9b07-7ca78c7ed0f0","cardId":"6081be5055c1d288ba063509","start":"2021-04-26T20:22:00.000Z","end":"2021-04-26T20:22:00.000Z","state":1}*/
        public string TimeId { get; set; }
        public string CardId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeState State { get; set; }
    }

    public class FullBoardData
    {
        public string BoardId { get; set; }
        public string BoardName { get; set; }
        public List<TrelloCard> Cards { get; set; }
        public List<TrelloMember> Members { get; set; }
    }
    public class UpdateChronoRequest
    {
        public string CardId { get; set; }
        public string MemberId { get; set; }
        public string ChronoId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string RequestMemberId { get; set; }
    }
}