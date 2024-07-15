const buildHistoryPath = 'build.history.json';

window.onload = (event) => {
    const table = document.getElementById("builds");
    console.log("table", table);
    fetchJSON(buildHistoryPath).then(history => {
        console.log("history", history);
        buildTable(table, history);
    });
};

async function fetchJSON(path) {
    const response = await fetch(path);
    if (!response.ok) {
        return {"List": []};
    }
    const movies = await response.json();
    return movies;
}

function buildTable(table, history) {

    const array = Array.isArray(history.List) ? Array.from(history.List) : [];
    if (array.length === 0) {
        insertMessage(table, "No builds available for testing");
        return
    }
    array.forEach(item => {
        console.log("item", item);
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
    label.innerHTML = createLink(item.HRef, item.Label);
    notes.innerText = item.Notes;
}

function createLink(href, label) {
    return `<a href="${href}" rel="external nofollow noopener" target="_blank">${label} &nbsp; <i class="fa fa-external-link" style="font-size:16px"></i></a>`;
}
