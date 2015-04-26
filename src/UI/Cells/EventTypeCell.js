var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'history-event-type-cell',

    render : function() {
        this.$el.empty();

        if (this.cellValue) {
            var icon;
            var toolTip;

            switch (this.cellValue.get('eventType')) {
                case 'grabbed':
                    icon = 'icon-sonarr-downloading';
                    toolTip = 'Episode grabbed from {0} and sent to download client'.format(this.cellValue.get('data').indexer);
                    break;
                case 'seriesFolderImported':
                    icon = 'icon-sonarr-hdd';
                    toolTip = 'Existing episode file added to library';
                    break;
                case 'downloadFolderImported':
                    icon = 'icon-sonarr-imported';
                    toolTip = 'Episode downloaded successfully and picked up from download client';
                    break;
                case 'downloadFailed':
                    icon = 'icon-sonarr-download-failed';
                    toolTip = 'Episode download failed';
                    break;
                case 'episodeFileDeleted':
                    icon = 'icon-sonarr-deleted';
                    toolTip = 'Episode file deleted';
                    break;
                default:
                    icon = 'icon-sonarr-unknown';
                    toolTip = 'unknown event';
            }

            this.$el.html('<i class="{0}" title="{1}" data-placement="right"/>'.format(icon, toolTip));
        }

        return this;
    }
});