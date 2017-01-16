var Marionette = require('marionette');
var RadioButtonView = require('./RadioButtonView');
var Config = require('../../../Config');

module.exports = Marionette.CollectionView.extend({
    className : 'btn-group',
    itemView  : RadioButtonView,

    attributes : {
        'data-toggle' : 'buttons'
    },

    initialize : function(options) {
        this.menu = options.menu;

        this.setActive();
    },

    setActive : function() {
        var storedKey = this.menu.defaultAction;

        if (this.menu.storeState) {
            storedKey = Config.getValue(this.menu.menuKey, storedKey);
        }

        if (!storedKey) {
            return;
        }
        this.collection.each(function(model) {
            if (model.get('key').toLocaleLowerCase() === storedKey.toLowerCase()) {
                model.set('active', true);
            } else {
                model.set('active, false');
            }
        });
    }
});