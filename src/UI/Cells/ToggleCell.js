var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'toggle-cell',

    events : {
        'click' : '_onClick'
    },

    _onClick : function() {

        var self = this;

        this.$el.tooltip('hide');

        var name = this.column.get('name');
        this.model.set(name, !this.model.get(name));

        var promise = this.model.save();

        this.$('i').spinForPromise(promise);

        promise.always(function() {
            self.render();
        });
    },

    render : function() {
        this.$el.empty();
        this.$el.html('<i />');

        var name = this.column.get('name');

        if (this.model.get(name)) {
            this.$('i').addClass(this.column.get('trueClass'));
        } else {
            this.$('i').addClass(this.column.get('falseClass'));
        }

        var tooltip = this.column.get('tooltip');

        if (tooltip) {
            this.$('i').attr('title', tooltip);
        }

        return this;
    }
});