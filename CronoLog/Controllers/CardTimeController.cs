using CronoLog.Models;
using CronoLog.Utils;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CronoLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("TrelloHostPolicy")]
    public class CardTimeController : ControllerBase
    {
        private readonly IMongoClient mDbClient;
        public CardTimeController(IMongoClient mongoClient)
        {
            mDbClient = mongoClient;
        }

        private IMongoCollection<TrelloBoard> BoardsCollection()
        {
            return mDbClient.GetDatabase("TrelloTimer").GetCollection<TrelloBoard>("Boards");
        }
        private IMongoCollection<TrelloCard> CardsCollection()
        {
            return mDbClient.GetDatabase("TrelloTimer").GetCollection<TrelloCard>("Cards");
        }

        [HttpPost("board-load")]
        public async Task<IActionResult> BoardLoad(BoardLoadData boardData)
        {
            var boardsCollection = BoardsCollection();

            var boardFilter = Builders<TrelloBoard>.Filter.Eq("Id", boardData.Id);
            var board = await boardsCollection.Find(boardFilter).FirstOrDefaultAsync();

            var members = boardData.Members.ConvertAll(e => new TrelloMember(e.Id, e.Name));
            if (board == null)
            {
                board = new TrelloBoard(boardData.Id, boardData.Name, members);
                boardsCollection.InsertOne(board);
            }
            else
            {
                var updateDefinition = Builders<TrelloBoard>.Update.Set("Members", members);
                await boardsCollection.FindOneAndUpdateAsync(boardFilter, updateDefinition);
                updateDefinition = Builders<TrelloBoard>.Update.Set("Name", boardData.Name);
                await boardsCollection.FindOneAndUpdateAsync(boardFilter, updateDefinition);

                var cardsCollection = CardsCollection();
                var cardsFilter = Builders<TrelloCard>.Filter.Eq("BoardId", board.Id);
                var cards = await cardsCollection.Find(cardsFilter).ToListAsync();

                foreach (var c in cards)
                {
                    var isActive = boardData.Cards.Exists(cIdName => cIdName.Id == c.Id);

                    var cardUpdate = Builders<TrelloCard>.Update.Set("Active", isActive);
                    var cardFilter = Builders<TrelloCard>.Filter.Eq("Id", c.Id);
                    await cardsCollection.FindOneAndUpdateAsync(cardFilter, cardUpdate);
                }
            }
            return new JsonResult(board);
        }

        [HttpPost("time-card")]
        public async Task<IActionResult> TimeCard(CardData cardData)
        {
            var boardsCollection = BoardsCollection();
            var cardsCollection = CardsCollection();

            cardData.List.Get(out TrelloList list);
            cardData.Member.Get(out TrelloMember member);

            var cardFilter = Builders<TrelloCard>.Filter.Eq("Id", cardData.Card.Id);
            TrelloCard card = await cardsCollection.Find(cardFilter).FirstOrDefaultAsync();

            var boardFilter = Builders<TrelloBoard>.Filter.Eq("Id", cardData.Board.Id);
            TrelloBoard? board = await boardsCollection.Find(boardFilter).FirstOrDefaultAsync();

            if (board is not null)
            {
                var m = board.Members.Find(m => m.Id == member.Id);
                if (m is not null)
                {
                    member.Name = m.Name;
                }
            }

            if (card == null)
            {
                card = new TrelloCard(cardData, list, member, cardData.Board.Id);

                if (board == null)
                {
                    board = new TrelloBoard(cardData.Board.Id, cardData.Board.Name);
                    try
                    {
                        boardsCollection.InsertOne(board);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                if (list.Name.ToLower().Contains(board.StopwatchList))
                {
                    card.Timers.Add(new CardTime(member, list));
                }
                await cardsCollection.InsertOneAsync(card);

                var update = Builders<TrelloBoard>.Update.Push("Cards", card.Id);
                await boardsCollection.FindOneAndUpdateAsync(boardFilter, update);
            }
            else
            {
                card.Name = cardData.Card.Name;
                var lastTime = card.Timers.LastOrDefault();
                if (lastTime != null)
                {
                    switch (lastTime.State)
                    {
                        case TimeState.STOPPED:
                            if (list.Name.ToLower().Contains(board.StopwatchList) && card.CurrentList != list)
                            {
                                card.Timers.Add(new CardTime(member, list));
                            }
                            break;
                        case TimeState.PAUSED:
                            if (!list.Name.ToLower().Contains(board.StopwatchList))
                            {
                                lastTime.State = TimeState.STOPPED;
                            }
                            break;
                        case TimeState.RUNNING:
                            if (lastTime.List.Id != list.Id)
                            {
                                lastTime.End = DateTime.UtcNow;
                                lastTime.EndMember = member;
                                lastTime.State = TimeState.STOPPED;
                            }
                            break;
                        default:
                            break;
                    }
                    card.CurrentList = list;
                }
                else
                {
                    if (list.Name.ToLower().Contains(board.StopwatchList))
                    {
                        card.Timers.Add(new CardTime(member, list));
                    }
                }
                await cardsCollection.FindOneAndReplaceAsync(cardFilter, card);
            }
            var cardTagPattern = CardUtils.MatchTagPattern(card.Name);
            if (cardTagPattern.Type == CardTagType.NONE)
            {
                return new JsonResult(new { });
            }
            return new JsonResult(card);
        }

        [HttpPost("pause-stopwatch")]
        public async void PauseStopwatch(CardData cardData)
        {
            var cardFilter = Builders<TrelloCard>.Filter.Eq("Id", cardData.Card.Id);
            var card = await CardsCollection().Find(cardFilter).FirstOrDefaultAsync();

            cardData.List.Get(out TrelloList list);
            cardData.Member.Get(out TrelloMember member);

            if (card != null)
            {
                var timer = card.Timers.Find(t => t.State == TimeState.RUNNING);
                if (timer != null)
                {
                    timer.State = TimeState.PAUSED;
                    timer.End = DateTime.UtcNow;
                    timer.EndMember = member;
                }
            }

            await CardsCollection()!.FindOneAndReplaceAsync(cardFilter!, card);
        }

        [HttpPost("resume-stopwatch")]
        public async void ResumeStopwatch(CardData cardData)
        {
            var collection = BoardsCollection();

            var boardFilter = Builders<TrelloBoard>.Filter.Eq("Id", cardData.Board.Id);
            var board = await collection.Find(boardFilter).FirstOrDefaultAsync();

            var cardFilter = Builders<TrelloCard>.Filter.Eq("Id", cardData.Card.Id);
            var card = await CardsCollection().Find(cardFilter).FirstOrDefaultAsync();

            cardData.List.Get(out TrelloList list);
            cardData.Member.Get(out TrelloMember member);

            if (board != null)
            {
                if (list.Name.ToLower().Contains(board.StopwatchList))
                {
                    if (card != null)
                    {
                        var lastTimer = card.Timers.Last();
                        if (lastTimer != null && lastTimer.State == TimeState.PAUSED)
                        {
                            lastTimer.State = TimeState.STOPPED;
                            var nTimer = new CardTime(member, list);
                            card.Timers.Add(nTimer);
                            await CardsCollection().FindOneAndReplaceAsync(cardFilter, card);
                        }
                    }
                }

            }
        }
        [HttpPatch("update-card-time")]
        public async Task<IActionResult> UpdateCardTime(UpdateCardTimeData upData)
        {

            var collection = CardsCollection();
            var cardFilter = Builders<TrelloCard>.Filter.Eq("Id", upData.CardId);
            var card = await collection.Find(cardFilter).FirstOrDefaultAsync();

            if (card != null)
            {
                var time = card.Timers.Find(t => t.Id == upData.TimeId);
                if (time != null)
                {
                    time.Start = upData.Start;
                    if (time.State != TimeState.RUNNING)
                    {
                        if (upData.Start.CompareTo(upData.End) > 0)
                        {
                            return new JsonResult(new { failed = true, message = "Data de termino n�o pode ser inferior � data de Inicio" });
                        }
                        time.End = upData.End;
                    }
                    await collection.FindOneAndReplaceAsync(cardFilter, card);
                    return new JsonResult(new { failed = false, message = "OK" });
                }
            }
            return new JsonResult(new { failed = true, message = "Falha ao encontrar Cartão ou Cronometro especificado" });
        }

        [HttpGet("card")]
        public async Task<IActionResult> GetCard(string cardId)
        {
            var cardsCollection = CardsCollection();
            var cardFilter = Builders<TrelloCard>.Filter.Eq("Id", cardId);
            var card = await cardsCollection.Find(cardFilter).FirstOrDefaultAsync();

            return new JsonResult(card);
        }

    }
}