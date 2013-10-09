'use strict';
define(
    [
        'reqres',
        'marionette',
        'backgrid',
        'Series/EpisodeFileModel',
        'Series/EpisodeFileCollection',
        'Cells/FileSizeCell',
        'Cells/QualityCell',
        'Episode/Summary/NoFileView',
        'Shared/LoadingView'
    ], function (reqres, Marionette, Backgrid, EpisodeFileModel, EpisodeFileCollection, FileSizeCell, QualityCell, NoFileView, LoadingView) {

        return Marionette.Layout.extend({
            template: 'Episode/Summary/EpisodeSummaryLayoutTemplate',

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
                    var episodeFileId = this.model.get('episodeFileId');

                    if (reqres.hasHandler(reqres.Requests.GetEpisodeFileById)) {
                        var episodeFile = reqres.request(reqres.Requests.GetEpisodeFileById, episodeFileId);
                        var episodeFileCollection = new EpisodeFileCollection(episodeFile, { seriesId: this.model.get('seriesId') });
                        this._showTable(episodeFileCollection);
                    }

                    else {
                        this.activity.show(new LoadingView());

                        var self = this;
                        var newEpisodeFile = new EpisodeFileModel({ id: episodeFileId });
                        var newEpisodeFileCollection = new EpisodeFileCollection(newEpisodeFile, { seriesId: this.model.get('seriesId') });
                        var promise = newEpisodeFile.fetch();

                        promise.done(function () {
                           self._showTable(newEpisodeFileCollection);
                        });
                    }
                }

                else {
                    this.activity.show(new NoFileView());
                }
            },

            _showTable: function (episodeFileCollection) {
                this.activity.show(new Backgrid.Grid({
                    collection: episodeFileCollection,
                    columns   : this.columns,
                    className : 'table table-bordered'
                }));
            }
        });
    });
