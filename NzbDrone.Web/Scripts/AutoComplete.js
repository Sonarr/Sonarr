jQuery(document).ready(function () {
    $.ajaxSetup({
        cache: false
    });

    $('.folderLookup').livequery(function () {
        bindFolderAutoComplete(".folderLookup");
    });
    
    $('.seriesLookup').livequery(function () {
        bindSeriesAutoComplete(".seriesLookup");
    });


});

function bindFolderAutoComplete(selector) {

    $(selector).each(function (index, element) {

        YUI().use("autocomplete", "autocomplete-highlighters", 'autocomplete-filters', function (Y) {
            Y.one('body').addClass('yui3-skin-sam');
            Y.one(element).plug(Y.Plugin.AutoComplete, {
                resultHighlighter: 'startsWith',
                resultFilters: 'phraseMatch',
                source: '/Directory/GetDirectories/?q={query}'
            });
        });
    });

}

function bindSeriesAutoComplete(selector) {

    $(selector).each(function (index, element) {
        YUI().use("autocomplete", "autocomplete-highlighters", 'autocomplete-filters', function (Y) {
            Y.one('body').addClass('yui3-skin-sam');
            Y.one(element).plug(Y.Plugin.AutoComplete, {
                resultHighlighter: 'startsWith',
                resultFilters: 'phraseMatch',
                resultTextLocator: 'Value',
                minQueryLength: 3,
                queryDelay: 500,
                source: '/AddSeries/LookupSeries/?q={query}'
            });
        });
    });
}