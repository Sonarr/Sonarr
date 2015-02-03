var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var EditView = require('./RestrictionEditView');

module.exports = Marionette.ItemView.extend({
    template   : 'Settings/Indexers/Restriction/RestrictionItemViewTemplate',
    className  : 'row',
    ui         : {tags : '.x-tags'},
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