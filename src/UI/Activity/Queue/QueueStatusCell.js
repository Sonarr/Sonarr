var Marionette = require('marionette');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var moment = require('moment');
var UiSettingsModel = require('../../Shared/UiSettingsModel');
var FormatHelpers = require('../../Shared/FormatHelpers');

module.exports = NzbDroneCell.extend({
    className : 'queue-status-cell',
    template  : 'Activity/Queue/QueueStatusCellTemplate',

    render : function() {
        this.$el.empty();

        if (this.cellValue) {
            var status = this.cellValue.get('status').toLowerCase();
            var trackedDownloadStatus = this.cellValue.has('trackedDownloadStatus') ? this.cellValue.get('trackedDownloadStatus').toLowerCase() : 'ok';
            var icon = 'icon-sonarr-downloading';
            var title = 'Downloading';
            var itemTitle = this.cellValue.get('title');
            var content = itemTitle;

            if (status === 'paused') {
                icon = 'icon-sonarr-paused';
                title = 'Paused';
            }

            if (status === 'queued') {
                icon = 'icon-sonarr-queued';
                title = 'Queued';
            }

            if (status === 'completed') {
                icon = 'icon-sonarr-downloaded';
                title = 'Downloaded';
            }

            if (status === 'delay') {
                icon = 'icon-sonarr-pending';
                var ect = this.cellValue.get('estimatedCompletionTime');
                var time = '{0} at {1}'.format(FormatHelpers.relativeDate(ect), moment(ect).format(UiSettingsModel.time(true, false)));
                title = 'Download delayed till {0}'.format(time);
            }

            if (status === 'downloadclientunavailable') {
                icon = 'icon-sonarr-client-unavailable';
                title = 'Download pending, download client is unavailable';
            }

            if (status === 'failed') {
                icon = 'icon-sonarr-download-failed';
                title = 'Download failed';
            }

            if (status === 'warning') {
                icon = 'icon-sonarr-download-warning';
                title = 'Download warning: check download client for more details';
            }

            if (trackedDownloadStatus === 'warning') {
                icon += ' icon-sonarr-warning';

                this.templateFunction = Marionette.TemplateCache.get(this.template);
                content = this.templateFunction(this.cellValue.toJSON());
            }

            if (trackedDownloadStatus === 'error') {
                if (status === 'completed') {
                    icon = 'icon-sonarr-import-failed';
                    title = 'Import failed: ' + itemTitle;
                } else {
                    icon = 'icon-sonarr-download-failed';
                    title = 'Download failed';
                }

                this.templateFunction = Marionette.TemplateCache.get(this.template);
                content = this.templateFunction(this.cellValue.toJSON());
            }

            this.$el.html('<i class="{0}"></i>'.format(icon));
            this.$el.popover({
                content   : content,
                html      : true,
                trigger   : 'hover',
                title     : title,
                placement : 'right',
                container : this.$el
            });
        }
        return this;
    }
});