using CronoLog.Models;
using CronoLog.Utils;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CronoLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("TrelloHostPolicy")]
    public class TimedController : ControllerBase
    {
        private readonly MongoClient DbClient;
        public TimedController([FromServices] MongoClient dbClient)
        {
            DbClient = dbClient;
        }

        [HttpGet("pause-cards")]
        public async Task<IActionResult> PauseCards()
        {
            Console.WriteLine("Pausando cartões ...");
            Stopwatch sw = new();
            sw.Start();
            var cardsCollection = DatabaseUtils.CardsCollection(DbClient);
            var cards = await cardsCollection.Find(Builders<TrelloCard>.Filter.Empty).ToListAsync();

            foreach (var card in cards)
            {
                if (card.Timers.Count > 0)
                {
                    var lastTimer = card.Timers.FindLast((t) => t.State == TimeState.RUNNING);
                    if (lastTimer != null)
                    {
                        card.CurrentMember = new("0", "Admin");
                        lastTimer.State = TimeState.PAUSED;
                        lastTimer.End = DateTime.UtcNow;
                        lastTimer.EndMember = card.CurrentMember;

                        var cardFilter = Builders<TrelloCard>.Filter.Eq("Id", card.Id);
                        await cardsCollection.FindOneAndReplaceAsync(cardFilter, card);
                    }
                }
            }
            sw.Stop();
            return new JsonResult(new {TimeToPauseAllCards = sw.Elapsed});
        }

        [HttpGet("shake")]
        public IActionResult Shake()
        {
            Console.WriteLine("CronoLog: <= timed/shake");
            return new JsonResult(new { });
        }
    }
}