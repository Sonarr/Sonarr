$.tablesorter.addParser({
    // set a unique id 
    id    : 'title',
    is    : function () {
        // return false so this parser is not auto detected 
        return false;
    },
    format: function (s) {
        // format your data for normalization
        return s.match(/title="(.*?)"/)[1].toLowerCase();
    },
    // set type, either numeric or text 
    type  : 'text'
});

$.tablesorter.addParser({
    // set a unique id 
    id    : 'date',
    is    : function () {
        // return false so this parser is not auto detected 
        return false;
    },
    format: function (s) {
        // format your data for normalization
        var match = s.match(/data-date="(.*?)"/)[1];

        if (match === '') {
            return Date.create().addYears(100).format(Date.ISO8601_DATETIME);
        }

        return match;
    },
    // set type, either numeric or text 
    type  : 'text'
});

$.tablesorter.addParser({
    // set a unique id 
    id    : 'innerHtml',
    is    : function () {
        // return false so this parser is not auto detected 
        return false;
    },
    format: function (s) {
        // format your data for normalization
        return $(s).get(0).innerHTML;
    },
    // set type, either numeric or text 
    type  : 'text'
});
