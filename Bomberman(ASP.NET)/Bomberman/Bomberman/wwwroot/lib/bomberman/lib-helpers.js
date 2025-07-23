export function shuffleArray(array) {
    for (let i = array.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [array[i], array[j]] = [array[j], array[i]];
    }
    return array;
}

export function updateSpectatorList(spectatorList, spectatorListId) {
    let listContainer = document.getElementById(spectatorListId);
    listContainer.innerHTML = '';

    spectatorList.forEach(function (spectator) {
        let li = document.createElement('li');
        li.textContent = spectator;
        li.className = 'list-group-item';
        li.appendChild(listItem);
    });
}

export function updateProgressBar(bombCount, progressBar) {
    const progress = bombCount === 0 ? 0 : 100;
    progressBar.style.width = progress + "%";
}

export function generateNoiseMap(rows, cols) {
    let noiseMap = [];

    for (let i = 0; i < rows; i++) {
        let rowData = [];
        for (let j = 0; j < cols; j++) {
            let randomValue = Math.random();
            rowData.push(randomValue);
        }
        noiseMap.push(rowData);
    }

    return noiseMap;
}

export function setAnimationSize() {
    let forAnimations = document.getElementById("forAnimations");
    let map = document.getElementById("map");

    let computedStyle = window.getComputedStyle(map);

    forAnimations.style.width = computedStyle.width;
    forAnimations.style.height = computedStyle.height;
}

export function displayScores(data) {
    for (let i = 0; i < data.PlayerNames.length; i++) {
        let playerDiv = document.createElement("div");
        playerDiv.classList.add("player-details");

        let playerName = document.createElement("div");
        playerName.classList.add("player-name");
        playerName.textContent = data.PlayerNames[i];

        let playerInfo = document.createElement("div");
        playerInfo.classList.add("player-info");
        playerInfo.innerHTML = "<p><strong>Score:</strong> " + data.PlayerScores[i] + " pts</p>" +
            "<p><strong>Current/Max Bomb Count:</strong> " + data.PlayerCurrBomCounts[i] + "/"
            + data.PlayerMaxBomCounts[i] + "</p>";

        //For the bomb reload
        let progressBarContainer = document.createElement("div");
        progressBarContainer.classList.add("progress-bar-container");
        let progressBar = document.createElement("div");
        progressBar.classList.add("progress-bar");
        progressBarContainer.appendChild(progressBar);
        updateProgressBar(data.PlayerCurrBomCounts[i], progressBar);

        playerDiv.appendChild(playerName);
        playerDiv.appendChild(playerInfo);
        playerDiv.appendChild(progressBarContainer);

        document.getElementById("playersAndScores").appendChild(playerDiv); // Append playerDiv to #playersAndScores
    }
}

export function displayBoard(noiseMap, data, gameTable) {
    for (let i = 0; i < data.Board.length; i++) {
        let currRow = document.createElement("div");
        currRow.classList.add("row")

        for (let j = 0; j < data.Board[i].length; j++) {

            let currCell = document.createElement("div");
            currCell.classList.add("cell");

            switch (data.Board[i][j]) {
                case '0':
                    currCell.classList.add("empty" + (Math.floor(noiseMap[i][j] * 2) + 1)); //field1 or field2
                    break;
                case '1':
                    currCell.classList.add("wall" + (Math.floor(noiseMap[i][j] * 4) + 1)); //wall1 - wall4 randomized
                    break;
                case '2':
                    currCell.classList.add("box");
                    break;
                default:
            }
            currRow.appendChild(currCell);
        }
        gameTable.appendChild(currRow);
    }
}

export function displayPlayer(i, gameTable, shuffledSkins, data) {
    let playerDiv = document.createElement("div");
    playerDiv.classList.add("player");

    let skinObject = shuffledSkins[i % 4];
    let skinURL = "";
    switch (data.PlayerDirections[i]) {
        case 0: // Up
            skinURL = skinObject.up;
            break;
        case 1: // Right
            skinURL = skinObject.right;
            break;
        case 2: // Down
            skinURL = skinObject.down;
            break;
        case 3: // Left
            skinURL = skinObject.left;
            break;
        default:
            break;
    }
    playerDiv.style.backgroundImage = "url('" + skinURL + "')";

    gameTable.children[data.PlayerPositions[i][0]].children[data.PlayerPositions[i][1]].appendChild(playerDiv);
}

