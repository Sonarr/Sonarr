'use strict';
define(
    [
        'Cells/NzbDroneCell',
        'moment',
        'Shared/UiSettingsModel'
    ], function (NzbDroneCell, moment, UiSettings) {
        return NzbDroneCell.extend({

            className: 'next-execution-cell',

            render: function () {

                this.$el.empty();

                var interval = this.model.get('interval');
                var nextExecution = moment(this.model.get('nextExecution'));

                if (interval === 0 ) {
                    this.$el.html('-');
                }

                else if (moment().isAfter(nextExecution)) {
                    this.$el.html('now');
                }

                else {
                    var result = '<span title="{0}">{1}</span>';

                    if (UiSettings.get('showRelativeDates')) {
                        var tooltip = nextExecution.format(UiSettings.longDateTime());
                        var text = nextExecution.fromNow();

                        this.$el.html(result.format(tooltip, text));
                    }

                    else {
                        this.$el.html(nextExecution.format(UiSettings.longDateTime()));
                    }
                }

                return this;
            }
        });
    });
