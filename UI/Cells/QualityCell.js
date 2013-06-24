'use strict';
define(
    [
        'Cells/TemplatedCell'
    ], function (TemplatedCell) {
        return TemplatedCell.extend({

            className: 'quality-cell',
            template : 'Cells/QualityTemplate'

        });
    });
