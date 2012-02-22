var seriesEditorUrl = '../Series/SeriesEditor';
var saveSeriesEditorUrl = '../Series/SaveSeriesEditor';
var seriesDeleteUrl = '../Series/DeleteSeries';

$("#seriesEditor").dialog({
    autoOpen: false,
    height: 'auto',
    width: 670,
    resizable: false,
    modal: true,
    buttons: {
        "Delete": {
            text: "Delete",
            class: "ui-delete-button",
            click: function () {
                var answer = confirm("Are you sure you want to delete this series?");
                if (answer) {
                    var seriesId = $('#SeriesId').val();

                    $.ajax({
                        type: "POST",
                        url: seriesDeleteUrl,
                        data: { seriesId: seriesId },
                        success: function (data) {
                            //Remove the row from the grid... along with the details row
                            afterDelete();
                        }
                    });
                    $(this).dialog("close");
                }
            }
        },
        "Save": function () {
            //Save the form
            $('#SeriesEditorForm').submit();
            afterSave();

            $(this).dialog("close");
        },
        Cancel: function () {
            $(this).dialog("close");
        }
    }
});

$("#seriesDelete").dialog({
    autoOpen: false,
    resizable: false,
    height: 'auto',
    width: 450,
    modal: true,
    buttons: {
        "Delete": function () {
            var seriesId = $('.seriesId').val();
            $.ajax({
                type: "POST",
                url: seriesDeleteUrl,
                data: { seriesId: seriesId },
                success: function (data) {
                    //Remove the row from the grid... along with the details row
                    afterDelete(seriesId);
                }
            });
            $(this).dialog("close");
        },
        Cancel: function () {
            $(this).dialog("close");
        }
    }
});

$(".editButton").live('click', function () {
    //Get the SeriesId and Title
    var seriesId = parseInt($(this).attr("value"));
    var title = $(this).attr("rel");

    //Set the title of the dialog
    $("#seriesEditor").dialog("option", "title", title);

    //Pre-populate the view with ajax
    $('#seriesEditor').html('<div style="text-align: center; width: 100%; height: 100%;"><img src="../../Content/Images/ajax-loader.gif" style="padding-top: 120px;" /></div>');

    //Get the view
    $.ajax({
        url: seriesEditorUrl,
        data: { seriesId: seriesId },
        success: function (data) {
            $('#seriesEditor').html(data);
        }
    });

    //Open the dialog
    $("#seriesEditor").dialog("open");
});

$(".deleteButton").live('click', function () {
    //Get the SeriesId and Title
    var seriesId = parseInt($(this).attr("value"));
    var title = $(this).attr("rel");

    //Fill in the view
    $('#seriesDelete').children('.seriesId').val(seriesId);
    $('#seriesDelete').children('.seriesTitle').html(title);

    //Open the dialog
    $("#seriesDelete").dialog("open");
});