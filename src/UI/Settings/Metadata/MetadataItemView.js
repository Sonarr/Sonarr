var AppLayout = require('../../AppLayout');
var Marionette = require('marionette');
var EditView = require('./MetadataEditView');
var AsModelBoundView = require('../../Mixins/AsModelBoundView');

var view = Marionette.ItemView.extend({
    template : 'Settings/Metadata/MetadataItemViewTemplate',
    tagName  : 'li',

    events : {
        'click' : '_edit'
    },

    initialize : function() {
        this.listenTo(this.model, 'sync', this.render);
    },

    _edit : function() {
        var view = new EditView({ model : this.model });
        AppLayout.modalRegion.show(view);
    }
});

module.exports = AsModelBoundView.call(view);