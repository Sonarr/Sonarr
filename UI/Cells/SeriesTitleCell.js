"use strict";
define(['app', 'Cells/TemplatedCell'], function () {
    NzbDrone.Cells.SeriesTitleCell = NzbDrone.Cells.TemplatedCell.extend({

        className: 'series-title',
        template : 'Cells/SeriesTitleTemplate'

    });
});
