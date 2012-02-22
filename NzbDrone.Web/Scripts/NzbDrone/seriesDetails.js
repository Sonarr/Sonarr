var notIgnoredImage = '../../Content/Images/notIgnored.png';
var ignoredImage = '../../Content/Images/ignored.png';
var notAiredImage = '../../Content/Images/NotAired.png';
var readyImage = '../../Content/Images/Ready.png';
var downloadingImage = '../../Content/Images/Downloading.png';

var seriesId = 0;
var saveSeasonIgnoreUrl = '../Command/SaveSeasonIgnore';
var saveEpisodeIgnoreUrl = '../Command/SaveEpisodeIgnore';

//Episode Ignore Functions
$(".ignoreEpisode").live("click", function () {
    var toggle = $(this);
    var ignored = toggle.hasClass('ignored');

    if (ignored) {
        toggle.removeClass('ignored');
        toggle.attr('src', notIgnoredImage);
        toggleCellColour(toggle, false);
    }

    else {
        toggle.addClass('ignored');
        toggle.attr('src', ignoredImage);
        toggleCellColour(toggle, true);
    }

    var seasonNumber = 0;

    //Flip the ignored to the new state (We want the new value moving forward)
    ignored = !ignored;

    if (toggle.hasClass('ignoredEpisodesMaster')) {
        seasonNumber = toggle.attr('class').split(/\s+/)[2].replace('ignoreSeason_', '');

        toggleChildren(seasonNumber, ignored);
        toggleMasters(seasonNumber, ignored);
        saveSeasonIgnore(seasonNumber, ignored);
    }

    else {
        //Check to see if this is the last one ignored or the first not ignored
        var episodeId = toggle.attr('id');
        saveEpisodeIgnore(episodeId, ignored);
    }
});

function toggleChildren(seasonNumber, ignored) {
    var ignoreEpisodes = $('.ignoreEpisode_' + seasonNumber);

    if (ignored) {
        ignoreEpisodes.each(function (index) {
            $(this).addClass('ignored');
            $(this).attr('src', ignoredImage);
            toggleCellColour($(this), true);
        });
    }

    else {
        ignoreEpisodes.each(function (index) {
            $(this).removeClass('ignored');
            $(this).attr('src', notIgnoredImage);

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
            $(this).attr('src', ignoredImage);
        });
    }

    else {
        masters.each(function (index) {
            $(this).removeClass('ignored');
            $(this).attr('src', notIgnoredImage);
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
        if (toggle.parent('td').children('.statusImage').hasClass('status-Missing'))
            toggle.parent('td').addClass('episodeMissing');
    }
}

//Episode Ignore Saving
function saveSeasonIgnore(seasonNumber, ignored) {
    $.ajax({
        type: "POST",
        url: saveSeasonIgnoreUrl,
        data: jQuery.param({ seriesId: seriesId, seasonNumber: seasonNumber, ignored: ignored }),
        error: function (req, status, error) {
            alert("Sorry! We could save the ignore settings for Series: " + seriesId + ", Season: " + seasonNumber + " at this time. " + error);
        }
    });
}

function saveEpisodeIgnore(episodeId, ignored) {
    $.ajax({
        type: "POST",
        url: saveEpisodeIgnoreUrl,
        data: jQuery.param({ episodeId: episodeId, ignored: ignored }),
        error: function (req, status, error) {
            alert("Sorry! We could save the ignore settings for Episode: " + episodeId + " at this time. " + error);
        }
    });
}