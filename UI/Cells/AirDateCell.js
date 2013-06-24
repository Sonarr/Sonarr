'use strict';
define(['app', 'Shared/FormatHelpers', 'Cells/NzbDroneCell'], function () {
    return NzbDrone.Cells.NzbDroneCell.extend({
        className: 'air-date-cell',

        render: function () {

            this.$el.empty();
            var airDate = this.model.get(this.column.get('name'));
            this.$el.html(NzbDrone.Shared.FormatHelpers.DateHelper(airDate));
            return this;

        }
    });
});
