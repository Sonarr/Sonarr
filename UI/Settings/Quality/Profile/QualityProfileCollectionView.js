'use strict';

define(['app',
        'marionette',
        'Settings/Quality/Profile/QualityProfileView',
        'Settings/Quality/Profile/EditQualityProfileView',
        'Settings/Quality/Profile/QualityProfileSchemaCollection'],
    function (App, Marionette, QualityProfileView, EditProfileView, ProfileCollection) {

    return Marionette.CompositeView.extend({
        itemView         : QualityProfileView,
        itemViewContainer: '.quality-profiles',
        template         : 'Settings/Quality/Profile/QualityProfileCollectionTemplate',

        events: {
            'click .x-add': '_addProfile'
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
                    App.modalRegion.show(view);
                }
            });
        }
    });
});
