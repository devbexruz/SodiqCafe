// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Avtomatik ravishda barcha jadvallarni mobile uchun qulay "card" (karta) ko'rinishiga o'tkazish
document.addEventListener("DOMContentLoaded", function () {
    const tables = document.querySelectorAll("table.table");
    tables.forEach(table => {
        // Asosiy CSS klassni qo'shamiz
        table.classList.add("table-mobile-cards");
        
        // Agar jadval table-responsive ichida bo'lsa, mobil qurilmalarda scroll bo'lmasligi uchun
        // uni shaffof qilib qo'yishimiz mumkin yoki o'z holida qoldiramiz (CSS block ga o'tkazadi o'zi).

        const theadRows = table.querySelectorAll("thead tr");
        if(theadRows.length === 0) return;
        
        // Eng oxirgi thead qatoridan sarlavhalarni olamiz
        const headers = Array.from(theadRows[theadRows.length - 1].querySelectorAll("th")).map(th => th.innerText.trim());
        
        const rows = table.querySelectorAll("tbody tr");
        rows.forEach(row => {
            const cells = row.querySelectorAll("td");
            cells.forEach((cell, index) => {
                if (headers[index] && headers[index] !== "") {
                    cell.setAttribute("data-label", headers[index]);
                }
            });
        });
    });
});
