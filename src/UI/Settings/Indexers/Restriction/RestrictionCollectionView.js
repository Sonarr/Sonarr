'use strict';
define([
    'AppLayout',
    'marionette',
    'Settings/Indexers/Restriction/RestrictionItemView',
    'Settings/Indexers/Restriction/RestrictionEditView',
    'Tags/TagHelpers',
    'bootstrap'
], function (AppLayout, Marionette, RestrictionItemView, EditView) {

    return Marionette.CompositeView.extend({
        template : 'Settings/Indexers/Restriction/RestrictionCollectionViewTemplate',
        itemViewContainer : '.x-rows',
        itemView : RestrictionItemView,

        events: {
            'click .x-add'    : '_addMapping'
        },

        _addMapping: function() {
            var model = this.collection.create({ tags: [] });

            var view = new EditView({ model: model, targetCollection: this.collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
