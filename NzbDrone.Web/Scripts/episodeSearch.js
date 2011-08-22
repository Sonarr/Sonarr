var searchUrl = '../Episode/Search';

function searchForEpisode(id) {
    $.ajax({
        type: "POST",
        url: searchUrl,
        data: jQuery.param({ episodeId: id }),
        error: function (req, status, error) {
            alert("Sorry! We could search for " + id + " at this time. " + error);
        }
    });
}