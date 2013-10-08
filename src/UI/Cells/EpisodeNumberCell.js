 'use strict';

define(
    [
        'Cells/NzbDroneCell',
        'Shared/FormatHelpers'
    ], function (NzbDroneCell, FormatHelpers) {
        return NzbDroneCell.extend({

            className: 'episode-number-cell',

            render: function () {

                this.$el.empty();

                var airDateField = this.column.get('airDateUtc') || 'airDateUtc';
                var seasonField = this.column.get('seasonNumber') || 'seasonNumber';
                var episodeField = this.column.get('episodes') || 'episodeNumber';

                if (this.model) {
                    var result = 'Unknown';

                    var airDate = this.model.get(airDateField);
                    var seasonNumber = this.model.get(seasonField);
                    var episodes = this.model.get(episodeField);

                    if (this.cellValue) {
                        if (!seasonNumber) {
                            seasonNumber = this.cellValue.get(seasonField);
                        }

                        if (!episodes) {
                            episodes = this.cellValue.get(episodeField);
                        }

                        if (!airDate) {
                            this.model.get(airDateField);
                        }
                    }

                    if (episodes) {

                        var paddedEpisodes;

                        if (episodes.constructor === Array) {
                            paddedEpisodes = _.map(episodes,function (episodeNumber) {
                                return FormatHelpers.pad(episodeNumber, 2);
                            }).join();
                        }
                        else {
                            paddedEpisodes = FormatHelpers.pad(episodes, 2);
                        }

                        result = '{0}x{1}'.format(seasonNumber, paddedEpisodes);
                    }
                    else if (airDate) {
                        result = new Date(airDate).toLocaleDateString();
                    }

                    this.$el.html(result);
                }
                this.delegateEvents();
                return this;
            }
        });
    });
