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

                    switch (this.cellValue.get('eventType')) {
                        case 'grabbed':
                            icon = 'icon-nd-downloading';
                            toolTip = 'Episode grabbed from {0} and sent to download client'.format(this.cellValue.get('data').indexer);
                            break;
                        case 'seriesFolderImported':
                            icon = 'icon-hdd';
                            toolTip = 'Existing episode file added to library';
                            break;
                        case 'downloadFolderImported':
                            icon = 'icon-nd-imported';
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
