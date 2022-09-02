using CronoLog.Models;
using CronoLog.Utils;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace CronoLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("TrelloHostPolicy")]
    public class CardInfoBadgeController : ControllerBase
    {
        private readonly IMongoClient mDbClient;
        public CardInfoBadgeController(IMongoClient mongoClient)
        {
            mDbClient = mongoClient;
        }

        [HttpGet("{cardId}")]
        public async Task<IActionResult> GetCardInfo(string cardId)
        {
            var ciCollection = DatabaseUtils.CardInfoBadgeCollection(mDbClient);
            var cibFilter = Builders<CardInfoBadge>.Filter.Eq("Id", cardId);
            var cib = await ciCollection.Find(cibFilter).FirstOrDefaultAsync();

            if (cib == null)
            {
                return new JsonResult(new { });
            }
            else
            {
                return new JsonResult(cib);
            }
        }

        [HttpPost("{cardId}")]
        public async void SetCardInfo(string cardId, [FromBody] string descripton)
        {
            var ciCollection = DatabaseUtils.CardInfoBadgeCollection(mDbClient);
            var cibFilter = Builders<CardInfoBadge>.Filter.Eq("Id", cardId);
            var cib = await ciCollection.Find(cibFilter).FirstOrDefaultAsync();

            if (cib != null)
            {
                var updateDefinition = Builders<CardInfoBadge>.Update.Push("Descriptions", descripton);
                await ciCollection.FindOneAndUpdateAsync(cibFilter, updateDefinition);
            }
            else
            {
                cib = new CardInfoBadge(cardId, descripton);
                await ciCollection.InsertOneAsync(cib);
            }
        }
        [HttpPut("{cardId}")]
        public async void UpdateCardInfo(string cardId, [FromBody] string[] descriptions)
        {
            var ciCollection = DatabaseUtils.CardInfoBadgeCollection(mDbClient);
            var cibFilter = Builders<CardInfoBadge>.Filter.Eq("Id", cardId);
            var cib = await ciCollection.Find(cibFilter).FirstOrDefaultAsync();
            if (cib != null)
            {
                var updateDefinition = Builders<CardInfoBadge>.Update.Set("Descriptions", descriptions.ToList());
                await ciCollection.FindOneAndUpdateAsync(cibFilter, updateDefinition);
            }
            else
            {
                cib = new CardInfoBadge(cardId, descriptions.ToList());
                await ciCollection.InsertOneAsync(cib);
            }
        }
    }
}