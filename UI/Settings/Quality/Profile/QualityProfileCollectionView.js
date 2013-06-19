'use strict';

define(['marionette', 'Settings/Quality/Profile/QualityProfileView'], function (Marionette, QualityProfileView) {
    return Marionette.CompositeView.extend({
        itemView         : QualityProfileView,
        itemViewContainer: 'tbody',
        template         : 'Settings/Quality/Profile/QualityProfileCollectionTemplate'
    });
});
