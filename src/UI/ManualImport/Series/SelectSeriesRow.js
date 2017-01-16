var Backgrid = require('backgrid');

module.exports = Backgrid.Row.extend({
    className : 'select-row select-series-row',

    events : {
        'click' : '_onClick'
    },

    _onClick : function() {
        this.model.collection.trigger('row:selected', { model: this.model });
    }
});