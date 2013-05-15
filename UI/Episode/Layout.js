"use strict";
define(['app'], function () {

    NzbDrone.Episode.Layout = Backbone.Marionette.ItemView.extend({
        template: 'Episode/Search/LayoutTemplate'

    });

});