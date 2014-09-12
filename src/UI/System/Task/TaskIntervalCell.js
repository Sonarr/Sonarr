'use strict';
define(
    [
        'Cells/NzbDroneCell',
        'moment'
    ], function (NzbDroneCell, moment) {
        return NzbDroneCell.extend({

            className: 'task-interval-cell',

            render: function () {

                this.$el.empty();

                var interval = this.model.get('interval');
                var duration = moment.duration(interval, 'minutes').humanize();

                this.$el.html(
                    duration.replace(/an?(?=\s)/, '1')
                );

                return this;
            }
        });
    });
