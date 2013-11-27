'use strict';
define(
    [
        'underscore',
        'vent',
        'marionette',
        'Rename/RenamePreviewCollection',
        'Rename/RenamePreviewCollectionView',
        'Rename/RenamePreviewEmptyCollectionView',
        'Shared/LoadingView',
        'Commands/CommandController'
    ], function (_, vent, Marionette, RenamePreviewCollection, RenamePreviewCollectionView, EmptyCollectionView, LoadingView, CommandController) {

        return Marionette.Layout.extend({
            template: 'Rename/RenamePreviewLayoutTemplate',

            regions: {
                renamePreviews : '#rename-previews'
            },

            ui: {
                pathInfo     : '.x-path-info',
                renameAll    : '.x-rename-all',
                checkboxIcon: '.x-rename-all-button i'
            },

            events: {
                'click .x-organize'   : '_organizeFiles',
                'change .x-rename-all': '_toggleAll'
            },

            initialize: function (options) {
                this.model = options.series;
                this.seasonNumber = options.seasonNumber;

                var viewOptions = {};
                viewOptions.seriesId = this.model.id;
                viewOptions.seasonNumber = this.seasonNumber;

                this.collection = new RenamePreviewCollection(viewOptions);
                this.listenTo(this.collection, 'sync', this._showPreviews);
                this.listenTo(this.collection, 'rename:select', this._itemRenameChanged);

                this.collection.fetch();
            },

            onRender: function() {
                this.renamePreviews.show(new LoadingView());
            },

            _showPreviews: function () {
                if (this.collection.length === 0) {
                    this.ui.pathInfo.hide();
                    this.renamePreviews.show(new EmptyCollectionView());
                    return;
                }

                this.ui.pathInfo.show();
                this.collection.invoke('set', { rename: true });
                this.renamePreviews.show(new RenamePreviewCollectionView({ collection: this.collection }));
            },

            _organizeFiles: function () {
                if (this.collection.length === 0) {
                    vent.trigger(vent.Commands.CloseModalCommand);
                }

                var files = _.map(this.collection.where({ rename: true }), function (model) {
                    return model.get('episodeFileId');
                });

                if (files.length === 0) {
                    vent.trigger(vent.Commands.CloseModalCommand);
                    return;
                }

                if (this.seasonNumber) {
                    CommandController.Execute('renameFiles', {
                        name        : 'renameFiles',
                        seriesId    : this.model.id,
                        seasonNumber: this.seasonNumber,
                        files       : files
                    });
                }

                else {
                    CommandController.Execute('renameFiles', {
                        name        : 'renameFiles',
                        seriesId    : this.model.id,
                        seasonNumber: -1,
                        files       : files
                    });
                }

                vent.trigger(vent.Commands.CloseModalCommand);
            },

            _setCheckedState: function (checked) {
                if (checked) {
                    this.ui.checkboxIcon.addClass('icon-check');
                    this.ui.checkboxIcon.removeClass('icon-check-empty');
                }

                else {
                    this.ui.checkboxIcon.addClass('icon-check-empty');
                    this.ui.checkboxIcon.removeClass('icon-check');
                }
            },

            _toggleAll: function () {
                var checked = this.ui.renameAll.prop('checked');
                this._setCheckedState(checked);

                this.collection.each(function (model) {
                    model.trigger('rename:select', model, checked);
                });
            },

            _itemRenameChanged: function (model, checked) {
                var allChecked = this.collection.all(function (m) {
                    return m.get('rename');
                });

                if (!checked || allChecked) {
                    this._setCheckedState(checked);
                }
            }
        });
    });
