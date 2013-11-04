'use strict';
define(
    [
        'vent',
        'marionette',
        'jquery'
    ], function (vent, Marionette, $) {

        return Marionette.ItemView.extend({
            template: 'History/Details/HistoryDetailsViewTemplate',

            events: {
                'click .x-mark-as-failed': '_markAsFailed'
            },

            _markAsFailed: function () {
                var url = window.NzbDrone.ApiRoot + '/history/failed';
                var data = {
                    id: this.model.get('id')
                };

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: data
                });
                vent.trigger(vent.Commands.CloseModalCommand);
            }
        });
    });
