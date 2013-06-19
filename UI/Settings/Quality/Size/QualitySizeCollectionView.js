'use strict';

define(['marionette', 'Settings/Quality/Size/QualitySizeView'], function (Marionette, QualitySizeView) {
    return Marionette.CompositeView.extend({
        itemView         : QualitySizeView,
        itemViewContainer: '#quality-sizes-container',
        template         : 'Settings/Quality/Size/QualitySizeCollectionTemplate'
    });
});
