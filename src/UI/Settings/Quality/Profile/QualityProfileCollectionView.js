'use strict';

define(['AppLayout',
        'marionette',
        'Settings/Quality/Profile/QualityProfileView',
        'Settings/Quality/Profile/Edit/EditQualityProfileLayout',
        'Settings/Quality/Profile/QualityProfileSchemaCollection',
        'underscore'
], function (AppLayout, Marionette, QualityProfileView, EditProfileView, ProfileCollection, _) {

    return Marionette.CompositeView.extend({
        itemView         : QualityProfileView,
        itemViewContainer: '.quality-profiles',
        template         : 'Settings/Quality/Profile/QualityProfileCollectionTemplate',

        ui: {
            'addCard': '.x-add-card'
        },

        events: {
            'click .x-add-card': '_addProfile'
        },

        appendHtml: function(collectionView, itemView, index){
            collectionView.ui.addCard.parent('li').before(itemView.el);
        },

        _addProfile: function () {
            var self = this;
            var schemaCollection = new ProfileCollection();
            schemaCollection.fetch({
                success: function (collection) {
                    var model = _.first(collection.models);
                    model.set('id', undefined);
                    model.set('name', '');
                    model.collection = self.collection;

                    var view = new EditProfileView({ model: model, profileCollection: self.collection});
                    AppLayout.modalRegion.show(view);
                }
            });
        }
    });
});
