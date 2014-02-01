'use strict';

define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'blacklist-controls-cell',

            events: {
                'click': '_delete'
            },

            render: function () {
                this.$el.empty();
                this.$el.html('<i class="icon-nd-delete"></i>');

                return this;
            },

            _delete: function () {
                this.model.destroy();
            }
        });
    });
