'use strict';
define(
    [
        'Cells/NzbDroneCell',
        'moment',
        'Shared/UiSettingsModel'
    ], function (NzbDroneCell, moment, UiSettings) {
        return NzbDroneCell.extend({

            className: 'log-time-cell',

            render: function () {

                var date = moment(this._getValue());
                this.$el.html('<span title="{1}">{0}</span>'.format(date.format(UiSettings.get('timeFormat').replace('(', '').replace(')', '')), date.format(UiSettings.longDateTime())));

                return this;
            }
        });
    });
