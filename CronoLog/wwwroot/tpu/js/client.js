/* global TrelloPowerUp */
var Promise = TrelloPowerUp.Promise;

TrelloPowerUp.initialize({
    "board-buttons": InitBoardButtons,
    'card-buttons': InitCardButtons,
    'card-detail-badges': cardDetailBadges,
    "card-badges": cardBadges
});

async function cardDetailBadges(t, options) {
    let list = await t.list("id", "name")
    let trueMember = await t.member("id", "fullName")
    let member = { "id": trueMember.id, "name": trueMember.fullName }
    let board = await t.board("id", "name")
    let card = await t.card("id", "name")
    let dados = {
        card,
        member,
        list,
        board,
    }

    let cardResponse = await fetch(TRELLO_TIMER.API_URL + "/CardTime/time-card", {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        method: 'POST',
        body: JSON.stringify(dados)
    })
    let responseJson = await cardResponse.json()

    if (responseJson === undefined || responseJson.timers === undefined || responseJson.timers.length === 0) {
        return [];
    }

    let currentTimer = responseJson.timers[responseJson.timers.length - 1]
    if (currentTimer === undefined) {
        return []
    }

    let initTime = new Date(currentTimer["start"])

    let totalDuration = 0;
    responseJson.timers.forEach((val) => {
        if (val.state === TimerState.STOPPED || val.state === TimerState.PAUSED) {
            let startTime = new Date(val.start)
            let endTime = new Date(val.end)

            let thisDuration = endTime.getTime() - startTime.getTime()
            totalDuration += thisDuration
        }
    });

    if (currentTimer.state === TimerState.STOPPED || currentTimer.state === TimerState.PAUSED) {
        let color

        switch (currentTimer.state) {
            case TimerState.STOPPED:
                color = "red"
                break
            case TimerState.PAUSED:
                color = "orange"
                break
        }

        if (list.name.toLowerCase().includes("finalizado")) {
            color = "green"
        }

        let returnArray = []
        let listName = list.name.toLowerCase()
        if (!listName.includes("dúvidas") && !listName.includes("duvidas") && !listName.includes("geral")) {
            returnArray.push({
                dynamic: async function () {
                    await t.get('card', 'shared', 'shouldUpdate', false);
                    await t.set('card', 'shared', 'shouldUpdate', false);
                    return {
                        title: 'Cronômetro',
                        icon: Icons.timer,
                        text: `${getDurationStringHours(totalDuration)}`,
                        color
                    }
                }
            })
        }
        return returnArray
    }

    let returnArray = []

    let listName = list.name.toLowerCase()
    if (!listName.includes("dúvidas") && !listName.includes("duvidas") && !listName.includes("geral")) {
        returnArray.push({
            dynamic: async function () {
                await t.get('card', 'shared', 'shouldUpdate', false);
                await t.set('card', 'shared', 'shouldUpdate', false);
                let currentDate = Date.now();
                let duration = getDurationStringHours((currentDate - initTime.getTime()) + totalDuration);
                return {
                    title: 'Cronômetro',
                    icon: Icons.timer,
                    text: `${duration}`,
                    color: "blue",
                    refresh: 30
                }
            }
        })
    }
    return returnArray
}

async function cardBadges(t, options) {
    let list = await t.list("id", "name");
    let trueMember = await t.member("id", "fullName");
    let member = { "id": trueMember.id, "name": trueMember.fullName }
    let board = await t.board("id", "name");
    let card = await t.card("id", "name");
    let dados = {
        card,
        member,
        list,
        board,
    }

    //CardBadgeInfo -- remoção temporaria
    let cardBadgeInfoResponse;
    try {
        cardBadgeInfoResponse = await fetch(TRELLO_TIMER.API_URL + "/CardInfoBadge/" + card.id, {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: 'GET',
        });
    } catch (error) {
    }

    let cardBadge = [];
    if (cardBadgeInfoResponse.status !== 404) {
        let cardBadgeInfo = await cardBadgeInfoResponse.json();
        if (cardBadgeInfo) {
            if (cardBadgeInfo.descriptions) {
                cardBadge = cardBadgeInfo.descriptions.map((d) => {
                    return {
                        dynamic: async function () {
                            await t.get('card', 'shared', 'shouldUpdate', false);
                            await t.set('card', 'shared', 'shouldUpdate', false);
                            return {
                                text: d,
                                color: 'light-gray'
                            }
                        }
                    }
                })
            }
        }
    }
    //CardBadgeInfo ^
    console.log(JSON.stringify(dados))
    console.log(dados)
    let cardResponse = await fetch(TRELLO_TIMER.API_URL + "/CardTime/time-card", {
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        method: 'POST',
        body: JSON.stringify(dados)
    });
    let responseJson = await cardResponse.json();

    if (responseJson === undefined || responseJson.timers === undefined || responseJson.timers.length === 0) {
        return cardBadge;
    }

    let currentTimer = responseJson.timers[responseJson.timers.length - 1];
    if (currentTimer === undefined) {
        return cardBadge;
    }

    let initTime = new Date(currentTimer["start"]);

    let totalDuration = 0;
    responseJson.timers.forEach((val) => {
        if (val.state === TimerState.STOPPED || val.state === TimerState.PAUSED) {
            let startTime = new Date(val.start);
            let endTime = new Date(val.end);

            let thisDuration = endTime.getTime() - startTime.getTime();
            totalDuration += thisDuration;
        }
    });



    if (currentTimer.state === TimerState.STOPPED || currentTimer.state === TimerState.PAUSED) {
        let color;

        switch (currentTimer.state) {
            case TimerState.STOPPED:
                color = "red";
                break
            case TimerState.PAUSED:
                color = "orange";
                break
        }

        if (list.name.toLowerCase().includes("finalizado")) {
            color = "green";
        }

        let returnArray = []
        let listName = list.name.toLowerCase()
        if (!listName.includes("dúvidas") && !listName.includes("duvidas") && !listName.includes("geral")) {
            returnArray.push({
                dynamic: async function () {
                    await t.get('card', 'shared', 'shouldUpdate', false);
                    await t.set('card', 'shared', 'shouldUpdate', false);
                    return {
                        icon: Icons.timer,
                        text: `${getDurationStringHours(totalDuration)}`,
                        color
                    }
                }
            });
        }
        cardBadge.forEach(b => returnArray.push(b))
        return returnArray
    }

    let returnArray = []

    let listName = list.name.toLowerCase()
    if (!listName.includes("dúvidas") && !listName.includes("duvidas") && !listName.includes("geral")) {
        returnArray.push({
            dynamic: async function () {
                await t.get('card', 'shared', 'shouldUpdate', false);
                await t.set('card', 'shared', 'shouldUpdate', false);
                let currentDate = Date.now();
                let duration = getDurationStringHours((currentDate - initTime.getTime()) + totalDuration);
                return {
                    icon: Icons.timer,
                    text: `${duration}`,
                    color: "blue",
                    refresh: 30
                }
            }
        });
    }
    cardBadge.forEach(b => returnArray.push(b));
    return returnArray
}


