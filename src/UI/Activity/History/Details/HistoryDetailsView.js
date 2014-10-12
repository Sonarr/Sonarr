'use strict';
define(
    [
        'marionette',
        'Activity/History/Details/HistoryDetailsAge'
    ], function (Marionette) {

        return Marionette.ItemView.extend({
            template: 'Activity/History/Details/HistoryDetailsViewTemplate'
        });
    });
