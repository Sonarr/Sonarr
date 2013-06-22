'use strict';
define(['app'], function () {

    NzbDrone.Episode.Summary.View = Backbone.Marionette.ItemView.extend({
        template: 'Episode/Summary/ViewTemplate'
    });

});
