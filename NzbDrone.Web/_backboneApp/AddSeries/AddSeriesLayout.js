/// <reference path="../app.js" />
/// <reference path="AddNewSeries/AddNewSeriesView.js" />
/// <reference path="RootDir/RootDirView.js" />

NzbDrone.AddSeries.AddSeriesLayout = Backbone.Marionette.Layout.extend({
    template: "AddSeries/addSeriesLayoutTemplate",

    regions: {
        addNew: "#add-new",
        importExisting: "#import-existing",
        rootFolders: "#root-folders"
    },

    onRender: function () {
        this.$('#myTab a').click(function (e) {
            e.preventDefault();
            $(this).tab('show');
        });

        this.addNew.show(new NzbDrone.AddSeries.AddNewSeriesView());
        //this.importExisting.show(new NzbDrone.ImportExistingView());
        this.rootFolders.show(new NzbDrone.AddSeries.RootDirView());
    },

});