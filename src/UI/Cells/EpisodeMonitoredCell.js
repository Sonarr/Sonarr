'use strict';

define(
    [
        'Cells/ToggleCell',
        'Series/SeriesCollection',
        'Shared/Messenger'
    ], function (ToggleCell, SeriesCollection, Messenger) {
        return ToggleCell.extend({

            className: 'toggle-cell episode-monitored',

            _originalOnClick: ToggleCell.prototype._onClick,

            _onClick: function () {
                var series = SeriesCollection.get(this.model.get('seriesId'));

                if (!series.get('monitored')) {

                    Messenger.show({
                        message: 'Unable to change monitored state when series is not monitored',
                        type   : 'error'
                    });

                    return;
                }

                this._originalOnClick.apply(this, arguments);
            }
        });
    });
