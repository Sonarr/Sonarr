var $ = require('jquery');
var _ = require('underscore');
var BackgridSelectAll = require('backgrid.selectall');

module.exports = BackgridSelectAll.extend({
    enterEditMode : function(e) {
        var collection = this.column.get('sortedCollection') || this.model.collection;

        if (e.shiftKey && collection.lastToggled) {
            this._selectRange(collection);
        }

        var checked = $(e.target).prop('checked');

        collection.lastToggled = this.model;
        collection.checked = checked;
    },

    onChange : function(e) {
        var checked = $(e.target).prop('checked');
        this.$el.parent().toggleClass('selected', checked);
        this.model.trigger('backgrid:selected', this.model, checked);
    },

    _selectRange : function(collection) {
        var lastToggled = collection.lastToggled;
        var checked = collection.checked;

        var currentIndex = collection.indexOf(this.model);
        var lastIndex = collection.indexOf(lastToggled);

        var low = Math.min(currentIndex, lastIndex);
        var high = Math.max(currentIndex, lastIndex);
        var range = _.range(low + 1, high);

        _.each(range, function(index) {
            var model = collection.at(index);

            model.trigger('backgrid:select', model, checked);
        });

        collection.lastToggled = undefined;
        collection.checked = undefined;
    }
});