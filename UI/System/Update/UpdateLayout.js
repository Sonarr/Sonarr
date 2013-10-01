'use strict';
define(
    [
        'marionette',
        'backgrid',
        'System/Update/UpdateCollection',
        'System/Update/UpdateCollectionView',
        'Shared/Toolbar/ToolbarLayout',
        'Shared/LoadingView'
    ], function (Marionette, Backgrid, UpdateCollection, UpdateCollectionView, ToolbarLayout, LoadingView) {
        return Marionette.Layout.extend({
            template: 'System/Update/UpdateLayoutTemplate',

            regions: {
                updates: '#x-updates',
                toolbar: '#x-toolbar'
            },

            leftSideButtons: {
                type      : 'default',
                storeState: false,
                items     :
                    [
                        {
                            title  : 'Check for Update',
                            icon   : 'icon-nd-update',
                            command: 'applicationUpdate'
                        }
                    ]
            },

            initialize: function () {
                this.updateCollection = new UpdateCollection();
            },

            onRender: function () {
                this.updates.show(new LoadingView());
                this._showToolbar();

                var self = this;
                var promise = this.updateCollection.fetch();

                promise.done(function (){
                    self.updates.show(new UpdateCollectionView({ collection: self.updateCollection }));
                });
            },

            _showToolbar: function () {
                this.toolbar.show(new ToolbarLayout({
                    left   :
                        [
                            this.leftSideButtons
                        ],
                    context: this
                }));
            }
        });
    });
