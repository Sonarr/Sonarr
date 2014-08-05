'use strict';
define(
    [
        'Cells/NzbDroneCell',
        'moment',
        'Shared/FormatHelpers',
        'Shared/UiSettingsModel'
    ], function (NzbDroneCell, moment, FormatHelpers, UiSettings) {
        return NzbDroneCell.extend({

            className: 'relative-date-cell',

            render: function () {

                var dateStr = this.model.get(this.column.get('name'));

                if (dateStr) {
                    var date = moment(dateStr);
                    var result = '<span title="{0}">{1}</span>';
                    var tooltip = date.format(UiSettings.longDateTime());
                    var text;

                    if (UiSettings.get('showRelativeDates')) {
                        text = FormatHelpers.relativeDate(dateStr);
                    }

                    else {
                        text = date.format(UiSettings.get('shortDateFormat'));
                    }

                    this.$el.html(result.format(tooltip, text));
                }

                return this;
            }
        });
    });
