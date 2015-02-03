var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var RemotePathMappingItemView = require('./RemotePathMappingItemView');
var EditView = require('./RemotePathMappingEditView');
var RemotePathMappingModel = require('./RemotePathMappingModel');
require('bootstrap');

module.exports = Marionette.CompositeView.extend({
    template          : 'Settings/DownloadClient/RemotePathMapping/RemotePathMappingCollectionViewTemplate',
    itemViewContainer : '.x-rows',
    itemView          : RemotePathMappingItemView,
    events            : {"click .x-add" : '_addMapping'},
    _addMapping       : function(){
        var model = new RemotePathMappingModel();
        model.collection = this.collection;
        var view = new EditView({
            model            : model,
            targetCollection : this.collection
        });
        AppLayout.modalRegion.show(view);
    }
});