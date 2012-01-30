var notIgnoredImage = '../../Content/Images/notIgnored.png';
var ignoredImage = '../../Content/Images/ignored.png';
var notAiredImage = '../../Content/Images/NotAired.png';
var readyImage = '../../Content/Images/Ready.png';
var downloadingImage = '../../Content/Images/Downloading.png';

var seriesId = 0;
var saveSeasonIgnoreUrl = '../Series/SaveSeasonIgnore';
var saveEpisodeIgnoreUrl = '../Series/SaveEpisodeIgnore';

//Episode Ignore Functions
$(".ignoreEpisode").live("click", function () {
    var toggle = $(this);
    var ignored = toggle.hasClass('ignored');

    if (ignored) {
        toggle.removeClass('ignored');
        toggle.attr('src', notIgnoredImage);
    }

    else {
        toggle.addClass('ignored');
        toggle.attr('src', ignoredImage);
    }

    var seasonNumber = 0;

    //Flip the ignored to the new state (We want the new value moving forward)
    ignored = !ignored;

    if (toggle.hasClass('ignoredEpisodesMaster')) {
        //seasonNumber = toggle.attr('id').replace('master_', '');
        seasonNumber = toggle.attr('class').split(/\s+/)[2].replace('ignoreSeason_', '');

        toggleChildren(seasonNumber, ignored);
        toggleMasters(seasonNumber, ignored);
        saveSeasonIgnore(seasonNumber, ignored);
    }

    else {
        //Check to see if this is the last one ignored or the first not ignored
        seasonNumber = toggle.attr('class').split(/\s+/)[1].replace('ignoreEpisode_', '');
        var episodeId = toggle.attr('id');
        toggleMaster(seasonNumber, ignored);
        saveEpisodeIgnore(episodeId, ignored);
    }
});

function toggleChildren(seasonNumber, ignored) {
    var ignoreEpisodes = $('.ignoreEpisode_' + seasonNumber);

    if (ignored) {
        ignoreEpisodes.each(function (index) {
            $(this).addClass('ignored');
            $(this).attr('src', ignoredImage);
        });
    }

    else {
        ignoreEpisodes.each(function (index) {
            $(this).removeClass('ignored');
            $(this).attr('src', notIgnoredImage);
        });
    }
}

function toggleMaster(seasonNumber) {
    //Toggles all master toggles when the childen changes or the grid is loaded
    
    var ignoreEpisodes = $('.ignoreEpisode_' + seasonNumber);
    var ignoredCount = ignoreEpisodes.filter('.ignored').length;
    var masters = $('.ignoreSeason_' + seasonNumber);

    masters.each(function (index) {
        if (ignoreEpisodes.length == ignoredCount) {
            $(this).attr('src', ignoredImage);
            $(this).addClass('ignored');
        }

        else {
            $(this).attr('src', notIgnoredImage);
            $(this).removeClass('ignored');
        }
    });
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

//Functions called by the Telerik Season Grid 
function grid_rowBound(e) {
    var dataItem = e.dataItem;
    var row = e.row;
    var ignored = dataItem.Ignored;
    var episodeId = dataItem.EpisodeId;

    var ignoredIcon = $('#' + episodeId);

    if (ignored) {
        ignoredIcon.attr('src', ignoredImage);
    }

    else {
        ignoredIcon.attr('src', notIgnoredImage);
        ignoredIcon.removeClass('ignored');
    }

    if (seriesId == 0)
        seriesId = dataItem.SeriesId;

    highlightRow(e);
}

function grid_dataBound(e) {
    var id = $(this).attr('id');
    var seasonNumber = id.replace('seasons_', '');

    toggleMaster(seasonNumber);
    setMasterStatus(this);
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

//Set master status to match children
function setMasterStatus(grid) {
    //Get children of this grid
    var masterStatus = $(grid).find('.statusImageMaster');
    var statuses = $(grid).find('.statusImage').filter(':not(.statusImageMaster)');
    var episodeCount = statuses.length;
    
    //Get missing count
    if (statuses.filter('.status-Missing').length == episodeCount) {
        //Leave as is (default is missing)
        return;
    }

    if (statuses.filter('.status-NotAired').length == episodeCount) {
        masterStatus.attr('src', notAiredImage);
        return;
    }

    if (statuses.filter('.status-Ready').length == episodeCount) {
        masterStatus.attr('src', readyImage);
        return;
    }

    if (statuses.filter('.status-Downloading').length == episodeCount) {
        masterStatus.attr('src', downloadingImage);
        return;
    }
}