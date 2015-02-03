require('../JsLibraries/backbone.backgrid');
require('backbone');

require('backbone');
var backgrid = require('../JsLibraries/backbone.backgrid');
backgrid.Column.prototype.defaults = {
    name       : undefined,
    label      : undefined,
    sortable   : true,
    editable   : false,
    renderable : true,
    formatter  : undefined,
    cell       : undefined,
    sortType   : 'toggle'
};
module.exports = backgrid;