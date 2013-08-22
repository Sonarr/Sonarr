'use strict';
define(
    [
        'app',
        'marionette',
        'backgrid',
        'Cells/FileSizeCell',
        'Cells/QualityCell',
        'Episode/Summary/NoFileView'
    ], function (App, Marionette, Backgrid, FileSizeCell, QualityCell, NoFileView) {

        return Marionette.Layout.extend({
            template: 'Episode/Summary/LayoutTemplate',

            regions: {
                overview: '.episode-overview',
                activity: '.episode-file-info'
            },

            columns:
                [
                    {
                        name    : 'path',
                        label   : 'Path',
                        cell    : 'string',
                        sortable: false
                    },
                    {
                        name    : 'size',
                        label   : 'Size',
                        cell    : FileSizeCell,
                        sortable: false
                    },
                    {
                        name    : 'quality',
                        label   : 'Quality',
                        cell    : QualityCell,
                        sortable: false,
                        editable: true
                    }
                ],

            templateHelpers: {},

            initialize: function (options) {
                if (!this.model.series) {
                    this.templateHelpers.series = options.series.toJSON();
                }
            },

            onShow: function () {
                if (this.model.get('hasFile')) {
                    var episodeFile = App.request(App.Reqres.GetEpisodeFileById, this.model.get('episodeFileId'));

                    this.activity.show(new Backgrid.Grid({
                        collection: new Backbone.Collection(episodeFile),
                        columns   : this.columns,
                        className : 'table table-bordered'
                    }));
                }

                else {
                    this.activity.show(new NoFileView());
                }
            }
        });
    });
