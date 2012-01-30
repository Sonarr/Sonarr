function grid_onError(e) {
    //Suppress the alert
    e.preventDefault();
}

//Highlight rows based on a number of details
function highlightRow(e) {
    var row = e.row;
    var dataItem = e.dataItem;

    var ignored = dataItem.Ignored;
    var status = dataItem.Status;

    if (ignored) {
        $(row).addClass('episodeIgnored');
        return;
    }

    if (status == "Missing") {
        $(row).addClass('episodeMissing');
        return;
    }
}