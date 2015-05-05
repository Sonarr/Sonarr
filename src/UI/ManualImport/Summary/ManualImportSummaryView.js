var _ = require('underscore');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template  : 'ManualImport/Summary/ManualImportSummaryViewTemplate',

    initialize : function (options) {
        var episodes = _.map(options.episodes, function (episode) {
                return episode.toJSON();
            });

        this.templateHelpers = {
            file     : options.file,
            series   : options.series,
            season   : options.season,
            episodes : episodes,
            quality  : options.quality
        };
    }
});