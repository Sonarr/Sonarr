var Marionette = require('marionette');
var ButtonView = require('./ButtonView');

module.exports = Marionette.CollectionView.extend({
    className : 'btn-group',
    itemView  : ButtonView,

    initialize : function(options) {
        this.menu = options.menu;
        this.className = 'btn-group';

        if (options.menu.collapse) {
            this.className += ' btn-group-collapse';
        }
    },

    onRender : function() {
        if (this.menu.collapse) {
            this.$el.addClass('btn-group-collapse');
        }
    }
});