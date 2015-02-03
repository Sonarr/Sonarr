var AppLayout = require('../../AppLayout');
var Marionette = require('marionette');
var MetadataItemView = require('./MetadataItemView');

module.exports = Marionette.CompositeView.extend({
    itemView          : MetadataItemView,
    itemViewContainer : '#x-metadata',
    template          : 'Settings/Metadata/MetadataCollectionViewTemplate'
});