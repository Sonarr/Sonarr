var AppLayout = require('../../AppLayout');
var Marionette = require('marionette');
var EditView = require('./Edit/NotificationEditView');

module.exports = Marionette.ItemView.extend({
    template : 'Settings/Notifications/NotificationItemViewTemplate',
    tagName  : 'li',

    events : {
        'click' : '_edit'
    },

    initialize : function() {
        this.listenTo(this.model, 'sync', this.render);
    },

    _edit : function() {
        var view = new EditView({
            model            : this.model,
            targetCollection : this.model.collection
        });
        AppLayout.modalRegion.show(view);
    }
});