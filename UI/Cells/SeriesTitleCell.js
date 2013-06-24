'use strict';
define(['app', 'Cells/TemplatedCell'], function (App, TemplatedCell) {
    return TemplatedCell.extend({

        className: 'series-title',
        template : 'Cells/SeriesTitleTemplate'

    });
});
