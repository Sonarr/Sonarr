/* Click on row, show details */
$('.seriesTable a, .dataTable a').live('click', function (event) {
    if ($(this).attr('onclick'))
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

//Create Image
function createImageAjaxLink(url, image, alt, title, classes) {
    var html = "<a onclick=\"Sys.Mvc.AsyncHyperlink.handleClick(this, new Sys.UI.DomEvent(event), { insertionMode: Sys.Mvc.InsertionMode.replace });\" href=\"" + url + "\">" +
                    "<img class=\"" + classes + "\" src=\"" + image + "\" title=\"" + title + "\" alt=\"" + alt + "\"></a>";

    return html;
}

//Reload/Redraw the grid from the server (bServerSide == true)
function redrawGrid() {
    oTable.fnDraw();
}

//Force reload using Ajax Binding (bServerSide == false)
function reloadGrid() {
    oTable.fnReloadAjax();
}