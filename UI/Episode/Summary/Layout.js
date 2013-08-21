'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Cells/FileSizeCell',
        'Cells/QualityCell',
        'Episode/Summary/NoFileView'
    ], function (Marionette, Backgrid, FileSizeCell, QualityCell, NoFileView) {

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

            onShow: function () {
                if (this.model.get('episodeFile')) {
                    this.activity.show(new Backgrid.Grid({
                        collection: new Backbone.Collection(this.model.get('episodeFile')),
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
