//Hidden title string sorting
$.extend(jQuery.fn.dataTableExt.oSort, {
    "title-string-pre": function (a) {
        return a.match(/title="(.*?)"/)[1].toLowerCase();
    },

    "title-string-asc": function (a, b) {
        return ((a < b) ? -1 : ((a > b) ? 1 : 0));
    },

    "title-string-desc": function (a, b) {
        return ((a < b) ? 1 : ((a > b) ? -1 : 0));
    }
});


//bestDateString sorting
$.extend(jQuery.fn.dataTableExt.oSort, {
    "best-date-pre": function (a) {
        var match = a.match(/data-date="(.*?)"/)[1];

        if (match === '')
            return Date.create().addYears(100);

        return Date.create(match);
    },

    "best-date-asc": function (a, b) {
        return ((a < b) ? -1 : ((a > b) ? 1 : 0));
    },

    "best-date-desc": function (a, b) {
        return ((a < b) ? 1 : ((a > b) ? -1 : 0));
    }
});


//Skip articles sorting
$.extend(jQuery.fn.dataTableExt.oSort, {
    "skip-articles-pre": function (a) {
        return a.replace(/^(the|an|a|) /i, "");
    },

    "skip-articles-asc": function (a, b) {
        return ((a < b) ? -1 : ((a > b) ? 1 : 0));
    },

    "skip-articles-desc": function (a, b) {
        return ((a < b) ? 1 : ((a > b) ? -1 : 0));
    }
});