 'use strict';

define(
    [
        'marionette',
        'Cells/NzbDroneCell',
        'reqres'
    ], function (Marionette, NzbDroneCell, reqres) {
        return NzbDroneCell.extend({

            className: 'episode-number-cell',
            template : 'Series/Details/EpisodeNumberCellTemplate',

            render: function () {

                this.$el.empty();
                this.$el.html(this.model.get('episodeNumber'));

                var alternateTitles = [];

                if (reqres.hasHandler(reqres.Requests.GetAlternateNameBySeasonNumber)) {

                    if (this.model.get('sceneSeasonNumber') > 0) {
                        alternateTitles = reqres.request(reqres.Requests.GetAlternateNameBySeasonNumber,
                                                          this.model.get('seriesId'),
                                                          this.model.get('sceneSeasonNumber'));
                    }

                    if (alternateTitles.length === 0) {
                        alternateTitles = reqres.request(reqres.Requests.GetAlternateNameBySeasonNumber,
                            this.model.get('seriesId'),
                            this.model.get('seasonNumber'));
                    }
                }

                if (this.model.get('sceneSeasonNumber') > 0 ||
                    this.model.get('sceneEpisodeNumber') > 0 ||
                   (this.model.has('sceneAbsoluteEpisodeNumber') && this.model.get('sceneAbsoluteEpisodeNumber') > 0) ||
                    alternateTitles.length > 0)
                {
                    this.templateFunction = Marionette.TemplateCache.get(this.template);

                    var json = this.model.toJSON();
                    json.alternateTitles = alternateTitles;

                    var html = this.templateFunction(json);

                    this.$el.popover({
                        content  : html,
                        html     : true,
                        trigger  : 'hover',
                        title    : 'Scene Information',
                        placement: 'right',
                        container: this.$el
                    });
                }

                this.delegateEvents();
                return this;
            }
        });
    });
