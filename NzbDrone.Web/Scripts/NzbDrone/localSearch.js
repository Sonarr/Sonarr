jQuery(document).ready(function () {
    $.ajaxSetup({
        cache: false
    });

    var selector = '#localSeriesLookup';

    $(document).bind('keydown', 'ctrl+shift+f', function () {
        $(selector).focus();
    });

    $(document).bind('keyup', 's', function () {
        $(selector).focus();
    });


    $(selector).each(function (index, element) {
        $(element).blur(function () {
            $(element).val("");
        });

        $(element).watermark('Search...');

        $(element).autocomplete({
            source: "/Series/LocalSearch",
            minLength: 1,
            delay: 200,
            autoFocus: true,
            select: function (event, ui) {
                window.location = "../Series/Details?seriesId=" + ui.item.Id;
            }
        })

.data("autocomplete")._renderItem = function (ul, item) {
    return $("<li></li>")
.data("item.autocomplete", item)
.append("<a>" + item.Title + "<br>" + "</a>")
.appendTo(ul);
};
    });
});