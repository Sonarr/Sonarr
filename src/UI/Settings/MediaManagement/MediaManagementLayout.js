"use strict";

define(
    [
        'marionette',
        'Settings/MediaManagement/Naming/View',
        'Settings/MediaManagement/Sorting/View',
        'Settings/MediaManagement/FileManagement/FileManagementView'
    ], function (Marionette, NamingView, SortingView, FileManagementView) {
        return Marionette.Layout.extend({
            template: 'Settings/MediaManagement/MediaManagementLayoutTemplate',

            regions: {
                episodeNaming  : '#episode-naming',
                sorting        : '#sorting',
                fileManagement : '#file-management'
            },

            initialize: function (options) {
                this.settings = options.settings;
                this.namingSettings = options.namingSettings;
            },

            onShow: function () {
                this.episodeNaming.show(new NamingView({ model: this.namingSettings }));
                this.sorting.show(new SortingView({ model: this.settings }));
                this.fileManagement.show(new FileManagementView({ model: this.settings }));
            }
        });
    });

