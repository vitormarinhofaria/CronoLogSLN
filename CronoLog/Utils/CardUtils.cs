using CronoLog.Models;

namespace CronoLog.Utils
{
    public class CardUtils
    {
        static CardTagPattern CardPatternFullSave = new("[", "]", CardTagType.FULL_SAVE);
        static CardTagPattern CardPatternTrelloOnly = new("(", ")", CardTagType.TRELLO_ONLY);
        static CardTagPattern CardPatternNone = new("{", "}", CardTagType.NONE);
        public static CardTagPattern MatchTagPattern(string fullCardName)
        {
            var name = fullCardName.Trim();

            if (CardPatternFullSave.MatchName(name)) { return CardPatternFullSave; }
            if (CardPatternTrelloOnly.MatchName(name)) { return CardPatternTrelloOnly; }
            if (CardPatternNone.MatchName(name)) { return CardPatternNone; }

            return CardPatternNone;
        }

        public static (string, string) GetCardService_Name(string fullName, CardTagPattern pattern)
        {
            var cardName = fullName.Trim();

            var initPos = cardName.IndexOf(pattern.StartsWith);
            var endPos = cardName.IndexOf(pattern.EndsWith);
            
            var service = cardName.Substring(initPos + 1, endPos - 1).Trim();

            cardName = cardName.Replace($"{pattern.StartsWith}{service}{pattern.EndsWith} - ", "").Trim();
            cardName = cardName.Replace($"{pattern.StartsWith}{service}{pattern.EndsWith}-", "").Trim();
            cardName = cardName.Replace($"{pattern.StartsWith}{service}{pattern.EndsWith}", "").Trim();

            service = service[0].ToString().ToUpper() + service.Remove(0, 1).ToLower();
            service = service.Trim();

            return (service, cardName);
        }
    }

    public enum CardTagType
    {
        FULL_SAVE,
        TRELLO_ONLY,
        NONE,
        ENUM_LENGH,
    }

    public struct CardTagPattern
    {
        public string StartsWith { get; }
        public string EndsWith { get; }
        public CardTagType Type { get; }
        internal CardTagPattern(string start, string end, CardTagType type)
        {
            StartsWith = start;
            EndsWith = end;
            Type = type;
        }
        public bool MatchName(string name)
        {
            name.Trim();
            var initPos = name.IndexOf(StartsWith);
            var endPos = name.IndexOf(EndsWith);
            if (initPos != -1 && endPos != -1)
                return true;
            else
                return false;
        }
    }
}
