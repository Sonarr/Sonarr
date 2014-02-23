'use strict';

define(
    [
        'marionette',
        'Settings/MediaManagement/Naming/NamingView',
        'Settings/MediaManagement/Sorting/SortingView',
        'Settings/MediaManagement/FileManagement/FileManagementView',
        'Settings/MediaManagement/Permissions/PermissionsView'
    ], function (Marionette, NamingView, SortingView, FileManagementView, PermissionsView) {
        return Marionette.Layout.extend({
            template: 'Settings/MediaManagement/MediaManagementLayoutTemplate',

            regions: {
                episodeNaming  : '#episode-naming',
                sorting        : '#sorting',
                fileManagement : '#file-management',
                permissions    : '#permissions'
            },

            initialize: function (options) {
                this.settings = options.settings;
                this.namingSettings = options.namingSettings;
            },

            onShow: function () {
                this.episodeNaming.show(new NamingView({ model: this.namingSettings }));
                this.sorting.show(new SortingView({ model: this.settings }));
                this.fileManagement.show(new FileManagementView({ model: this.settings }));
                this.permissions.show(new PermissionsView({ model: this.settings }));
            }
        });
    });

