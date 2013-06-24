'use strict';

define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'history-event-type-cell',

            render: function () {
                this.$el.empty();

                if (this.cellValue) {

                    var icon;
                    var toolTip;

                    switch (this.cellValue) {
                        case 'grabbed':
                            icon = 'icon-cloud-download';
                            toolTip = 'Episode grabbed from indexer and sent to download client';
                            break;
                        case 'seriesFolderImported':
                            icon = 'icon-hdd';
                            toolTip = 'Existing episode file added to library';
                            break;
                        case 'downloadFolderImported':
                            icon = 'icon-download-alt';
                            toolTip = 'Episode downloaded successfully and picked up from download client';
                            break;
                        default :
                            icon = 'icon-question';
                            toolTip = 'unknown event';

                    }

                    this.$el.html('<i class="{0}" title="{1}"/>'.format(icon, toolTip));
                }

                return this;
            }
        });
    });
