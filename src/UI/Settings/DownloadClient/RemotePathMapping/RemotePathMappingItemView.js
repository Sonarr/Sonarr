var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var EditView = require('./RemotePathMappingEditView');

module.exports = Marionette.ItemView.extend({
    template     : 'Settings/DownloadClient/RemotePathMapping/RemotePathMappingItemViewTemplate',
    className    : 'row',
    events       : {"click .x-edit" : '_editMapping'},
    initialize   : function(){
        this.listenTo(this.model, 'sync', this.render);
    },
    _editMapping : function(){
        var view = new EditView({
            model            : this.model,
            targetCollection : this.model.collection
        });
        AppLayout.modalRegion.show(view);
    }
});