'use strict';
define(['app', 'Cells/TemplatedCell'], function (App, TemplatedCell) {
    return TemplatedCell.extend({

        className: 'quality-cell',
        template : 'Cells/QualityTemplate'

    });
});
