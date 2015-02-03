var TemplatedCell = require('./TemplatedCell');
var QualityCellEditor = require('./Edit/QualityCellEditor');

module.exports = TemplatedCell.extend({
    className : 'quality-cell',
    template  : 'Cells/QualityCellTemplate',
    editor    : QualityCellEditor
});