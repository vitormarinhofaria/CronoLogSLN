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
        public static IMongoCollection<TrelloCard> CardsCollection(MongoClient dbClient)
        {
            return dbClient.GetDatabase(DatabaseName).GetCollection<TrelloCard>(CardsCollectionName);
        }
        public static IMongoCollection<TrelloBoard> BoardsCollection(MongoClient dbClient)
        {
            return dbClient.GetDatabase(DatabaseName).GetCollection<TrelloBoard>(BoardsColletionName);
        }
        public static IMongoCollection<CardInfoBadge> CardInfoBadgeCollection(MongoClient dbClient)
        {
            return dbClient.GetDatabase(DatabaseName).GetCollection<CardInfoBadge>(CardInfoCollectionName);
        }
    }
}
