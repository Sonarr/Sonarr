jQuery(document).ready(function () {
    $.ajaxSetup({
        cache: false
    });

    bindAutoCompletes();

    $(document).bind('keydown', 'ctrl+shift+f', function () {
        $('#localSeriesLookup').focus();
    });
    $(document).bind('keyup', 's', function () {
        $('#localSeriesLookup').focus();
    });
});

//
$('.folderLookup:not(.ui-autocomplete-input), .seriesLookup:not(.ui-autocomplete-input), .localSeriesLookup:not(.ui-autocomplete-input)').live('focus', function (event) {
    bindAutoCompletes();
});

function bindAutoCompletes() {
    bindFolderAutoComplete(".folderLookup");
    bindSeriesAutoComplete(".seriesLookup");
    bindLocalSeriesAutoComplete("#localSeriesLookup");
}

function bindFolderAutoComplete(selector) {
    $(selector).each(function (index, element) {
        $(element).autocomplete({
            //source: "/Directory/GetDirectories",
            source: function (request, response) {
                $.ajax({
                    url: "/Directory/GetDirectories",
                    dataType: "json",
                    data: {
                        term: request.term
                    },
                    success: function (data) {
                        var re = $.ui.autocomplete.escapeRegex(request.term);
                        var matcher = new RegExp("^" + re, "i");
                        response($.grep(data, function (item) { return matcher.test(item); }));
                    }
                });
            },
            minLength: 3
        });
    });
}

function bindSeriesAutoComplete(selector) {

    $(selector).each(function (index, element) {
        $(element).autocomplete({
            source: "/AddSeries/LookupSeries",
            minLength: 3,
            delay: 500,
            select: function (event, ui) {
                $(this).val(ui.item.Title);
                $(this).siblings('.seriesId').val(ui.item.Id);
                return false;
            }
        })
	    .data("autocomplete")._renderItem = function (ul, item) {
	        return $("<li></li>")
			.data("item.autocomplete", item)
			.append("<a><strong>" + item.Title + "</strong><br>" + item.FirstAired + "</a>")
			.appendTo(ul);
	    };
    });
}

function bindLocalSeriesAutoComplete(selector) {

    $(selector).each(function (index, element) {
        $(element).watermark('Search...');
        $(element).autocomplete({
            source: "/Series/LocalSearch",
            minLength: 1,
            delay: 100,
            autoFocus:true,
            select: function (event, ui) {
                window.location = "../Series/Details?seriesId=" + ui.item.Id;
            }
        })
	    .data("autocomplete")._renderItem = function (ul, item) {
	        return $("<li></li>")
			.data("item.autocomplete", item)
			.append("<a><strong>" + item.Title + "</strong><br>" + "</a>")
			.appendTo(ul);
	    };
    });
}