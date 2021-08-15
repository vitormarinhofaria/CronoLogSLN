using CronoLog.Models;
using CronoLog.Utils;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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
        public TimedController()
        {
        }

        public static async void SetEndTime(MongoClient dbClient)
        {
            TimedController.NoSleep();

            bool running = true;
            DateTime doWhen = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 3, 10, 0);
            doWhen = doWhen.AddDays(1);
            Console.WriteLine($"Cartões em excecução serão pausados automaticamente em: {doWhen.ToLongDateString()} {doWhen.ToLongTimeString()} UTC");

            while (running)
            {
                DateTime now = DateTime.Now;
                TimeSpan countDown = doWhen - now;

                await Task.Delay(countDown);
                Console.WriteLine("Pausando cartões... ");

                var cardsCollection = DatabaseUtils.CardsCollection(dbClient);
                var boardsCollection = DatabaseUtils.BoardsCollection(dbClient);
                var cards = await cardsCollection.Find(Builders<TrelloCard>.Filter.Empty).ToListAsync();

                foreach (var card in cards)
                {
                    if (card.Timers.Count > 0)
                    {
                        var lastTimer = card.Timers.FindLast((t) => t.State == TimeState.RUNNING);
                        if (lastTimer != null)
                        {
                            var boardFilter = Builders<TrelloBoard>.Filter.Eq("Id", card.BoardId);
                            var board = await boardsCollection.Find(boardFilter).FirstOrDefaultAsync();
                            var httpClient = new HttpClient();
                            CardData cardData = new CardData()
                            {
                                Board = new IdName(board.Id, board.Name),
                                Member = new IdName("0", "Admin"),
                                Card = new IdName(card.Id, card.Name),
                                List = new IdName(card.CurrentList.Id, card.CurrentList.Name)
                            };
                            await httpClient.PostAsJsonAsync(ApiUtils.API_URL + "/cardtime/pause-stopwatch", cardData);
                        }
                    }
                }
                doWhen = doWhen.AddDays(1);
                Console.WriteLine($"Cartões em excecução serão pausados automaticamente em: {doWhen.ToLongDateString()} {doWhen.ToLongTimeString()} UTC");
            }
        }
        public static async void NoSleep()
        {
            bool running = true;
            DateTime taskTime = DateTime.UtcNow.AddMinutes(1);
            Console.WriteLine($"NoSleep: {taskTime}");
            while (running)
            {
                TimeSpan timeToWait = taskTime - DateTime.Now;
                await Task.Delay(timeToWait);

                var httpClient = new HttpClient();
                Console.WriteLine($"Fazendo chamada /timed/shake ... {taskTime.ToLongTimeString()}");
                await httpClient.GetAsync(ApiUtils.API_URL + "/Timed/shake");

                taskTime = taskTime.AddMinutes(20);
                Console.WriteLine($"Proxima chamada as {taskTime.ToLongTimeString()}");
            }
        }

        [HttpGet("/shake")]
        public IActionResult Shake()
        {
            return new JsonResult(new { });
        }
    }
}