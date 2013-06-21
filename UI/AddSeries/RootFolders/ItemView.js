"use strict";

define(
    [
        'marionette'
    ], function (Marionette) {

        return Marionette.ItemView.extend({

            template: 'AddSeries/RootFolders/RootFolderItemTemplate',
            tagName : 'tr',

            events: {
                'click .x-remove': 'removeFolder',
                'click .x-folder': 'folderSelected'
            },

            removeFolder: function () {
                this.model.destroy({ wait: true });
                this.model.collection.remove(this.model);
            },

            folderSelected: function () {
                this.trigger('folderSelected', this.model);
            }
        });
    });
