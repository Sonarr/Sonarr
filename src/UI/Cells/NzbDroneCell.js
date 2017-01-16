var Backgrid = require('backgrid');
var Backbone = require('backbone');

module.exports = Backgrid.Cell.extend({

    _originalInit : Backgrid.Cell.prototype.initialize,

    initialize : function() {
        this._originalInit.apply(this, arguments);
        this.cellValue = this._getValue();

        this.listenTo(this.model, 'change', this._refresh);

        if (this._onEdit) {
            this.listenTo(this.model, 'backgrid:edit', function(model, column, cell, editor) {
                if (column.get('name') === this.column.get('name')) {
                    this._onEdit(model, column, cell, editor);
                }
            });
        }
    },

    _refresh : function() {
        this.cellValue = this._getValue();
        this.render();
    },

    _getValue : function() {

        var cellValue = this.column.get('cellValue');

        if (cellValue) {
            if (cellValue === 'this') {
                return this.model;
            }

            else {
                return this.model.get(cellValue);
            }
        }

        var name = this.column.get('name');

        if (name === 'this') {
            return this.model;
        }

        var value = this.model.get(name);

        if (!value) {
            return undefined;
        }

        //if not a model
        if (!value.get && typeof value === 'object') {
            value = new Backbone.Model(value);
        }

        return value;
    }
});