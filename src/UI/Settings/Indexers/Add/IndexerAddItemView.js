'use strict';

define([
    'underscore',
    'jquery',
    'AppLayout',
    'marionette',
    'Settings/Indexers/Edit/IndexerEditView'
], function (_, $, AppLayout, Marionette, EditView) {

    return Marionette.ItemView.extend({
        template  : 'Settings/Indexers/Add/IndexerAddItemViewTemplate',
        tagName   : 'li',
        className : 'add-thingy-item',

        events: {
            'click .x-preset': '_addPreset',
            'click'          : '_add'
        },

        initialize: function (options) {
            this.targetCollection = options.targetCollection;
        },

        _addPreset: function (e) {
        
            var presetName = $(e.target).closest('.x-preset').attr('data-id');

            var presetData = _.where(this.model.get('presets'), {name: presetName})[0];
            
            this.model.set(presetData);
            
            this.model.set({
                id         : undefined,
                enable     : true
            });

            var editView = new EditView({ model: this.model, targetCollection: this.targetCollection });
            AppLayout.modalRegion.show(editView);
        },

        _add: function (e) {
            if ($(e.target).closest('.btn,.btn-group').length !== 0) {
                return;
            }

            this.model.set({
                id         : undefined,
                enable     : true
            });

            var editView = new EditView({ model: this.model, targetCollection: this.targetCollection });
            AppLayout.modalRegion.show(editView);
        }
    });
});
