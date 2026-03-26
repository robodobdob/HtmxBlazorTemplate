// rating stars web component
customElements.define('rating-stars', class extends HTMLElement {
        constructor() {
            super();
            this.rating = this.getAttribute("rating");
        }

        connectedCallback() {
            let stars = [1, 2, 3, 4, 5].map(i =>
                `<span class=` + (i <= this.rating ? `text-warning` : `text-secondary`) + `>★</span>`
            );
            this.innerHTML = stars.join('');
        }
    }
);

// utility modal close event listener
var dialog = document.getElementById('utilityModal');
if (dialog) {
    dialog.addEventListener('close', (e) => {
        document.getElementById('utilityModal_content').innerHTML = '';
    });

    document.body.addEventListener("close-modal", function(evt){
        dialog.close();
    })
}