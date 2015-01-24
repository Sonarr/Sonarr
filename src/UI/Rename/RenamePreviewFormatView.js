'use strict';
define(
    [
        'vent',
        'marionette',
        'Settings/MediaManagement/Naming/NamingModel'
    ], function (vent, Marionette, NamingModel) {
        return Marionette.ItemView.extend({
            template: 'Rename/RenamePreviewFormatViewTemplate',

            templateHelpers: function () {
                var type = this.model.get('seriesType');

                return {
                    rename : this.naming.get('renameEpisodes'),
                    format : this.naming.get(type + 'EpisodeFormat')
                };
            },

            initialize: function () {
                this.naming = new NamingModel();
                this.naming.fetch();

                this.listenTo(this.naming, 'sync', this.render);
            }
        });
    });