export function displayZone(data, gameTable) {
    for (let i = 0; i < data.Board.length; i++) {
        for (let j = 0; j < data.Board[i].length; j++) {
            if (i < data.ZoneLength || j < data.ZoneLength ||
                i > data.Board.length - data.ZoneLength - 1 ||
                j > data.Board[i].length - data.ZoneLength - 1) {
                gameTable.children[i].children[j].classList.add("fog");
            }
        }
    }
}

export class GameStateData {
    constructor() {
        this.init();
    }

    decodeMapString(mapString) {
        let prevPlayerCount = this.PlayerNames.length;
        // Get information for animations
        this.indexOfUser = this.PlayerNames.indexOf(currentUser);
        if (this.indexOfUser != -1) {
            this.prevDataForUser = [
                this.PlayerCurrBomCounts[this.indexOfUser],
                this.PlayerMaxBomCounts[this.indexOfUser],
                this.PlayerIsShielded[this.indexOfUser],
                this.PlayerScores[this.indexOfUser]
            ];
        }
        else {
            this.prevDataForUser = [];
        }

        this.init();

        let s = mapString.split(";");
        this.parsePlayers(s);
        this.parseBoard(s);
        this.parseSpectators(s, prevPlayerCount);
    }

    parsePlayers(s) {
        this.MaxPlayerCount = parseInt(s[0]);
        if (s[1] !== "") {
            let playerSplit = s[1].split(',');
            for (let i = 0; i < playerSplit.length; i += 8) {
                this.PlayerNames.push(playerSplit[i]);
                this.PlayerScores.push(parseInt(playerSplit[i + 1]));
                this.PlayerCurrBomCounts.push(parseInt(playerSplit[i + 2]));
                this.PlayerMaxBomCounts.push(parseInt(playerSplit[i + 3]));
                this.PlayerPositions.push([parseInt(playerSplit[i + 4]), parseInt(playerSplit[i + 5])]);
                this.PlayerDirections.push(parseInt(playerSplit[i + 6]))
                this.PlayerIsShielded.push(playerSplit[i + 7] === "1");
            }
        }
    }

    parseSpectators(s, prevPlayerCount) {
        if (s[s.length - 1] !== "") {
            let spectatorSplit = s[s.length - 1].split(',');
            for (let i = 0; i < spectatorSplit.length; i++) {
                this.SpectatorList.push(spectatorSplit[i]);
            }
        }
        //In case of player death
        if (this.PlayerNames.length < prevPlayerCount) {
            this.indexOfUser = -1;
            this.prevDataForUser = [];
        }
    }

    parseBoard(s) {
        // Board
        let rows = s[2].split(",");
        for (let i = 0; i < rows.length; i++) {
            this.Board.push(rows[i].split(""));
        }

        // Floating items
        if (s[3] != "") {
            let splitItems = s[3].split("|");
            for (let i = 0; i < splitItems.length; i++) {
                let position = splitItems[i].split(",");
                this.FloatingItems.push([parseInt(position[0]), parseInt(position[1]), position[2]]);
            }
        }

        // Zone
        this.ZoneLength = parseInt(s[4]);
    }

    init() {
        this.indexOfUser = 0;
        this.prevDataForUser = [];
        this.ZoneLength = 0;
        this.MaxPlayerCount = 0;
        this.PlayerNames = [];
        this.PlayerScores = [];
        this.PlayerCurrBomCounts = [];
        this.PlayerMaxBomCounts = [];
        this.PlayerPositions = [] //PlayerPositions[i] = [posy, posx]
        this.PlayerDirections = [] //0-up, 1-right, 2-down, 3-left
        this.PlayerIsShielded = [] //true or false
        this.Board = []; //Board[y][x], each cell has a string in it, indicating what tile it should be //"0"-Empty, "1"-Wall, "2"-Box
        this.FloatingItems = []; //FloatingItems[i] = [posy, posx, string] //check GameModel.EncodeGameStateToString() method for possible string values (Includes monsters)
        this.SpectatorList = [];
    }
}
