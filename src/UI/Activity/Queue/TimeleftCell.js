'use strict';

define(
    [
        'Cells/NzbDroneCell',
        'filesize',
        'moment'
    ], function (NzbDroneCell, fileSize, moment) {
        return NzbDroneCell.extend({

            className: 'timeleft-cell',

            render: function () {
                this.$el.empty();

                if (this.cellValue) {

                    //If the release is pending we want to use the timeleft as the time it will be processed at
                    if (this.cellValue.get('status').toLowerCase() === 'pending') {
                        this.$el.html('-');
                        this.$el.attr('title', 'Will be processed during the first RSS Sync after {0}'.format(moment(this.cellValue.get('estimatedCompletionTime')).calendar()));
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
