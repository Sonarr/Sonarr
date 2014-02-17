'use strict';
define(
    [
        'AppLayout',
        'marionette',
        'Settings/Metadata/MetadataItemView'
    ], function (AppLayout, Marionette, MetadataItemView) {
        return Marionette.CompositeView.extend({
            itemView         : MetadataItemView,
            itemViewContainer: '#x-metadata',
            template         : 'Settings/Metadata/MetadataCollectionViewTemplate'
        });
    });
