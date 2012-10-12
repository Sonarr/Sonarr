/* Click on row, show details */
$(document).on('click', '.seriesTable a, .dataTable a', function (event) {
    if ($(this).attr('data-ajax') === "true" || $(this).attr('onclick'))
        return;

    event.preventDefault();
    var link = $(this).attr('href');

    if ($(this).attr('target') === '_blank')
        window.open(link);

    else
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
    signalRProvider.updatedStatus = function (data) {
        var row = $('[data-episode-id="' + data.EpisodeId + '"]');

        if (row.length == 0)
            return;

        var statusElement = $(row).find('i.statusImage');

        if (statusElement.length == 0)
            return;

        statusElement.attr('data-status', data.EpisodeStatus);

        if (data.EpisodeStatus != "Missing") {
            statusElement.parent('td').removeClass('episodeMissing');
        }

        if (data.Quality != null) {
            var qualityColumn = $(row).find('.episodeQuality');
            
            if (qualityColumn)
                qualityColumn.text(data.Quality);
        }
    };

    // Start the connection
    $.connection.hub.start({ transport: 'longPolling' });
});