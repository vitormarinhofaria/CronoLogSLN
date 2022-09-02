using CronoLog.Models;
using MongoDB.Driver;

namespace CronoLog.Utils
{
    public class DatabaseUtils
    {
        public const string DatabaseName = "TrelloTimer";
        public const string CardsCollectionName = "Cards";
        public const string BoardsColletionName = "Boards";
        public const string CardInfoCollectionName = "CardInfoBadges";
        public static IMongoCollection<TrelloCard> CardsCollection(IMongoClient dbClient)
        {
            return dbClient.GetDatabase(DatabaseName).GetCollection<TrelloCard>(CardsCollectionName);
        }
        public static IMongoCollection<TrelloBoard> BoardsCollection(IMongoClient dbClient)
        {
            return dbClient.GetDatabase(DatabaseName).GetCollection<TrelloBoard>(BoardsColletionName);
        }
        public static IMongoCollection<CardInfoBadge> CardInfoBadgeCollection(IMongoClient dbClient)
        {
            return dbClient.GetDatabase(DatabaseName).GetCollection<CardInfoBadge>(CardInfoCollectionName);
        }
    }
}
