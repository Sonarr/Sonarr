"use strict";
define(
    [
        'backgrid'
    ], function (Backgrid) {

        return Backgrid.Row.extend({
            events: {
                'click .x-search': 'search'
            },
            search: function () {
                window.alert('Episode Search');
            }
        });
    });

