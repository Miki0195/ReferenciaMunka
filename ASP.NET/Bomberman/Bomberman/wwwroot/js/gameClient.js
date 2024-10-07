import * as Assets from "../lib/bomberman/lib-assets.js";
import * as Helpers from "../lib/bomberman/lib-helpers.js";
import * as Handlers from "../lib/bomberman/lib-handlers.js";

var shuffledSkins = {};
var shuffledPlayerSkins = Helpers.shuffleArray(Assets.playerSkins);

shuffledPlayerSkins.forEach((skin, index) => {
    shuffledSkins[index.toString()] = skin;
});

var connection = new signalR.HubConnectionBuilder().withUrl("/Play").build();
connection.on("OnReady", function (readyCount, maxPlayers) {
    Handlers.OnReady(readyCount, maxPlayers)
});

connection.on("OnNewJoin", function (users, readyCount, maxPlayers) {
    Handlers.OnNewJoin(users, readyCount, maxPlayers);
})

var gameState = new Helpers.GameStateData();
function decodeString(mapString) {
    gameState.decodeMapString(mapString);

    if (gameState.SpectatorList.length > 0) {
        document.getElementById("spectatorListContainer").classList.remove("hide");
    } else {
        document.getElementById("spectatorListContainer").classList.add("hide");
    }

    Helpers.updateSpectatorList(gameState.SpectatorList, "spectatorList");
}

function animateChanges() {
    if (gameState.indexOfUser === -1 || gameState.prevDataForUser.length === 0) {
        return;
    }

    if (gameState.prevDataForUser[0] < gameState.PlayerCurrBomCounts[gameState.indexOfUser]) {
        let element = document.getElementById("bombRecharge");
        element.classList.remove("animateChange");
        element.offsetWidth;
        element.classList.add("animateChange");
    }

    if (gameState.prevDataForUser[1] !== gameState.PlayerMaxBomCounts[gameState.indexOfUser]) {
        let element = document.getElementById("extraMaxBombCount");
        if (gameState.prevDataForUser[1] > gameState.PlayerMaxBomCounts[gameState.indexOfUser]) //lost maxBombCount
            element = document.getElementById("minusMaxBombCount");
        element.classList.remove("animateChange");
        element.offsetWidth;
        element.classList.add("animateChange");
    }

    if (gameState.prevDataForUser[2] !== gameState.PlayerIsShielded[gameState.indexOfUser]) {
        let element = document.getElementById("getShielded");
        if (gameState.prevDataForUser[2])
            element = document.getElementById("looseShielded");
        element.classList.remove("animateChange");
        element.offsetWidth;
        element.classList.add("animateChange");
    }

    if (gameState.prevDataForUser[3] + 500 <= gameState.PlayerScores[gameState.indexOfUser]) { //500 so that the power is the only one that can give it
        let element = document.getElementById("getExtraPoints");
        element.classList.remove("animateChange");
        element.offsetWidth;
        element.classList.add("animateChange");
    }
}

window.addEventListener('resize', function () {
    Helpers.setAnimationSize();
});

var animationDivHasBeenResized = false;
var firstTickHappened = false;

