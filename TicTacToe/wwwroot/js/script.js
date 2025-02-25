"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/game").build();

var gamer = document.getElementById("gamer");
var findOpponent = document.getElementById("findOpponent");
var register = document.getElementById("register");
var game = document.getElementById("game");
var findAnotherGame = document.getElementById("findAnotherGame");
var gameInformation = document.getElementById("gameInformation");
var waitingForOpponent = document.getElementById("waitingForOpponent");

findAnotherGame.style.display = "none";
register.style.display = "block";
waitingForOpponent.style.display = "none";
game.style.display = "none";
findOpponent.style.display = "none";

document.getElementById("registerName").addEventListener("click", function () {
    connection.invoke('registerClient', gamer.value);
    register.style.display = "none";
    findOpponent.style.display = "block";
});

document.getElementsByClassName("findGame")[0].addEventListener("click", findGame);

function findGame() {
    connection.invoke('findOpponent');
    waitingForOpponent.style.display = "block";
    register.style.display = "none";
    findOpponent.style.display = "none";
}

connection.on('noOpponents', function (message) {
    document.getElementById("information").innerHTML = "Szukanie przeciwnika";
});

findAnotherGame.addEventListener("click", function () {
    gameInformation.innerHTML = "";
    game.style.display = "none";
    findAnotherGame.style.display = "none";
    connection.invoke('registerClient', gamer.value);

    findGame();
});

connection.on('waitingForOpponent', function (message) {
    document.getElementById("information").innerHTML = "Czekaj na ruch przeciwnika";
});

connection.on('waitingForMarkerPlacement', function (message) {
    document.getElementById("information").innerHTML = "Twój ruch";
});

connection.on('foundOpponent', function (message1, message2) {
    findAnotherGame.style.display = "none";
    waitingForOpponent.style.display = "none";
    gameInformation.innerHTML = "Witaj <b>" + message2 + "</b> grasz przeciwko " + message1;

    game.innerHTML = "<div id='information'></div><br/>";
    for (var i = 0; i < 9; i++) {
        var div = document.createElement("div");
        div.id = i;
        div.classList.add("box");

        switch (i)
        {
            case 0:
                div.style.borderBottom = "rgb(77, 77, 77) solid 5px";
                div.style.borderRight = "rgb(77, 77, 77) solid 5px";
                div.style.top = "300px";
                div.style.left = "130px";
                break;
            case 1:
                div.style.borderLeft = "rgb(77, 77, 77) solid 5px";
                div.style.borderRight = "rgb(77, 77, 77) solid 5px";
                div.style.borderBottom = "rgb(77, 77, 77) solid 5px";
                div.style.top = "300px";
                div.style.left = "260px";
                break;
            case 2:
                div.style.borderBottom = "rgb(77, 77, 77) solid 5px";
                div.style.top = "300px";
                div.style.left = "390px";
                break;
            case 3:
                div.style.borderRight = "rgb(77, 77, 77) solid 5px";
                div.style.borderBottom = "rgb(77, 77, 77) solid 5px";
                div.style.top = "430px";
                div.style.left = "130px";
                break;
            case 4:
                div.style.borderLeft = "rgb(77, 77, 77) solid 5px";
                div.style.borderTop = "rgb(77, 77, 77) solid 5px";
                div.style.borderRight = "rgb(77, 77, 77) solid 5px";
                div.style.top = "430px";
                div.style.left = "260px";
                break;
            case 5:
                div.style.borderBottom = "rgb(77, 77, 77) solid 5px";
                div.style.top = "430px";
                div.style.left = "390px";
                break;
            case 6:
                div.style.borderTop = "rgb(77, 77, 77) solid 5px";
                div.style.borderRight = "rgb(77, 77, 77) solid 5px";
                div.style.top = "560px";
                div.style.left = "130px";
                break;
            case 7:
                div.style.borderTop = "rgb(77, 77, 77) solid 5px";
                div.style.borderRight = "rgb(77, 77, 77) solid 5px";
                div.style.borderLeft = "rgb(77, 77, 77) solid 5px";
                div.style.top = "560px";
                div.style.left = "260px";
                break;
            case 8:
                div.style.borderTop = "rgb(77, 77, 77) solid 5px";
                div.style.top = "560px";
                div.style.left = "390px";
                break;
        }
        game.appendChild(div);
    }

    game.style.display = "inline";
});

connection.on('addMarkerPlacement', function (message) {
    if (message.opponentName !== gamer.value) {
        document.getElementById(message.markerPosition).classList.add("mark2");
        document.getElementById(message.markerPosition).classList.add("marked");
        document.getElementById("information").innerHTML = "Twój ruch";
    }
    else {
        document.getElementById(message.markerPosition).classList.add("mark1");
        document.getElementById(message.markerPosition).classList.add("marked");
        document.getElementById("information").innerHTML = "Czekaj na ruch przeciwnika";
    }
});

connection.on('opponentDisconnected', function (message) {
    gameInformation.innerHTML = "Koniec gry! " + message + " odszedł z gry i wygrałeś";

    findAnotherGame.style.display = "block";
    game.style.display = "none";
});

connection.on('gameOver', function (message) {
    document.getElementById("information").innerHTML = "Koniec gry. Grę wygrał: " + message + "";
    findAnotherGame.style.display = "block";
});

document.addEventListener('click', function (event) {
    if (event.target.className === "box") {
        if (event.target.classList.contains("marked")) return;
        connection.invoke('play', parseInt(event.target.id));
    }
});

connection.start();