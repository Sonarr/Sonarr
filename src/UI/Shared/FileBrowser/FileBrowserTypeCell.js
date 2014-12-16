'use strict';

define(
    [
        'vent',
        'Cells/NzbDroneCell'
    ], function (vent, NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'file-browser-type-cell',

            render: function () {
                this.$el.empty();

                var type = this.model.get(this.column.get('name'));
                var icon = 'icon-hdd';

                if (type === 'computer') {
                    icon = 'icon-desktop';
                }

                else if (type === 'parent') {
                    icon = 'icon-level-up';
                }

                else if (type === 'folder') {
                    icon = 'icon-folder-close';
                }

                else if (type === 'file') {
                    icon = 'icon-file';
                }

                this.$el.html('<i class="{0}"></i>'.format(icon));

                this.delegateEvents();
                return this;
            }
        });
    });