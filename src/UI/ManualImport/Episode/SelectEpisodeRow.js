var Backgrid = require('backgrid');

module.exports = Backgrid.Row.extend({
    className : 'select-episode-row',

    events : {
        'click' : '_toggle'
    },

    _toggle : function(e) {

        if (e.target.type === 'checkbox') {
            return;
        }

        var checked = this.$el.find('.select-row-cell :checkbox').prop('checked');

        this.model.trigger('backgrid:select', this.model, !checked);
    }
});