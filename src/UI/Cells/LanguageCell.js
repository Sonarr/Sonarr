var TemplatedCell = require('./TemplatedCell');
var LanguageCellEditor = require('./Edit/LanguageCellEditor');

module.exports = TemplatedCell.extend({
    className : 'language-cell',
    template  : 'Cells/LanguageCellTemplate',
    editor    : LanguageCellEditor
});