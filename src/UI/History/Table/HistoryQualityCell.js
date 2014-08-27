'use strict';
define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'history-quality-cell',

            render: function () {

                var title = '';
                var quality = this.model.get('quality');

                if (quality.proper) {
                    title = 'PROPER';
                }

                if (this.model.get('qualityCutoffNotMet')) {
                    this.$el.html('<span class="badge badge-inverse" title="{0}">{1}</span>'.format(title, quality.quality.name));
                }
                else {
                    this.$el.html('<span class="badge" title="{0}">{1}</span>'.format(title, quality.quality.name));
                }

                return this;
            }


        });
    });
