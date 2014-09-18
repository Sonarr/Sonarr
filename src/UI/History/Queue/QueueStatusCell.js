'use strict';

define(
    [
        'Cells/NzbDroneCell'
    ], function (NzbDroneCell) {
        return NzbDroneCell.extend({

            className: 'queue-status-cell',

            render: function () {
                this.$el.empty();

                if (this.cellValue) {
                    var status = this.cellValue.get('status').toLowerCase();
                    var errorMessage = (this.cellValue.get('errorMessage') || '');
                    var icon = 'icon-nd-downloading';
                    var title = 'Downloading';

                    if (status === 'paused') {
                        icon = 'icon-pause';
                        title = 'Paused';
                    }

                    if (status === 'queued') {
                        icon = 'icon-cloud';
                        title = 'Queued';
                    }

                    if (status === 'completed') {
                        icon = 'icon-inbox';
                        title = 'Downloaded';
                    }
                    
                    if (status === 'pending') {
                        icon = 'icon-time';
                        title = 'Pending';
                    }

                    if (status === 'failed') {
                        icon = 'icon-nd-download-failed';
                        title = 'Download failed: check download client for more details';
                    }

                    if (status === 'warning') {
                        icon = 'icon-nd-download-warning';
                        title = 'Download warning: check download client for more details';
                    }

                    if (errorMessage !== '') {
                        if (status === 'completed') {
                            icon = 'icon-nd-import-failed';
                            title = 'Import failed';
                        }
                        else {
                            icon = 'icon-nd-download-failed';
                            title = 'Download failed';
                        }
                        this.$el.html('<i class="{0}"></i>'.format(icon));
                        
                        this.$el.popover({
                            content  : errorMessage.replace(new RegExp('\r\n', 'g'), '<br/>'),
                            html     : true,
                            trigger  : 'hover',
                            title    : title,
                            placement: 'right',
                            container: this.$el
                        });
                    }
                    else {
                        this.$el.html('<i class="{0}" title="{1}"></i>'.format(icon, title));
                    }
                }

                return this;
            }
        });
    });
