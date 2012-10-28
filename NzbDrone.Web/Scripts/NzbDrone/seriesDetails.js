var seriesId = 0;
var saveSeasonIgnoreUrl = '/command/saveseasonignore';
var saveEpisodeIgnoreUrl = '/command/saveepisodeignore';

var changeQualityType;
var changeQualityData;
var changeEpisodeQualityUrl = '/episode/changeepisodequality';
var changeSeasonQualityUrl = '/episode/changeseasonquality';

//Episode Ignore Functions
$(".ignoreEpisode").live("click", function () {
    var toggle = $(this);
    var ignored = toggle.hasClass('ignored');

    if (ignored) {
        toggle.removeClass('ignored');
        toggleCellColour(toggle, false);
    }

    else {
        toggle.addClass('ignored');
        toggleCellColour(toggle, true);
    }

    var seasonNumber = 0;

    //Flip the ignored to the new state (We want the new value moving forward)
    ignored = !ignored;

    if (toggle.hasClass('ignoredEpisodesMaster')) {
        seasonNumber = toggle.attr('data-season');

        toggleChildren(seasonNumber, ignored);
        toggleMasters(seasonNumber, ignored);
        saveSeasonIgnore(seasonNumber, ignored);
    }

    else {
        //Check to see if this is the last one ignored or the first not ignored
        var episodeId = toggle.attr('data-episode-id');
        saveEpisodeIgnore(episodeId, ignored);
    }
});

function toggleChildren(seasonNumber, ignored) {
    var ignoreEpisodes = $('[data-season="' + seasonNumber + '"]:not(table)');

    if (ignored) {
        ignoreEpisodes.each(function (index) {
            $(this).addClass('ignored');
            toggleCellColour($(this), true);
        });
    }

    else {
        ignoreEpisodes.each(function (index) {
            $(this).removeClass('ignored');

            toggleCellColour($(this), false);
        });
    }
}

function toggleMasters(seasonNumber, ignored) {
    //Toggles the other master(s) to match the one that was just changed
    var masters = $('.ignoreSeason_' + seasonNumber);

    if (ignored) {
        masters.each(function (index) {
            $(this).addClass('ignored');
        });
    }

    else {
        masters.each(function (index) {
            $(this).removeClass('ignored');
        });
    }
}

function toggleCellColour(toggle, ignored) {
    if (ignored) {
        toggle.parent('td').addClass('episodeIgnored');
        toggle.parent('td').removeClass('episodeMissing');
    }
    
    else {
        toggle.parent('td').removeClass('episodeIgnored');

        //check to see if episode is missing
        if (toggle.parent('td').children('.statusImage').attr('data-status') === 'Missing')
            toggle.parent('td').addClass('episodeMissing');
    }
}

//Episode Ignore Saving
function saveSeasonIgnore(seasonNumber, ignored) {
    $.ajax({
        type: "POST",
        url: saveSeasonIgnoreUrl,
        data: jQuery.param({ seriesId: seriesId, seasonNumber: seasonNumber, ignored: ignored }),
    });
}

function saveEpisodeIgnore(episodeId, ignored) {
    $.ajax({
        type: "POST",
        url: saveEpisodeIgnoreUrl,
        data: jQuery.param({ episodeId: episodeId, ignored: ignored }),
    });
}

//Change quality
$(document).on('click', '.changeQuality', function() {   
    changeQualityType = $(this).attr('data-changetype');

    if (changeQualityType === "episode") {
        var row = $(this).closest('tr');

        changeQualityData = $(row).attr('data-episodefileid');
        
        if (changeQualityData === "0")
            return;
        
        var qualityId = $(row).find('.episodeQuality').attr('data-qualityid');
        $('#NewQuality').val(qualityId);
    }

    else {
        changeQualityData = $(this).closest('table').attr('data-season');
    }
    
    $('#qualityChanger').dialog('open');
});

$("#qualityChanger").dialog({
    autoOpen: false,
    height: 'auto',
    width: 670,
    resizable: false,
    modal: true,
    buttons: {
        "Save": function () {        
            //Save the quality
            var newQualityId = $('#NewQuality').val();
            var newQualityText = $('#NewQuality :selected').text();

            if (changeQualityType === "episode") {
                $.ajax({
                    url: changeEpisodeQualityUrl,
                    data: { episodeFileId: changeQualityData, quality: newQualityId },
                    type: 'POST',
                    success: function(data) {
                        var row = $('tr[data-episodefileid="' + changeQualityData + '"]');
                        var qualityCell = $(row).find('.episodeQuality');
                        $(qualityCell).attr('data-qualityid', newQualityId);
                        $(qualityCell).text(newQualityText);
                    }
                });
            }

            else {
                $.ajax({
                    url: changeSeasonQualityUrl,
                    data: { seriesId: seriesId, seasonNumber: changeQualityData, quality: newQualityId },
                    type: 'POST',
                    success: function (data) {
                        var table = $('table[data-season="' + changeQualityData + '"]');
                        var rows = $(table).find('tbody').children('tr');

                        $(rows).each(function() {   
                            if ($(this).attr('data-episodefileid') === '0')
                                return;
                            
                            var qualityCell = $(this).find('.episodeQuality');
                            $(qualityCell).attr('data-qualityid', newQualityId);
                            $(qualityCell).text(newQualityText);
                        });
                    }
                });
            }

            $(this).dialog("close");
        },
        Cancel: function () {
            $(this).dialog("close");
        }
    }
});