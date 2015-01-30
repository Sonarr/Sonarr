'use strict';
define([
    'AppLayout',
    'marionette',
    'Settings/DownloadClient/RemotePathMapping/RemotePathMappingItemView',
    'Settings/DownloadClient/RemotePathMapping/RemotePathMappingEditView',
    'Settings/DownloadClient/RemotePathMapping/RemotePathMappingModel',
    'bootstrap'
], function (AppLayout, Marionette, RemotePathMappingItemView, EditView, RemotePathMappingModel) {

    return Marionette.CompositeView.extend({
        template : 'Settings/DownloadClient/RemotePathMapping/RemotePathMappingCollectionViewTemplate',
        itemViewContainer : '.x-rows',
        itemView : RemotePathMappingItemView,

        events: {
            'click .x-add'    : '_addMapping'
        },

        _addMapping: function() {
            var model = new RemotePathMappingModel();
            model.collection = this.collection;

            var view = new EditView({ model: model, targetCollection: this.collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
