"use strict";
define(['app', 'Cells/TemplatedCell'], function () {
    NzbDrone.Cells.QualityCell = NzbDrone.Cells.TemplatedCell.extend({

        className: 'quality-cell',
        template : 'Cells/QualityTemplate'

    });
});
