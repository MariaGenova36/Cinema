document.addEventListener('DOMContentLoaded', function () {
    const seatButtons = document.querySelectorAll('.seat-btn');
    const seatRowInput = document.getElementById('SeatRow');
    const seatColInput = document.getElementById('SeatColumn');

    if (!seatButtons || !seatRowInput || !seatColInput) return;

    seatButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const isSelected = btn.classList.contains('seat-selected');

            // Де-селекция при повторно кликване
            if (isSelected) {
                btn.classList.remove('seat-selected');
                seatRowInput.value = '';
                seatColInput.value = '';
            } else {
                // Премахваме селекцията от всички останали
                seatButtons.forEach(b => b.classList.remove('seat-selected'));

                // Селектираме текущия
                btn.classList.add('seat-selected');
                seatRowInput.value = btn.dataset.row;
                seatColInput.value = btn.dataset.col;
            }
        });
    });

    // Валидация при submit
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function (e) {
            const seatRow = seatRowInput.value;
            const seatCol = seatColInput.value;

            if (!seatRow || !seatCol) {
                e.preventDefault();
                alert("Please choose a seat before making a reservation.");
            }
        });
    }
});
