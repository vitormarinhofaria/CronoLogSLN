async function main() {
    let cardName = document.querySelector("#cardName");
    let timersContainer = document.querySelector("#timersContainer");

    let t = window.TrelloPowerUp.iframe();
    let cardCtxData = await t.card('id', 'name');
    let listName = await t.list('id', 'name');

    let cardDataReq = await fetch(`${TRELLO_TIMER.API_URL}/CardTime/card?cardId=${cardCtxData.id}`);
    let cardData = await cardDataReq.json();

    cardName.textContent = cardData.name;

    if (cardData.timers !== undefined && cardData.length !== 0) {
        cardData.timers.forEach(e => {
            CreateTimerNode(e, listName, cardData, timersContainer, t);
        });
    }
}
main();

function CreateTimerNode(time, list, card, container, trelloContext) {
    let timeDateStart = new Date(time.start);
    let timeDateEnd = new Date(time.end);

    let memberName = document.createElement("label");
    memberName.textContent = time.startMember.name;
    memberName.style = "font-style: italic; padding: 3px; align-self: center;"

    let tContainer = document.createElement("div");
    tContainer.className = "timerNode nodeBase";

    let startLabel = document.createElement("div");
    let startDate = document.createElement("input");
    let startHour = document.createElement("input");
    startDate.classList.add("txtDateInput");
    startHour.classList.add("txtDateInput");
    let cTimeDay = (timeDateStart.getDate() < 10) ? `0${timeDateStart.getDate()}` : `${timeDateStart.getDate()}`;
    let cTimeMonth = (timeDateStart.getUTCMonth() < 9) ? `0${timeDateStart.getUTCMonth() + 1}` : `${timeDateStart.getUTCMonth() + 1}`;
    startDate.value = `${cTimeDay}/${cTimeMonth}/${timeDateStart.getFullYear()}`;
    let cTimeHour = (timeDateStart.getHours() < 10) ? `0${timeDateStart.getHours()}` : `${timeDateStart.getHours()}`;
    let cTimeMinutes = (timeDateStart.getMinutes() < 10) ? `0${timeDateStart.getMinutes()}` : `${timeDateStart.getMinutes()}`;
    startHour.value = `${cTimeHour}:${cTimeMinutes}`;
    startDate.maxLength = 10
    startHour.maxLength = 5
    startLabel.appendChild(startDate);
    startLabel.appendChild(startHour);
    startLabel.classList.add("nodeBase");
    startLabel.classList.add("dateDiv");
    let startDateVal = startDate.value;
    startDate.addEventListener("keyup", (event) => { startDateVal = event.target.value });
    let startHourVal = startHour.value;
    startHour.addEventListener("keyup", (event) => { startHourVal = event.target.value });

    let endLabel = document.createElement("div");
    let endDate = document.createElement("input");
    let endHour = document.createElement("input");
    endDate.classList.add("txtDateInput");
    endHour.classList.add("txtDateInput");
    let cTimeDayEnd = (timeDateEnd.getDate() < 10) ? `0${timeDateEnd.getDate()}` : `${timeDateEnd.getDate()}`;
    let cTimeMonthEnd = (timeDateEnd.getUTCMonth() < 9) ? `0${timeDateEnd.getUTCMonth() + 1}` : `${timeDateEnd.getUTCMonth() + 1}`;
    endDate.value = `${cTimeDayEnd}/${cTimeMonthEnd}/${timeDateEnd.getFullYear()}`;
    let cTimeHourEnd = (timeDateEnd.getHours() < 10) ? `0${timeDateEnd.getHours()}` : `${timeDateEnd.getHours()}`;
    let cTimeMinutesEnd = (timeDateEnd.getMinutes() < 10) ? `0${timeDateEnd.getMinutes()}` : `${timeDateEnd.getMinutes()}`;
    endHour.value = `${cTimeHourEnd}:${cTimeMinutesEnd}`;
    endDate.maxLength = 10
    endHour.maxLength = 5
    endLabel.appendChild(endDate);
    endLabel.appendChild(endHour);
    endLabel.classList.add("nodeBase");
    endLabel.classList.add("dateDiv");
    let endDateVal = endDate.value;
    endDate.addEventListener("keyup", (event) => { endDateVal = event.target.value });
    let endHourVal = endHour.value;
    endHour.addEventListener("keyup", (event) => { endHourVal = event.target.value });

    tContainer.appendChild(startLabel);
    tContainer.appendChild(endLabel);

    let statusLabel = document.createElement("div");
    statusLabel.classList.add("nodeBase");
    statusLabel.classList.add("timeStatus");
    switch (time.state) {
        case 0:
            statusLabel.classList.add("running");
            break;
        case 1:
            if (list.name.includes("Finalizado")) {
                statusLabel.classList.add("finalized");
            } else {
                statusLabel.classList.add("stopped");
            }
            break;
        case 2:
            statusLabel.classList.add("paused");
        default:
            break;
    }
    if (time.state === 0) {
        endDate.value = "..."
        endHour.value = "..."
    }
    tContainer.appendChild(statusLabel);

    let saveButton = document.createElement("a");
    let btnIcon = document.createElement("img");
    btnIcon.src = Icons.check;
    btnIcon.width = 25;
    btnIcon.height = 25;
    btnIcon.className = "sucIcon";
    saveButton.appendChild(btnIcon);
    saveButton.addEventListener("click", async (e) => {
        let [startDay, startMonth, startYear] = startDateVal.trim().split("/");
        let [startHour, startMinutes] = startHourVal.trim().split(":");

        let [endDay, endMonth, endYear] = endDateVal.trim().split("/");
        let [endHour, endMinutes] = endHourVal.trim().split(":");

        let mStartTime = new Date(Date.parse(`${startMonth} ${startDay} ${startYear} ${startHour}:${startMinutes}:01 GMT-0300`));
        console.log(mStartTime);
        let mEndTime = new Date(Date.parse(`${endMonth} ${endDay} ${endYear} ${endHour}:${endMinutes}:01 GMT-0300`));
        console.log(mEndTime);

        if (isNaN(mStartTime.getTime()) || isNaN(mEndTime.getTime())) {
            alert("Data invalida\n Utilize o formato DD/MM/AAAA  hh:mm");
            return;
        }

        let changeTimeRequest = {
            timeId: time.id,
            cardId: card.id,
            start: mStartTime.toISOString(),
            end: mEndTime.toISOString(),
            state: time.state,
        }

        let success = await PostCardEdit(changeTimeRequest, trelloContext);
        if (success === true) {
            e.target.classList.add("suc");
            setTimeout(() => {e.target.classList.remove("suc")}, 1700);
        } else {
            e.target.classList.add("fail");
            setTimeout(() => {e.target.classList.remove("fail")}, 1700);
        }
    });
    tContainer.appendChild(saveButton);

    container.appendChild(memberName);
    container.appendChild(tContainer);

    async function PostCardEdit(changesObject, trelloContext) {
        document.body.style.cursor = 'progress';
        let response = await fetch(TRELLO_TIMER.API_URL + "/CardTime/update-card-time", {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: 'PATCH',
            body: JSON.stringify(changesObject)
        });
        if(response.status !== 200){
            console.log(response);
        }
        let respJson = await response.json();
        document.body.style.cursor = 'default';
        if (respJson.failed === false) {
            // alert(`Alteração salva!`);
            console.log(respJson.message)
            await trelloContext.set('card', 'shared', 'shouldUpdate', true);
            return true;
        }else{
            console.log(respJson.message)
            return false;
        }
    }
}
const BLACK_ROCKET_ICON = 'https://cdn.glitch.com/1b42d7fe-bda8-4af8-a6c8-eff0cea9e08a%2Frocket-ship.png?1494946700421';