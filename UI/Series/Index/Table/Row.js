'use strict';
define(['app','backgrid'], function () {
    NzbDrone.Series.Index.Table.Row = Backgrid.Row.extend({
        events: {
            'click .x-edit'  : 'editSeries',
            'click .x-remove': 'removeSeries'
        },

        editSeries: function () {
            var view = new NzbDrone.Series.Edit.EditSeriesView({ model: this.model});
            NzbDrone.modalRegion.show(view);
        },

        removeSeries: function () {
            var view = new NzbDrone.Series.Delete.DeleteSeriesView({ model: this.model });
            NzbDrone.modalRegion.show(view);
        }
    });

    return NzbDrone.Series.Table.Row;
});

