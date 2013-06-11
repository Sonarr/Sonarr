"use strict";

define(['app', 'Cells/NzbDroneCell' ], function () {
    NzbDrone.History.EventTypeCell = NzbDrone.Cells.NzbDroneCell.extend({

        className: 'history-event-type-cell',

        render: function () {
            this.$el.empty();

            if (this.cellValue) {

                var icon = 'icon-question';
                var toolTip = 'unknow event';

                switch (this.cellValue) {
                    case 'grabbed':
                        icon = 'icon-cloud-download';
                        toolTip = 'Episode grabbed from indexer and sent to download client';
                        break;
                    case 'seriesFolderImported':
                        icon = 'icon-hdd';
                        toolTip = 'Existing episode file added to library';
                        break;
                    case 'DownloadFolderImported':
                        icon = 'icon-download-alt';
                        toolTip = 'Episode downloaded succesfully and picked up from download client';
                        break;
                }

                this.$el.html('<i class="{0}" title="{1}"/>'.format(icon, toolTip));
            }

            return this;
        }
    });
});
