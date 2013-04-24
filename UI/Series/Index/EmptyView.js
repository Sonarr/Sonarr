'use strict';

define(['app'], function (app) {

    NzbDrone.Series.Index.EmptyView = Backbone.Marionette.CompositeView.extend({
        template: 'Series/Index/EmptyTemplate'
    });
});