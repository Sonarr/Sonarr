/// <reference path="../app.js" />
NzbDrone.AddSeries.SearchResultModel = Backbone.Model.extend({
    mutators: {
        seriesYear: function () {
            var date = Date.utc.create(this.get('firstAired'));
            return date.format('{yyyy}');
        }
    }
});
