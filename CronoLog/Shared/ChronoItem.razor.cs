using CronoLog.Models;
using CronoLog.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CronoLog.Shared
{
    public partial class ChronoItem
    {
        [Inject]
        public MongoClient DbClient { get; set; }
        [Parameter]
        public bool FirstClick { get; set; }

        [Parameter]
        public CardTime Chrono { get; set; }

        [Parameter]
        public string CardId { get; set; }

        [Parameter]
        public List<TrelloMember> Members { get; set; }

        [Parameter]
        public string MemberId { get; set; }

        [Parameter]
        public Action<string, string, string> RemoveChrono { get; set; }

        public TrelloMember SelectedMember { get; set; }

        public UpdateChronoRequest updateRequest = new();

        private bool isEditMode = false;
        private bool requestHiddenConfirmation = false;
        protected override async Task OnInitializedAsync()
        {

            updateRequest.CardId = CardId;
            SelectedMember = Chrono.StartMember;
            updateRequest.MemberId = Chrono.StartMember.Id;
            updateRequest.RequestMemberId = MemberId;

            Chrono.Start = Chrono.Start.AddSeconds(-Chrono.Start.Second);
            Chrono.End = Chrono.End.AddSeconds(-Chrono.End.Second);
            updateRequest.ChronoId = Chrono.Id;
            updateRequest.Start = Chrono.Start;
            updateRequest.End = Chrono.End;

            await base.OnInitializedAsync();
        }

        public async void SaveBtn_()
        {
            var updateData = updateRequest;
            var cardsCollection = DatabaseUtils.CardsCollection(DbClient);
            var cardFilter = Builders<TrelloCard>.Filter.Eq("Id", updateData.CardId);
            var card = await cardsCollection.Find(cardFilter).FirstOrDefaultAsync();

            if (card != null)
            {
                var chrono = card.Timers.Find((t) => t.Id == updateData.ChronoId);
                if (chrono != null)
                {
                    var boardsCollection = DatabaseUtils.BoardsCollection(DbClient);
                    var boardFilter = Builders<TrelloBoard>.Filter.Eq("Id", card.BoardId);
                    var board = await boardsCollection.Find(boardFilter).FirstOrDefaultAsync();
                    var updateMember = board.Members.Find((m) => m.Id == updateData.MemberId);

                    chrono.StartMember = updateMember;
                    chrono.Start = DateUtils.ToDbSaveTime(updateData.Start);
                    if (chrono.State != TimeState.RUNNING)
                    {
                        chrono.End = DateUtils.ToDbSaveTime(updateData.End);
                    }
                    await cardsCollection.FindOneAndReplaceAsync(cardFilter, card);

                    //return new JsonResult("Alteraç\uc3a3o salva com sucesso!");
                    await js.InvokeVoidAsync("alert", "Alteração salva com sucesso.");
                }
                else
                {
                    //return new JsonResult("Timer n\uc3a3o encontrado");
                    await js.InvokeVoidAsync("alert", "Não foi possivel alterar esse Timer, erro na identificação do Timer");
                }
            }
            else
            {
                //return new JsonResult("Cart\uc3a3o n\uc3a3o encontrado");
                await js.InvokeVoidAsync("alert", "Não foi possivel alterar esse Timer, erro na identificação do cartão");
            }
        }

        public async void SaveBtn()
        {
            updateRequest.Start = updateRequest.Start.ToUniversalTime();
            updateRequest.End = updateRequest.End.ToUniversalTime();
            var json = JsonSerializer.Serialize(updateRequest);
            updateRequest.Start = Utils.DateUtils.ToBrSpTimezone(updateRequest.Start);
            updateRequest.End = Utils.DateUtils.ToBrSpTimezone(updateRequest.End);

            StringContent content = new(json);

            Console.WriteLine(json);

            var response = await httpC.PostAsJsonAsync($"{ApiUtils.API_URL}/webapp/chrono", updateRequest);
            var responseString = await response.Content.ReadFromJsonAsync<string>();
            var alertString = "";
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(responseString);
                if (responseString == "ascs")
                {
                    alertString = "Alteração salva com sucesso";
                }
                if (responseString == "nptvc")
                {
                    alertString = "Não é permitido alterar Timers que não pertencem a você";
                }
                if (responseString == "tne")
                {
                    alertString = "Timer não encontrado";
                }
                if (responseString == "cne")
                {
                    alertString = "Cartão não encontrado";
                }
                Console.WriteLine(alertString);
            }
            await js.InvokeVoidAsync("alert", alertString);

        }
        public async void DeleteChrono_()
        {
            var cardFilter = Builders<TrelloCard>.Filter.Eq("Id", CardId);
            var card = await DatabaseUtils.CardsCollection(DbClient).Find(cardFilter).FirstOrDefaultAsync();
            if (card != null)
            {
                var timer = card.Timers.Find(t => t.Id == Chrono.Id);
                if (timer != null)
                {
                    card.Timers.Remove(timer);
                    await DatabaseUtils.CardsCollection(DbClient).FindOneAndReplaceAsync(cardFilter, card);
                    RemoveChrono(CardId, MemberId, Chrono.Id);
                    await js.InvokeVoidAsync("alert", "Timer deletado com sucesso");
                }
                else
                {
                    await js.InvokeVoidAsync("alert", "Não foi possivel excluir esse Timer, ele não foi encontrado");
                }
            }
            else
            {
                await js.InvokeVoidAsync("alert", "Não foi possivel excluir esse Timer, identificação do cartão invalida");
            }
        }
        public async void DeleteChrono()
        {
            var response = await httpC.DeleteAsync($"{ApiUtils.API_URL}/webapp/chrono/{CardId}/{Chrono.Id}/{MemberId}");
            var responseString = await response.Content.ReadFromJsonAsync<string>();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var alertString = "";
                Console.WriteLine(responseString);
                if (responseString == "ascs")
                {
                    alertString = "Alterado com sucesso";
                    await js.InvokeVoidAsync("alert", "Alterado com sucesso");
                    RemoveChrono(CardId, MemberId, Chrono.Id);
                }
                if (responseString == "nptvc")
                {
                    alertString = "Não é permitido alterar Timers que não pertencem a você";
                    await js.InvokeVoidAsync("alert", "Não é permitido alterar Timers que não pertencem a você");
                }
                if (responseString == "tne")
                {
                    alertString = "Timer não encontrado";
                    await js.InvokeVoidAsync("alert", alertString);
                }
                if (responseString == "cne")
                {
                    alertString = "Cartão não encontrado";
                    await js.InvokeVoidAsync("alert", alertString);
                }
                Console.WriteLine(alertString);
            }

        }
    }
}
