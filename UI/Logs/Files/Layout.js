'use strict';
define(
    [
        'app',
        'marionette',
        'backgrid',
        'Logs/Files/FilenameCell',
        'Cells/RelativeDateCell',
        'Logs/Files/Collection',
        'Logs/Files/Row',
        'Logs/Files/ContentsView',
        'Logs/Files/ContentsModel'
    ], function (App, Marionette, Backgrid, FilenameCell, RelativeDateCell, LogFileCollection, LogFileRow, ContentsView, ContentsModel) {
        return Marionette.Layout.extend({
            template: 'Logs/Files/LayoutTemplate',

            regions: {
                grid     : '#x-grid',
                contents : '#x-contents'
            },

            columns:
                [
                    {
                        name : 'filename',
                        label: 'Filename',
                        cell : FilenameCell
                    },
                    {
                        name : 'lastWriteTime',
                        label: 'Last Write Time',
                        cell : RelativeDateCell
                    }
                ],

            initialize: function () {
                this.collection = new LogFileCollection();
                this.collectionPromise = this.collection.fetch();

                App.vent.on(App.Commands.ShowLogFile, this._showLogFile, this);
            },

            onShow: function () {
                var self = this;
                this._showTable();

                this.collectionPromise.done(function (){
                    self._showLogFile({ model: _.first(self.collection.models) });
                });
            },

            _showTable: function () {

                this.grid.show(new Backgrid.Grid({
                    row       : LogFileRow,
                    columns   : this.columns,
                    collection: this.collection,
                    className : 'table table-hover'
                }));
            },

            _showLogFile: function (options) {
                var self = this;
                var filename = options.model.get('filename');

                $.ajax({
                    url: '/log/' + filename,
                    success: function (data) {
                        var model = new ContentsModel({
                            filename: filename,
                            contents: data
                        });

                        self.contents.show(new ContentsView({ model: model }));
                    }
                });
            }
        });
    });
