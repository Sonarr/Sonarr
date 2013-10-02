'use strict';

define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'timeleft-cell',

            render: function () {
                this.$el.empty();

                if (this.cellValue) {

                    var timeleft = this.cellValue.get('timeleft');
                    var size = this.cellValue.get('size');
                    var sizeleft = this.cellValue.get('sizeleft');

                    this.$el.html(timeleft);
                    this.$el.attr('title', '{0} MB / {1} MB'.format(sizeleft, size));
                }

                return this;
            }
        });
    });
