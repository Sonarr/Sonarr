'use strict';
define(
    [
        'Cells/TemplatedCell',
        'Cells/Edit/QualityCellEditor'
    ], function (TemplatedCell, QualityCellEditor) {
        return TemplatedCell.extend({

            className: 'quality-cell',
            template : 'Cells/QualityCellTemplate',
            editor   : QualityCellEditor
        });
    });
