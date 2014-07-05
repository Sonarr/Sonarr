'use strict';
define(
    [
        'Cells/TemplatedCell'
    ], function (TemplatedCell) {
        return TemplatedCell.extend({

            className: 'series-title',
            template : 'Cells/SeriesTitleTemplate'

        });
    });
