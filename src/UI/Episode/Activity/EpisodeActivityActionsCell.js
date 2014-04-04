'use strict';

define(
    [
        'jquery',
        'vent',
        'marionette',
        'Cells/NzbDroneCell'
    ], function ($, vent, Marionette, NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'episode-actions-cell',

            events: {
                'click .x-failed' : '_markAsFailed'
            },

            render: function () {
                this.$el.empty();

                if (this.model.get('eventType') === 'grabbed') {
                    this.$el.html('<i class="icon-nd-delete x-failed" title="Mark download as failed"></i>');
                }

                return this;
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
            }
        });
    });
