'use strict';
define(
    [
        'Cells/NzbDroneCell',
        'moment',
        'Shared/FormatHelpers',
        'Shared/UiSettingsModel'
    ], function (NzbDroneCell, moment, FormatHelpers, UiSettings) {
        return NzbDroneCell.extend({

            className: 'relative-time-cell',

            render: function () {

                var dateStr = this.model.get(this.column.get('name'));

                if (dateStr) {
                    var date = moment(dateStr);
                    var result = '<span title="{0}">{1}</span>';

                    if (UiSettings.get('showRelativeDates')) {
                        var tooltip = date.format(UiSettings.longDateTime());
                        var text = date.fromNow();

                        this.$el.html(result.format(tooltip, text));
                    }

                    else {
                        this.$el.html(date.format(UiSettings.longDateTime()));
                    }


                }

                return this;
            }
        });
    });
