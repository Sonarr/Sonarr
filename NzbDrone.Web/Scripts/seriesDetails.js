var notIgnoredImage = '../../Content/Images/notIgnored.png';
var ignoredImage = '../../Content/Images/ignored.png';

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

    if (toggle.hasClass('ignoredEpisodesMaster')) {
        var seasonNumber = toggle.attr('id').replace('master_', '');

        toggleChildren(seasonNumber, ignored);
    }
});

function toggleChildren(seasonNumber, ignored) {
    var ignoreEpisodes = $('.ignoreEpisode_' + seasonNumber);

    if (!ignored) {
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

function grid_rowBound(e) {
    var dataItem = e.dataItem;
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
}

function grid_dataBound(e) {
    var id = $(this).attr('id');
    var seasonNumber = id.replace('seasons_', '');
    var ignoreEpisodes = $('.ignoreEpisode_' + seasonNumber);
    var master = $('#master_' + seasonNumber);
    var count = ignoreEpisodes.length;
    var ignoredCount = 0;

    ignoreEpisodes.each(function (index) {
        if ($(this).hasClass('ignored')) {
            ignoredCount++;
        }
    });

    if (ignoredCount == count) {
        master.attr('src', ignoredImage);
        master.addClass('ignored');
    }

    else {
        master.attr('src', notIgnoredImage);
        master.removeClass('ignored');
    }
}