using System;

namespace CronoLog.Models
{
    public class CardTime
    {
        public string Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TrelloMember StartMember { get; set; }
        public TrelloMember? EndMember { get; set; }
        public TimeState State { get; set; }
        public TrelloList List { get; set; }
        public CardTime(TrelloMember startMember, TrelloList list)
        {
            StartMember = startMember;
            List = list;
            Start = DateTime.UtcNow;
            End = new DateTime().ToUniversalTime();
            State = TimeState.RUNNING;
            Guid guid = Guid.NewGuid();
            Id = guid.ToString();
        }
        public CardTime() { }
    }

    public enum TimeState
    {
        RUNNING,
        STOPPED,
        PAUSED,
    }
}