//URLs
var addSeriesUrl = '../AddSeries/AddExistingSeries';
var addNewSeriesUrl = '../AddSeries/AddNewSeries';
var quickAddNewSeriesUrl = '../AddSeries/QuickAddNewSeries';
var existingSeriesUrl = '../AddSeries/ExistingSeries';
var addNewUrl = '../AddSeries/AddNew';

var deleteRootDirUrl = '../AddSeries/DeleteRootDir';
var saveRootDirUrl = '../AddSeries/SaveRootDir';
var rootListUrl = '../AddSeries/RootList';


//ExistingSeries
$(".masterQualitySelector").live('change', function () {

    var profileId = $(this).val();
    $("#existingSeries").find(".qualitySelector").each(function () {
        $(this).val(profileId);
    });
});

$(".addExistingButton").live('click', function() {
    var root = $(this).parents(".existingSeries");
    var title = $(this).siblings(".seriesLookup").val();
    var seriesId = $(this).siblings(".seriesId").val();
    var qualityId = $(this).siblings(".qualitySelector").val();
    var date = $(this).siblings('.aired-after').val();

    var path = root.find(".seriesPathValue Label").text();

    if (seriesId === 0 || $.trim(title).length === 0) {
        $.gritter.add({
            title: 'Failed',
            text: 'Invalid Series Information for \'' + path + '\'',
            image: '../../content/images/error.png',
            class_name: 'gritter-fail'
        });

        return false;
    }

    $.ajax({
        type: "POST",
        url: addSeriesUrl,
        data: jQuery.param({ path: path, seriesName: title, seriesId: seriesId, qualityProfileId: qualityId, airedAfter: date }),
        error: function(req, status, error) {
            alert("Sorry! We could not add " + path + " at this time. " + error);
        },
        success: function() {
            root.hide('highlight', 'fast');
            if ($('.existingSeries').filter(":visible").length === 1)
                reloadExistingSeries();
        }
    });

});

function reloadExistingSeries() {
    $('#existingSeries').html('<img src="../../Content/Images/ajax-loader.gif" />');
    $.ajax({
      url: existingSeriesUrl,
      success: function( data ) {
        $('#existingSeries').html(data);
      }
    });
}

$(".aired-after-master").live('change', function () {

    var date = $(this).val();
    $("#existingSeries").find(".aired-after").each(function () {
        $(this).val(date);
    });
});

//RootDir
//Delete RootDir
$('#rootDirs .actionButton img').live('click', function (image) {
    var path = $(image.target).attr('id');

    $.ajax({
        type: "POST",
        url: deleteRootDirUrl,
        data: { Path: path },
        success: function () {
            refreshRoot();
            $("#rootDirInput").val('');
        }
    });
});

$('#saveDir').live('click', saveRootDir);

function saveRootDir() {
    var path = $("#rootDirInput").val();

    if (path) {
        $.ajax({
            type: "POST",
            url: saveRootDirUrl,
            data: { Path: path },
            success: function () {
                refreshRoot();
                $("#rootDirInput").val('');
            }
        });
    }
}

function refreshRoot() {
    $.ajax({
        url: rootListUrl,
        success: function (data) {
            $('#rootDirs').html(data);
            reloadAddNew();
            reloadExistingSeries();
        }
    });
}


//AddNew
$('#saveNewSeries').live('click', function () {
    var seriesTitle = $("#newSeriesLookup").val();
    var seriesId = $("#newSeriesId").val();
    var qualityId = $("#qualityList").val();
    var path = $('#newSeriesPath').val();
    var date = $('#newAiredAfter').val();

    $.ajax({
        type: "POST",
        url: addNewSeriesUrl,
        data: jQuery.param({ path: path, seriesName: seriesTitle, seriesId: seriesId, qualityProfileId: qualityId, airedAfter: date }),
        error: function (req, status, error) {
            alert("Sorry! We could not add " + path + " at this time. " + error);
        },
        success: function () {
            $("#newSeriesLookup").val("");
        }
    });
});

function reloadAddNew() {
    $.ajax({
        url: addNewUrl,
        success: function (data) {
            $('#addNewSeries').html(data);
        }
    });
}


//QuickAddNew
$('#quickAddNew').live('click', function () {
    var seriesTitle = $("#newSeriesLookup").val();
    var seriesId = $("#newSeriesId").val();
    var qualityId = $("#qualityList").val();

    $.ajax({
        type: "POST",
        url: quickAddNewSeriesUrl,
        data: jQuery.param({ seriesName: seriesTitle, seriesId: seriesId, qualityProfileId: qualityId }),
        error: function (req, status, error) {
            alert("Sorry! We could not add " + path + " at this time. " + error);
        },
        success: function () {
            $("#newSeriesLookup").val("");
            $('#newSeriesPath').val("");
        }
    });
});


//Watermark
$('#rootDirInput').livequery(function () {
    $(this).watermark('Enter your new root folder path...');
});

$('#newSeriesLookup').livequery(function () {
    $(this).watermark('Title of the series you want to add...');
});

$('.existingSeriesContainer .seriesLookup').livequery(function () {
    $(this).watermark('Please enter the series title...');
});