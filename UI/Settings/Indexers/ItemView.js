"use strict";

define([
    'app',
    'Settings/Indexers/Collection'

], function () {

    NzbDrone.Settings.Indexers.ItemView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Indexers/ItemTemplate',
        tagName   : 'li'
    });
});
