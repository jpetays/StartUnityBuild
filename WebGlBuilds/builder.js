const buildHistoryPath = 'build.history.json';

// noinspection JSUnusedGlobalSymbols
const Item = {
    Ver: "2",
    Track: "Test",
    Date: "2024-07-19 09:30",
    Label: "1.0.7",
    HRef: "/Test-1_0_7_201_247418",
    Notes: "Work In Progress for Demo 1.1.x"
};

window.onload = (_) => {
    const table = document.getElementById("builds");
    console.log("table", table);
    fetchJSON(buildHistoryPath).then(history => {
        buildTable(table, history);
    });
};

async function fetchJSON(path) {
    const response = await fetch(path);
    if (!response.ok) {
        return {"List": []};
    }
    return await response.json();
}

function buildTable(table, history) {

    const array = Array.isArray(history.List) ? Array.from(history.List) : [];
    if (array.length === 0) {
        insertMessage(table, "No builds available for testing");
        return
    }
    console.log("history", array);
    array.forEach(item => {
        insertRow(table, item);
    });
}

function insertMessage(table, message) {
    const row = table.insertRow();
    const cell = row.insertCell();
    cell.colSpan = 3;
    cell.innerText = message;
}

function insertRow(table, item) {
    const row = table.insertRow();
    const date = row.insertCell();
    const label = row.insertCell();
    const notes = row.insertCell();
    date.innerText = item.Date;
    const labelText = item.Ver > 1 ? `${item.Track} ${item.Label}` : item.Label;
    label.innerHTML = createLink(item.HRef, labelText);
    notes.innerText = item.Notes;
}

function createLink(href, label) {
    const part1 = `<a href="${href}" rel="external nofollow noopener" target="_blank">${label}</a>`;
    const part2 = `<a href="${href}" rel="external nofollow noopener" target="_blank"  class="spacer">&nbsp;&nbsp;</a>`;
    const part3 = `<a href="${href}" rel="external nofollow noopener" target="_blank"><i class="fa fa-external-link" style="font-size:16px"></i></a>`;
    return `${part1}${part2}${part3}`;
}
