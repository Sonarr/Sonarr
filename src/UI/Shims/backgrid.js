require('../JsLibraries/backbone.backgrid');
require('backbone');

require('backbone');

var backgrid = require('../JsLibraries/backbone.backgrid');
var header = require('../Shared/Grid/HeaderCell');

header.register(backgrid);

backgrid.Column.prototype.defaults = {
    name       : undefined,
    label      : undefined,
    sortable   : true,
    editable   : false,
    renderable : true,
    formatter  : undefined,
    cell       : undefined,
    headerCell : 'Sonarr',
    sortType   : 'toggle'
};
module.exports = backgrid;