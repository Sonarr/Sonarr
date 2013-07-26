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
            'click .x-add-card': '_addProfile'
        },

        onRender: function () {
            this.listenTo(this.collection, 'add', this.render);

            this.templateFunction = Marionette.TemplateCache.get('Settings/Quality/Profile/AddCardTemplate');
            var html = this.templateFunction();

            this.$itemViewContainer.append(html);
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
