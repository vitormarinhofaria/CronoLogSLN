using System;
using System.Collections.Generic;
using System.Linq;

namespace CronoLog.Models
{
    public class TrelloCard
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TrelloList CurrentList { get; set; }
        public TrelloMember CurrentMember { get; set; }
        public string BoardId { get; set; }
        public List<CardTime> Timers { get => timers; set => timers = value; }
        private List<CardTime> timers = new();
        public bool Active { get; set; }
        public TrelloCard() { }

        public TrelloCard(CardData cardData, TrelloList tList, TrelloMember tMember, string boardId)
        {
            Id = cardData.Card.Id;
            Name = cardData.Card.Name;
            CurrentList = tList;
            CurrentMember = tMember;
            BoardId = boardId;
        }

        public TrelloCard(string id, string name, TrelloList currentList, TrelloMember currentMember, string boardId)
        {
            Id = id;
            Name = name;
            CurrentList = currentList;
            CurrentMember = currentMember;
            BoardId = boardId;
        }

        public CardTime Time(TrelloMember member, TrelloList list)
        {
            CardTime returnCard;
            var runningTimer = Timers.Find(t => t.State == TimeState.RUNNING);

            if (list.Id == CurrentList.Id && runningTimer != null)
            {
                returnCard = runningTimer;
            }
            else if (runningTimer != null)
            {
                runningTimer.End = DateTime.UtcNow;
                runningTimer.State = TimeState.STOPPED;
                runningTimer.EndMember = member;

                var time = new CardTime(member, list);
                Timers.Add(time);
                returnCard = time;
            }
            else if (Timers.Count == 0 || CurrentList.Id != list.Id)
            {
                var time = new CardTime(member, list);
                Timers.Add(time);
                returnCard = time;
            }
            else
            {
                returnCard = Timers.LastOrDefault();
            }

            CurrentList = list;
            CurrentMember = member;

            return returnCard;
        }

    }
}