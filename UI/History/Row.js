"use strict";
define(['app','backgrid'], function () {
    NzbDrone.Missing.Row = Backgrid.Row.extend({
        events: {
            'click .x-search': 'search'
        },

        search: function () {
            window.alert('Episode Search');
        }
    });

    return NzbDrone.Missing.Row;
});
