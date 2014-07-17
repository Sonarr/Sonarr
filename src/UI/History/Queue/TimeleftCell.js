'use strict';

define(
    [
        'Cells/NzbDroneCell',
        'filesize'
    ], function (NzbDroneCell, fileSize) {
        return NzbDroneCell.extend({

            className: 'timeleft-cell',

            render: function () {
                this.$el.empty();

                if (this.cellValue) {

                    var timeleft = this.cellValue.get('timeleft');
                    var totalSize = fileSize(this.cellValue.get('size'), 1, false);
                    var remainingSize = fileSize(this.cellValue.get('sizeleft'), 1, false);

                    if (timeleft === undefined) {
                        this.$el.html("-");
                    }
                    else {
                        this.$el.html('<span title="{1} / {2}">{0}</span>'.format(timeleft, remainingSize, totalSize));
                    }
                }

                return this;
            }
        });
    });
