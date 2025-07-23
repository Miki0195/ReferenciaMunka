$(document).ready(function () {
    const seats = document.querySelectorAll('[data-status]');
    const maxNumberOfSeats = Number($('#maximumNumberOfSeats').data('maximumnumberofseats'));
    seats.forEach(seat => {
        $(seat).on('click', function () {

            // Count data-status with 'selected' value
            let selectedCount = Array.from(seats).filter(el => el.getAttribute('data-status') === 'Selected').length;

            const currentStatus = seat.getAttribute('data-status');

            if (currentStatus === 'Free') {
                selectedCount = selectedCount + 1;
            } else {
                selectedCount = selectedCount - 1;
            }

            if (selectedCount > maxNumberOfSeats) {
                console.warn("Too much selected seat!");
                return;
            }

            $('#selected-count').text(selectedCount);

            // Change the status to 'selected' or 'free'.
            // Don't change if reserved.
            let newState = ""
            if (currentStatus === 'Free') {
                newState = "Selected"
                seat.className = "table-success text-center";
            } else if (currentStatus === 'Selected') {
                seat.setAttribute('data-status', 'Free');
                newState = "Free"
                seat.className = "table-light text-center";
            }
            seat.setAttribute('data-status', newState);
            seat.title = "Row: " + seat.getAttribute('data-seatrow') + " Column: " + parseInt(seat.getAttribute('data-seatcolumn')) + " Status: " + newState;
        });
    });

    $('#reserveButton').on('click', function () {


        let form = $("#reservationForm");
        // Run the ViewModel validations
        if (!form.valid()) {
            // Show error
            alert("One or more fields contain incorrect data");
            return;
        }

        const selectedSeats = [];

        document.querySelectorAll('[data-status="Selected"]').forEach(seat => {
            selectedSeats.push({
                Row: parseInt(seat.getAttribute('data-seatrow')),
                Column: parseInt(seat.getAttribute('data-seatcolumn')),
                Status: "Selected"
            });
        });

        if (selectedSeats.length < 1) {
            alert("Choose at least one seat");
            return;
        }

        const name = $('#name').val();
        const email = $('#email').val();
        const phonenumber = $('#phone').val();
        const comment = $('#comment').val();


        // Find and read the AntiForgeryToken value
        const screeningId = $('#screeningId').data('screeningid');
        const tokenElement = $('input[name="__RequestVerificationToken"]');
        const tokenValue = tokenElement ? tokenElement.value : null;

        const statusEnum = {
            "Free": 0,
            "Selected": 1,
            "Reserved": 2,
            "Sold": 3
        };

        fetch('/Reservation/Reserve', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenValue
            },
            body: JSON.stringify({
                name: name,
                phone: phonenumber,
                email: email,
                comment: comment,
                screeningId: Number(screeningId),
                seats: selectedSeats.map(seat => ({
                    ...seat,
                    Status: statusEnum[seat.Status] || seat.Status // Map enum values to enum (order) number
                }))
            })
        })
            .then(response => {
                if (response.ok) { // True if responnse between 200-299
                    alert("Sikeres foglalás!")
                    window.location.href = "../Home/Index"
                } else {
                    throw new Error('Error occured: ' + response.status); // Throw error when return code not success
                }
            })
            .catch(error => {
                console.error('Error occured: ', error); //Write error to console
            });
    });
});
