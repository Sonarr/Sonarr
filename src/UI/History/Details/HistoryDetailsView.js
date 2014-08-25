'use strict';
define(
    [
        'marionette',
        'History/Details/HistoryDetailsAge'
    ], function (Marionette) {

        return Marionette.ItemView.extend({
            template: 'History/Details/HistoryDetailsViewTemplate'
        });
    });
