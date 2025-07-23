export function OnReady(readyCount, maxPlayers) {
    let playersContainer = document.getElementById("playersContainer");
    playersContainer.innerHTML = "";

    //Remove ready lights if nobody is ready
    if (readyCount == 0) {
        playersContainer.hidden = true;
        return;
    }

    for (let i = 0; i < maxPlayers; i++) {
        let playerBox = document.createElement("div");
        playerBox.classList.add("player-box");

        if (i < readyCount) {
            playerBox.classList.add("ready");
        }

        playersContainer.appendChild(playerBox);
    }

    if (maxPlayers == readyCount) {
        playersContainer.hidden = true;
        document.getElementById("readyButton").hidden = true;
    }
}

export function OnNewJoin(users, readyCount, maxPlayers) {
    let playersContainer = document.getElementById("playersContainer");
    playersContainer.innerHTML = "";

    //Remove ready lights if nobody is ready
    if (readyCount == 0) {
        playersContainer.hidden = true;
        return;
    }

    for (let i = 0; i < maxPlayers; i++) {
        let playerBox = document.createElement("div");
        playerBox.classList.add("player-box");

        if (i < readyCount) {
            playerBox.classList.add("ready");
        }

        playersContainer.appendChild(playerBox);
    }

    if (maxPlayers == readyCount) {
        playersContainer.hidden = true;
        document.getElementById("readyButton").hidden = true;
    }
}