connection.on("OnGameUpdate", function (mapString, isInProgres) {
    let playersDiv = document.getElementById("playersAndScores");
    let playerCount = document.getElementById("playerCount");
    let gameTable = document.getElementById("map");

    //Variables are definied above the implementation of this function:
    decodeString(mapString);

    //Ready stage only
    if (!isInProgres) {
        if (!PlayerNames.includes(currentUser)) { //currentUser is definined at the beginning of Play.cshtml
            document.getElementById("readyButton").hidden = true;
        }
        return;
    }

    animateChanges();

    //Representing the data:
    gameTable.hidden = false;
    document.getElementById("readyCount").hidden = true;
    document.getElementById("readyButton").hidden = true;
    document.getElementById("playersContainer").hidden = true;

    playersDiv.innerHTML = "";
    playerCount.innerHTML = "";
    gameTable.innerHTML = "";

    document.getElementById("hideReadyBoxes").classList.add("hide");
    document.getElementById("scoreboard").classList.remove("hide");

    Helpers.displayScores(gameState);
    Helpers.displayBoard(noiseMap, gameState, gameTable);

    //whenever the enemy dies you get its skin - known problem
    for (let i = 0; i < gameState.PlayerPositions.length; i++) {
        Helpers.displayPlayer(i, gameTable, shuffledSkins, gameState);

        if (gameState.PlayerIsShielded[i] == true) {
            let shieldDiv = document.createElement("div");
            shieldDiv.classList.add("player-shield");
            let skinurl = "";
            let shieldObject = Assets.shield[0];
            skinurl = shieldObject.shield;
            shieldDiv.style.backgroundImage = "url('" + skinurl + "')";

            gameTable.children[gameState.PlayerPositions[i][0]].children[gameState.PlayerPositions[i][1]].appendChild(shieldDiv);
        }
    }

    //monsters and stuff
    for (let i = 0; i < gameState.FloatingItems.length; i++) {

        let item = gameState.FloatingItems[i];
        let itemType = item[2];

        let itemDiv = document.createElement("div");
        itemDiv.classList.add("floating-item");

        let monsterSkinObject = Assets.monsterSkins[0];
        let bombSkinObject = Assets.bombSkin[0];
        let fireSkinObject = Assets.fireSkin[0];
        let goodPowerUpObject = Assets.goodPowerUp[0];
        let badPowerUpObject = Assets.badPowerUp[0];

        let skinUrl = "";
        switch (true) {
            case /Monster/.test(itemType):
                switch (parseInt(itemType.match(/\d+$/)[0])) {
                    case 0: //Up
                        skinUrl = monsterSkinObject.up;
                        break;
                    case 1: //Right
                        skinUrl = monsterSkinObject.right;
                        break;
                    case 2: //Down
                        skinUrl = monsterSkinObject.down;
                        break;
                    case 3: //Left
                        skinUrl = monsterSkinObject.left;
                        break;
                    default:
                        break;
                }
                break;
            case /Fire/.test(itemType):
                switch (itemType.match(/[A-Z]\d$/)[0]) {
                    //middle
                    case "M0":
                        skinUrl = fireSkinObject.firem0;
                        break;
                    case "M1":
                        skinUrl = fireSkinObject.firem1;
                        break;
                    case "M2":
                        skinUrl = fireSkinObject.firem2;
                        break;
                    case "M3":
                        skinUrl = fireSkinObject.firem3;
                        break;
                    case "M4":
                        skinUrl = fireSkinObject.firem4;
                        break;
                    //horizontal
                    case "H1":
                        skinUrl = fireSkinObject.fireh1;
                        break;
                    case "H2":
                        skinUrl = fireSkinObject.fireh2;
                        break;
                    case "H3":
                        skinUrl = fireSkinObject.fireh3;
                        break;
                    case "H4":
                        skinUrl = fireSkinObject.fireh4;
                        break;
                    //left ??
                    case "R1":
                        skinUrl = fireSkinObject.firel1;
                        break;
                    case "R2":
                        skinUrl = fireSkinObject.firel2;
                        break;
                    case "R3":
                        skinUrl = fireSkinObject.firel3;
                        break;
                    case "R4":
                        skinUrl = fireSkinObject.firel4;
                        break;
                    //right ??
                    case "L1":
                        skinUrl = fireSkinObject.firel1;
                        itemDiv.style.transform = "rotate(180deg)";
                        break;
                    case "L2":
                        skinUrl = fireSkinObject.firel2;
                        itemDiv.style.transform = "rotate(180deg)";
                        break;
                    case "L3":
                        skinUrl = fireSkinObject.firel3;
                        itemDiv.style.transform = "rotate(180deg)";
                        break;
                    case "L4":
                        skinUrl = fireSkinObject.firel4;
                        itemDiv.style.transform = "rotate(180deg)";
                        break;
                    //vertical
                    case "V1":
                        skinUrl = fireSkinObject.fireh1;
                        itemDiv.style.transform = "rotate(90deg)";
                        break;
                    case "V2":
                        skinUrl = fireSkinObject.fireh2;
                        itemDiv.style.transform = "rotate(90deg)";
                        break;
                    case "V3":
                        skinUrl = fireSkinObject.fireh3;
                        itemDiv.style.transform = "rotate(90deg)";
                        break;
                    case "V4":
                        skinUrl = fireSkinObject.fireh4;
                        itemDiv.style.transform = "rotate(90deg)";
                        break;
                    //top
                    case "T1":
                        skinUrl = fireSkinObject.firel1;
                        itemDiv.style.transform = "rotate(90deg)";
                        break;
                    case "T2":
                        skinUrl = fireSkinObject.firel2;
                        itemDiv.style.transform = "rotate(90deg)";
                        break;
                    case "T3":
                        skinUrl = fireSkinObject.firel3;
                        itemDiv.style.transform = "rotate(90deg)";
                        break;
                    case "T4":
                        skinUrl = fireSkinObject.firel4;
                        itemDiv.style.transform = "rotate(90deg)";
                        break;
                    //bottom
                    case "B1":
                        skinUrl = fireSkinObject.firel1;
                        itemDiv.style.transform = "rotate(-90deg)";
                        break;
                    case "B2":
                        skinUrl = fireSkinObject.firel2;
                        itemDiv.style.transform = "rotate(-90deg)";
                        break;
                    case "B3":
                        skinUrl = fireSkinObject.firel3;
                        itemDiv.style.transform = "rotate(-90deg)";
                        break;
                    case "B4":
                        skinUrl = fireSkinObject.firel4;
                        itemDiv.style.transform = "rotate(-90deg)";
                        break;
                    default:
                        break;
                }
                break;
            case /GOOD/.test(itemType):
                skinUrl = goodPowerUpObject.goodPowerUp;
                break;
            case /BAD/.test(itemType):
                skinUrl = badPowerUpObject.badPowerUp;
                break;
            case /Bomb/.test(itemType):
                switch (parseInt(itemType.match(/\d$/)[0])) {
                    case 0: //Stage 0
                        skinUrl = bombSkinObject.stage0;
                        break;
                    case 1: //Stage 1
                        skinUrl = bombSkinObject.stage1;
                        break;
                    case 2: //Stage 2
                        skinUrl = bombSkinObject.stage2;
                        break;
                    default:
                        break;
                }
            default:
                break;

        }
        itemDiv.style.backgroundImage = "url('" + skinUrl + "')";

        gameTable.children[gameState.FloatingItems[i][0]].children[gameState.FloatingItems[i][1]].appendChild(itemDiv);

    }

    //Zone
    Helpers.displayZone(gameState, gameTable);

    //Sets size for the animationSize
    if (!animationDivHasBeenResized) {
        Helpers.setAnimationSize();
        animationDivHasBeenResized = true;
    }

    if (gameState.SpectatorList > 0) {
        document.getElementById("spectatorListContainer").classList.remove("hide");
    } else {
        document.getElementById("spectatorListContainer").classList.add("hide");
    }

    Helpers.updateSpectatorList(gameState.SpectatorList, "spectatorList");

    //First frame only updates
    if (!firstTickHappened) {
        //Automaticly scrolls down to the bottom of the page on the start of the game
        window.scrollTo(0, document.body.scrollHeight);
        firstTickHappened = true;
    }
});

