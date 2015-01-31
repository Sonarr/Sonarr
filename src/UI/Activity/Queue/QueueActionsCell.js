'use strict';

define(
    [
        'jquery',
        'marionette',
        'Cells/NzbDroneCell'
    ], function ($, Marionette, NzbDroneCell) {
        return NzbDroneCell.extend({

            className : 'queue-actions-cell',

            events: {
                'click .x-remove' : '_remove',
                'click .x-import' : '_import',
                'click .x-grab'   : '_grab'
            },

            ui: {
                import : '.x-import',
                grab   : '.x-grab'
            },

            render: function () {
                this.$el.empty();

                if (this.cellValue) {
                    var status = this.cellValue.get('status').toLowerCase();
                    var trackedDownloadStatus = this.cellValue.has('trackedDownloadStatus') ? this.cellValue.get('trackedDownloadStatus').toLowerCase() : 'ok';
                    var icon = '';
                    var title = '';

                    if (status === 'completed' && trackedDownloadStatus === 'warning') {
                        icon = 'icon-inbox x-import';
                        title = 'Force import';
                    }

                    if (status === 'pending') {
                        icon = 'icon-download-alt x-grab';
                        title = 'Add to download queue (Override Delay Profile)';
                    }

                    //TODO: Show manual import if its completed or option to blacklist
                    //if (trackedDownloadStatus === 'error') {
                    //    if (status === 'completed') {
                    //        icon = 'icon-nd-import-failed';
                    //        title = 'Import failed: ' + itemTitle;
                    //    }
                    //TODO: What do we show when waiting for retry to take place?

                    //    else {
                    //        icon = 'icon-nd-download-failed';
                    //        title = 'Download failed';
                    //    }
                    //}

                    this.$el.html('<i class="{0}" title="{1}"></i>'.format(icon, title) +
                    '<i class="icon-nd-delete x-remove" title="Remove from Download Client"></i>');
                }

                return this;
            },

            _remove : function () {
                this.model.destroy();
            },

            _import : function () {
                var self = this;

                var promise = $.ajax({
                    url: window.NzbDrone.ApiRoot + '/queue/import',
                    type: 'POST',
                    data: JSON.stringify(this.model.toJSON())
                });

                $(this.ui.import).spinForPromise(promise);

                promise.success(function () {
                    //find models that have the same series id and episode ids and remove them
                    self.model.trigger('destroy', self.model);
                });
            },

            _grab : function () {
                var self = this;

                var promise = $.ajax({
                    url: window.NzbDrone.ApiRoot + '/queue/grab',
                    type: 'POST',
                    data: JSON.stringify(this.model.toJSON())
                });

                $(this.ui.grab).spinForPromise(promise);

                promise.success(function () {
                    //find models that have the same series id and episode ids and remove them
                    self.model.trigger('destroy', self.model);
                });
            }
        });
    });
