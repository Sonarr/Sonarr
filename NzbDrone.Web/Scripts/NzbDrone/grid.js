/* Click on row, show details */
$(document).on('click', '.seriesTable a, .dataTable a', function (event) {
    if ($(this).attr('data-ajax') === "true")
        return;

    event.preventDefault();
    var link = $(this).attr('href');
    window.location = link;
    event.stopPropegation();
});

$('.seriesTable .data-row td:not(:last-child)').live('click', function () {
    if ($(this).closest('table').hasClass('no-details'))
        return;
    $(this).parent('tr').next('.detail-row').toggle();
});

function grid_onError(e) {
    //Suppress the alert
    e.preventDefault();
}

//Perform the details opening
var oTable;

$('.dataTable td:not(:last-child)').live('click', function () {
    if ($(this).closest('table').hasClass('no-details'))
        return;

    var nTr = this.parentNode;

    if ($(nTr).hasClass('details-opened')) {
        oTable.fnClose(nTr);
        $(nTr).removeClass('details-opened');
    }

    else {
        oTable.fnOpen(nTr, fnFormatDetails(nTr), 'Details');
        $(nTr).addClass('details-opened');
    }
});

//Datatables format display details
function fnFormatDetails(nTr) {
    var aData = oTable.fnGetData(nTr);
    return aData["Details"];
}

//Reload/Redraw the grid from the server (bServerSide == true)
function redrawGrid() {
    oTable.fnDraw();
}

//Force reload using Ajax Binding (bServerSide == false)
function reloadGrid() {
    oTable.fnReloadAjax();
}


//SignalR
$(function () {
    // Proxy created on the fly
    var signalRProvider = $.connection.signalRProvider;

    // Declare a function on the chat hub so the server can invoke it
    signalRProvider.updatedStatus = function (episodeId, episodeStatus) {
        var imageSrc = '../../Content/Images/' + episodeStatus + '.png';
        var row = $('tr.episodeId_' + episodeId);

        if (row.length == 0)
            return;

        var statusImage = $(row).find('img.statusImage');

        if (statusImage.length == 0)
            return;

        statusImage.attr('alt', episodeStatus);
        statusImage.attr('title', episodeStatus);
        statusImage.attr('src', imageSrc);

        if (episodeStatus != "Missing") {
            statusImage.parent('td').removeClass('episodeMissing');
        }
    };

    // Start the connection
    $.connection.hub.start();
});