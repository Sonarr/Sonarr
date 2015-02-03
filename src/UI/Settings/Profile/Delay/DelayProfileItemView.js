var $ = require('jquery');
var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var EditView = require('./Edit/DelayProfileEditView');

module.exports = Marionette.ItemView.extend({
    template   : 'Settings/Profile/Delay/DelayProfileItemViewTemplate',
    className  : 'row',
    events     : {"click .x-edit" : '_edit'},
    initialize : function(){
        this.listenTo(this.model, 'sync', this.render);
    },
    _edit      : function(){
        var view = new EditView({
            model            : this.model,
            targetCollection : this.model.collection
        });
        AppLayout.modalRegion.show(view);
    }
});