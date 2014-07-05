'use strict';
define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'backup-type-cell',

            render: function () {
                this.$el.empty();

                var icon = 'icon-time';
                var title = 'Scheduled';

                var type = this.model.get(this.column.get('name'));

                if (type === 'manual') {
                    icon = 'icon-book';
                    title = 'Manual';
                }

                else if (type === 'update') {
                    icon = 'icon-retweet';
                    title = 'Before update';
                }

                this.$el.html('<i class="{0}" title="{1}"></i>'.format(icon, title));

                return this;
            }
        });
    });
