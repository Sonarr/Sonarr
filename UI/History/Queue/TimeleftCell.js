'use strict';

define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'history-event-type-cell',

            render: function () {
                this.$el.empty();

                if (this.cellValue) {

                    var timeleft = this.cellValue.get('timeleft');
                    var size = this.cellValue.get('size');
                    var sizeLeft = this.cellValue.get('sizeLeft');

                    this.$el.html(timeleft);
                    this.$el.attr('title', '{0} MB / {1} MB'.format(sizeLeft, size));
                }

                return this;
            }
        });
    });