const newBtn = document.getElementById("newBtn");
const spectateBtn = document.getElementById("spectateBtn");
const modalDeath = document.getElementById("deathPopup");

connection.on("OnDeath", function (user, score) {
    modalDeath.style.display = 'block';

    document.getElementById('finalScore').textContent = score;
    newBtn.addEventListener('click', backToCreateGame);
    spectateBtn.addEventListener('click', Spectate);
});

const exitBtn = document.getElementById("exitBtn");
const modalEndGame = document.getElementById("gameEndPopup");
const modalText = document.getElementById("gameEndPopupText");
connection.on("OnGameEnd", function () {
    modalEndGame.style.display = 'block';
    modalText.innerHTML = '';
    if (gameState.PlayerNames.includes(currentUser)) {
        modalText.innerHTML = "You won! :)";
    }

    exitBtn.addEventListener('click', backToCreateGame);
});

function backToCreateGame() {
    modalEndGame.style.display = 'none';
    window.location.assign('/Game/Games');
    cleanup();
}

var noiseMap = Helpers.generateNoiseMap(40, 40);

connection.start().then(function () {
    document.getElementById("map").hidden = true;

}).catch(function (err) {
    return console.error(err.toString());
});

//Keydown movement without rapid firing
var keyDownAt = {}
keyDownAt["w"] = false;
keyDownAt["a"] = false;
keyDownAt["s"] = false;
keyDownAt["d"] = false;
keyDownAt["W"] = false;
keyDownAt["A"] = false;
keyDownAt["S"] = false;
keyDownAt["D"] = false;
document.addEventListener("keydown", function (e) {
    var input = e.key;
    if (input in keyDownAt) {
        if (keyDownAt[input] === undefined || keyDownAt[input]) {
            return;
        } else {
            keyDownAt[input] = true;
        }
    }
    connection.invoke("SendInput", input).catch(function (err) {
        return console.error(err.toString());
    });

    if (e.key === ' ') {
        e.preventDefault();
    }
});

document.addEventListener("keyup", function (e) {
    if (e.key in keyDownAt) {
        keyDownAt[e.key] = false;
    }

    if (e.key === ' ') {
        e.preventDefault();
    }
});

var readyButton = document.getElementById("readyButton");
var isReady = false;

document.getElementById("readyButton").addEventListener("click", function () {
    isReady = !isReady;
    readyButton.value = isReady ? "Unready" : "Ready";

    connection.invoke("ReceiveReady", isReady).catch(function (err) {
        return console.error(err.toString());
    });
});

var confirm = false;
var linkToNavigate = null;

const modal = document.getElementById('confirmationModal');
const confirmLeaveBtn = document.getElementById('confirmLeave');
const cancelLeaveBtn = document.getElementById('cancelLeave');
const navbarLinks = document.querySelectorAll('.navbar-link');
document.getElementById("LeaveeModalXButton").addEventListener('click', handleCancelLeave);

navbarLinks.forEach(link => {
    link.addEventListener('click', handleNavbarLinkClick);
});

function handleNavbarLinkClick(event) {
    event.preventDefault();
    modal.style.display = 'block';
    linkToNavigate = this.href;

    confirmLeaveBtn.addEventListener('click', handleConfirmLeave);
    cancelLeaveBtn.addEventListener('click', handleCancelLeave);
}

function handleConfirmLeave() {
    modal.style.display = 'none';
    confirm = true;
    if (confirm && linkToNavigate) {
        window.location.href = linkToNavigate;
    }
    cleanup();
}

function handleCancelLeave() {
    modal.style.display = 'none';
    confirm = false;
    cleanup();
}

function Spectate() {
    modalDeath.style.display = 'none';
    confirm = false;
    cleanup();
}

function cleanup() {
    confirmLeaveBtn.removeEventListener('click', handleConfirmLeave);
    cancelLeaveBtn.removeEventListener('click', handleCancelLeave);
    linkToNavigate = null;
}