using BlazorDownloadFile;
using CronoLog.Models;
using CronoLog.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CronoLog.Pages
{
    public partial class Board
    {
        [Parameter]
        public string? BoardId { get; set; }
        [Parameter]
        public string? MemberId { get; set; }
        [Inject]
        public IMongoClient DbClient { get; set; }
        [Inject]
        IBlazorDownloadFileService? BlazorDownloadFileService { get; set; }
        [Inject]
        public IJSRuntime? Js { get; set; }
        [Inject]
        public NavigationManager? Navigation { get; set; }

        public FullBoardData? BoardData { get; set; }
        protected string? CurrentCardId { get; set; }
        protected string? CurrentMemberId { get; set; }

        private Dictionary<string, Dictionary<string, List<CardTime>>>? CardMemberTimers { get; set; }
        public bool firstClick = true;
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            CurrentCardId = "";
            CurrentMemberId = "";
            CardMemberTimers = new();
            if (BoardId != string.Empty)
            {
                await GetFullBoardDataMulti();
                foreach (var card in BoardData.Cards)
                {
                    if (!CardMemberTimers.ContainsKey(card.Id))
                    {
                        CardMemberTimers.Add(card.Id, new());
                    }
                    foreach (var timer in card.Timers)
                    {
                        if (!CardMemberTimers[card.Id].ContainsKey(timer.StartMember.Id))
                        {
                            CardMemberTimers[card.Id].Add(timer.StartMember.Id, new(card.Timers.Count + 1));
                        }

                        CardMemberTimers[card.Id][timer.StartMember.Id].Add(timer);
                    }
                }

            }
        }

        private async Task GetFullBoardDataMulti()
        {
            var boardFilter = Builders<TrelloBoard>.Filter.Eq("Id", BoardId);
            var board = await DatabaseUtils.BoardsCollection(DbClient).Find(boardFilter).FirstOrDefaultAsync();
            BoardData = new FullBoardData
            {
                BoardId = board.Id,
                BoardName = board.Name,
                Members = board.Members
            };
            var cardsFilter = Builders<TrelloCard>.Filter.Eq("BoardId", BoardId);
            var cards = await DatabaseUtils.CardsCollection(DbClient).Find(cardsFilter).ToListAsync();
            var cardsList = new List<TrelloCard>();

            foreach (var card in cards)
            {
                if (!card.CurrentList.Name.ToLower().Contains("geral") && !card.CurrentList.Name.ToLower().Contains("dúvidas") && !card.CurrentList.Name.ToLower().Contains("duvidas") && card.Active)
                {
                    foreach (var cTimer in card.Timers)
                    {
                        cTimer.Start = DateUtils.ToBrSpTimezone(cTimer.Start);
                        cTimer.End = DateUtils.ToBrSpTimezone(cTimer.End);
                    }
                    cardsList.Add(card);
                }
            }
            BoardData.Cards = cardsList;
        }

        public async void DownloadExcel()
        {
            var bytes = Controllers.WebAppController.GetExcelLocal(BoardId, DbClient, out string fileName);
            var t = await BlazorDownloadFileService.DownloadFile(fileName, bytes, CancellationToken.None, "application/octet-stream");
        }

        private void RemoveChronoFromList(string cardId, string memberId, string chronoId)
        {
            var timer = CardMemberTimers[cardId][MemberId].Find((timer) => timer.Id == chronoId);
            CardMemberTimers[cardId][memberId].Remove(timer);
            StateHasChanged();
        }

        private void SetCurrentSelected(TrelloCard card)
        {
            if (CurrentCardId != card.Id)
            {
                CurrentCardId = card.Id;
                CurrentMemberId = string.Empty;
            }
            StateHasChanged();
        }

        private void SetCurrentMember(TrelloCard card, TrelloMember member)
        {
            firstClick = false;
            CurrentMemberId = (CurrentMemberId == member.Id) ? string.Empty : member.Id;
            CurrentCardId = card.Id;
        }
        private async void RefreshPage()
        {
            BoardData = null;
            await OnInitializedAsync();
            StateHasChanged();
        }
    }
}
