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
    $(this).parent('tr').next('.detail-row').toggle();
});

function grid_onError(e) {
    //Suppress the alert
    e.preventDefault();
}

//Perform the details opening
var oTable;

$('.dataTable td:not(:last-child)').live('click', function () {
    var nTr = this.parentNode;

    if ($(nTr).hasClass('details-opened')) {
        oTable.fnClose(nTr);
        $(nTr).removeClass('details-opened');
    }

    else {
        oTable.fnOpen(nTr, fnFormatDetails(oTable, nTr), 'Details');
        $(nTr).addClass('details-opened');
    }
});

//Datatables format display details
function fnFormatDetails(oTable, nTr) {
    var aData = oTable.fnGetData(nTr);
    return aData[aData.length - 1];
}