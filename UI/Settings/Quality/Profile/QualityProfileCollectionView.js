'use strict';

define(['marionette', 'Settings/Quality/Profile/QualityProfileView'], function (Marionette, QualityProfileView) {
    return Marionette.CompositeView.extend({
        itemView         : QualityProfileView,
        itemViewContainer: '.quality-profiles',
        template         : 'Settings/Quality/Profile/QualityProfileCollectionTemplate'
    });
});
