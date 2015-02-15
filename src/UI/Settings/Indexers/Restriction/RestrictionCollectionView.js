var AppLayout = require('../../../AppLayout');
var Marionette = require('marionette');
var RestrictionItemView = require('./RestrictionItemView');
var EditView = require('./RestrictionEditView');
require('../../../Tags/TagHelpers');
require('bootstrap');

module.exports = Marionette.CompositeView.extend({
    template          : 'Settings/Indexers/Restriction/RestrictionCollectionViewTemplate',
    itemViewContainer : '.x-rows',
    itemView          : RestrictionItemView,

    events : {
        'click .x-add' : '_addMapping'
    },

    _addMapping : function() {
        var model = this.collection.create({ tags : [] });
        var view = new EditView({
            model            : model,
            targetCollection : this.collection
        });

        AppLayout.modalRegion.show(view);
    }
});