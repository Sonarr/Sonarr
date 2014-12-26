'use strict';

define(
    [
        'Cells/NzbDroneCell',
        'filesize',
        'moment',
        'Shared/UiSettingsModel',
        'Shared/FormatHelpers'
    ], function (NzbDroneCell, fileSize, moment, UiSettingsModel, FormatHelpers) {
        return NzbDroneCell.extend({

            className: 'timeleft-cell',

            render: function () {
                this.$el.empty();

                if (this.cellValue) {

                    //If the release is pending we want to use the timeleft as the time it will be processed at
                    if (this.cellValue.get('status').toLowerCase() === 'pending') {
                        var ect = this.cellValue.get('estimatedCompletionTime');
                        var time = '{0} at {1}'.format(FormatHelpers.relativeDate(ect), moment(ect).format(UiSettingsModel.time(true, false)));

                        this.$el.html('-');
                        this.$el.attr('title', 'Will be processed during the first RSS Sync after {0}'.format(time));
                        this.$el.attr('data-container', 'body');

                        return this;
                    }

                    var timeleft = this.cellValue.get('timeleft');
                    var totalSize = fileSize(this.cellValue.get('size'), 1, false);
                    var remainingSize = fileSize(this.cellValue.get('sizeleft'), 1, false);

                    if (timeleft === undefined) {
                        this.$el.html('-');
                    }
                    else {
                        this.$el.html('<span title="{1} / {2}">{0}</span>'.format(timeleft, remainingSize, totalSize));
                    }
                }

                return this;
            }
        });
    });
