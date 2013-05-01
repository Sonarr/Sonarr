'use strict';

define(['app'], function () {

    NzbDrone.Series.Index.EmptyView = Backbone.Marionette.CompositeView.extend({
        template: 'Series/Index/EmptyTemplate'
    });
});
