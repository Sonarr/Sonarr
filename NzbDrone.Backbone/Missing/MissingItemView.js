'use strict';

define([
        'app',
        'Missing/MissingCollection'

], function () {
    NzbDrone.Missing.MissingItemView = Backbone.Marionette.ItemView.extend({
        template: 'Missing/MissingItemTemplate',
        tagName: 'tr',

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        }
    })
})