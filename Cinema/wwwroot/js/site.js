document.addEventListener('DOMContentLoaded', function () {
    const seatButtons = document.querySelectorAll('.seat-btn.seat-available');
    const seatRowsInput = document.getElementById('SeatRows');
    const seatColsInput = document.getElementById('SeatCols');
    const ticketTypeSelect = document.getElementById('ticketTypeSelect');
    const ticketMultiplierInput = document.getElementById('TicketMultiplier');
    const priceSpan = document.getElementById('ticketPrice');

    let selectedSeats = [];

    seatButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const row = parseInt(btn.dataset.row);
            const col = parseInt(btn.dataset.col);
            const index = selectedSeats.findIndex(s => s.row === row && s.col === col);

            if (index >= 0) {
                selectedSeats.splice(index, 1);
                btn.classList.remove('seat-selected');
            } else {
                selectedSeats.push({ row, col });
                btn.classList.add('seat-selected');
            }

            seatRowsInput.value = JSON.stringify(selectedSeats.map(s => s.row));
            seatColsInput.value = JSON.stringify(selectedSeats.map(s => s.col));

            updatePrice();
        });
    });

    if (ticketTypeSelect) {
        ticketTypeSelect.addEventListener('change', updatePrice);
    }

    function updatePrice() {
        if (!priceSpan) return;

        const basePrice = parseFloat(priceSpan.dataset.baseprice);
        const multiplier = parseFloat(ticketTypeSelect.selectedOptions[0].dataset.multiplier);
        const seatsCount = selectedSeats.length || 1;
        const totalPrice = basePrice * multiplier * seatsCount;

        priceSpan.textContent = totalPrice.toFixed(2) + " lv.";

        // Винаги записваме с точка, за да не зависи от културата
        if (ticketMultiplierInput) {
            ticketMultiplierInput.value = multiplier.toString();
        }
    }

    window.validateSeatSelection = function () {
        if (selectedSeats.length === 0) {
            alert("Please select at least one seat.");
            return false;
        }
        return true;
    }

    updatePrice();
});

//Button back to top


window.onscroll = function () {
    const btn = document.getElementById("backToTopBtn");
    if (!btn) return;
    if (document.body.scrollTop > 200 || document.documentElement.scrollTop > 200) {
        btn.style.display = "block";
    } else {
        btn.style.display = "none";
    }
};

function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}
