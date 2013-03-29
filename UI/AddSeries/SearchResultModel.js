define(['app', 'AddSeries/RootFolders/RootFolderCollection', 'Quality/QualityProfileCollection'],
    function (app, rootFolderCollection, qualityProfileCollection) {

    NzbDrone.AddSeries.SearchResultModel = Backbone.Model.extend({
        mutators: {
            seriesYear: function () {
                var date = Date.utc.create(this.get('firstAired')).format('({yyyy})');

                //don't append year, if the series name already has the name appended.
                if (this.get('title').endsWith(date)) {
                    return "";
                } else {
                    return date;
                }
            }
        },

        defaults: {
            qualityProfiles: qualityProfileCollection,
            rootFolders: rootFolderCollection
        }

    });

});
