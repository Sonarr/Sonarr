var _ = require('underscore');
var Backgrid = require('backgrid');

module.exports = Backgrid.Row.extend({
    className : 'file-browser-row',

    events : {
        'click' : '_selectRow'
    },

    _originalInit : Backgrid.Row.prototype.initialize,

    initialize : function() {
        this._originalInit.apply(this, arguments);
    },

    _selectRow : function() {
        if (this.model.get('type') === 'file') {
            this.model.collection.trigger('filebrowser:row:fileselected', this.model);
        } else {
            this.model.collection.trigger('filebrowser:row:folderselected', this.model);
        }
    }
});