var lastLobbyNum = -1;
var isSpectating = false;

function showSpectateForm(lobbyNum) {
    lastLobbyNum = lobbyNum;
    document.getElementById('spectateFormBackground').style.display = 'block';
}

function closeSpectateForm() {
    document.getElementById('spectateFormBackground').style.display = 'none';
}

function goToLobby() {
    if (lastLobbyNum == -1) {
        return;
    }
    window.location.href = "/Game/Go?lobbyNum=" + lastLobbyNum + "&isSpectating=" + isSpectating;
}

window.onload = function () {
    document.getElementById('spectateSwitch').addEventListener('change', function () {
        let validationDiv = document.getElementById('validationDiv');
        let wDiv = document.getElementById('w-div');
        let submitButton = document.getElementById('submitButton');

        if (this.checked) {
            isSpectating = true;
            submitButton.disabled = false;
            wDiv.classList.remove("black-bg");

            validationDiv.classList.add('squished');
        } else {
            isSpectating = false;
            submitButton.disabled = true;

            validationDiv.classList.remove('squished');
        }
    });

    document.addEventListener('keydown', function (event) {
        if (document.getElementById('spectateSwitch').checked) {
            return;
        }

        if (event.key === 'w' || event.key === 'W') {
            let submitButton = document.getElementById('submitButton');
            if (submitButton.disabled === true) {
                submitButton.disabled = false;
                document.getElementById('w-div').classList.add("black-bg");
            }
        }
    });
};

function closeTextPopUp() {
    document.getElementById("textPopUpBackgroundId").style.display = "none";
}