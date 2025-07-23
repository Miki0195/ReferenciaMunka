document.addEventListener('keydown', function (event) {
    if (event.key === 'w' || event.key === 'W') {
        let submitButton = document.getElementById('submitButton');
        if (submitButton.disabled) {
            submitButton.disabled = false;
            document.getElementById('w-div').classList.add("black-bg");
        }
    }
});