function getDuration(milli) {
    let minutes = Math.floor(milli / 60000);
    minutes = (minutes === -1) ? 0 : minutes;
    let hours = Math.floor(minutes / 60);
    let days = Math.floor(hours / 24);
    minutes = minutes % 60;
    hours = hours % 24;
    return {
        minutes, hours, days
    }
};
function getDurationHours(milli) {
    let minutes = Math.floor(milli / 60000);
    minutes = (minutes === -1) ? 0 : minutes;
    let hours = Math.floor(minutes / 60);
    minutes = minutes % 60;
    return {
        minutes, hours
    }
};

function getDurationStringHours(miliseconds) {
    let duration = getDurationHours(miliseconds);

    let minutes = (duration.minutes < 10) ? `0${duration.minutes}` : `${duration.minutes}`;
    let hours = (duration.hours < 10) ? `0${duration.hours}` : `${duration.hours}`;

    return `${hours}h ${minutes}m`;
}

function getDurationString(miliseconds) {
    let duration = getDuration(miliseconds);

    let minutes = (duration.minutes < 10) ? `0${duration.minutes}` : `${duration.minutes}`;
    let hours = (duration.hours < 10) ? `0${duration.hours}` : `${duration.hours}`;
    let days = (duration.days < 10) ? `0${duration.days}` : `${duration.days}`;

    return `${days}D ${hours}:${minutes}`;
}

const TimerState = { RUNNING: 0, STOPPED: 1, PAUSED: 2, FINALIZED: 3 };

async function InitBoardButtons(t, opts) {
    let board = await t.board("id", "name", "members");
    let cards = await t.cards("id", "name");
    let member = await t.member("id", "fullName");
    let members = board.members.map((member) => { return { id: member.id, name: member.fullName } });
    let jsonBody = JSON.stringify({ id: board.id, name: board.name, members, cards });

    let response = await fetch(`${TRELLO_TIMER.API_URL}/CardTime/board-load`, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: jsonBody
    });
    return [{
        // we can either provide a button that has a callback function
        icon: Icons.page_next,
        text: 'Informações do Quadro',
        title: "Informações do Quadro",
        url: `${TRELLO_TIMER.API_URL}/board/${board.id}/${member.id}`,
        condition: 'edit'
    }]
}

async function InitCardButtons(t, options) {
    return [{
        icon: Icons.pause,
        text: 'Pausar Cronometro',
        callback: async (t) => {
            let list = await t.list("id", "name");
            let trueMember = await t.member("id", "fullName");
            let member = { "id": trueMember.id, "name": trueMember.fullName }
            let board = await t.board("id", "name");
            let card = await t.card("id", "name");
            let dados = {
                card,
                member,
                list,
                board,
            }
            let cardResponse = await fetch(`${TRELLO_TIMER.API_URL}/CardTime/pause-stopwatch`, {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dados)
            });
            await t.set('card', 'shared', 'shouldUpdate', true);
            // return t.popup({
            //     title: "TODO",
            //     url: 'timerFrame.html',
            // });
        }
    }, {
        icon: Icons.play,
        text: 'Resumir Cronometro',
        callback: async (t) => {
            let list = await t.list("id", "name");
            let trueMember = await t.member("id", "fullName");
            let member = { "id": trueMember.id, "name": trueMember.fullName }
            let board = await t.board("id", "name");
            let card = await t.card("id", "name");
            let dados = {
                card,
                member,
                list,
                board,
            }
            let cardResponse = await fetch(`${TRELLO_TIMER.API_URL}/CardTime/resume-stopwatch`, {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dados)
            });
            await t.set('card', 'shared', 'shouldUpdate', true);
            // return t.popup({
            //     title: "TODO",
            //     url: 'timerFrame.html',
            // });
        }
    },
    {
        icon: Icons.wrench,
        text: "Editar Tempo",
        callback: (t, options) => {
            return t.popup({
                title: "Editar Cartão",
                url: "editCard.html"
            })
        }
    },
    {
        icon: Icons.info,
        text: "Comentários",
        callback: async (t, options) => {
            await t.set('card', 'shared', 'updateDesc', true);
            return t.popup({
                title: "Comentários",
                url: "editCardInfo.html"
            })
        }
    }
    ];
}
