function bindFolderAutoComplete(selector) {
    YUI().use("autocomplete", "autocomplete-highlighters", 'autocomplete-filters', function (Y) {
        Y.one('body').addClass('yui3-skin-sam');
        Y.one(selector).plug(Y.Plugin.AutoComplete, {


            resultHighlighter: 'startsWith',
            resultFilters: 'phraseMatch',
            source: '/Directory/GetDirectories/?q={query}'
        });
    })

}