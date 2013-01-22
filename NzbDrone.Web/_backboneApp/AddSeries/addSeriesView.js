NzbDrone.AddNewSeriesView = Backbone.Marionette.ItemView.extend({
    template: "AddSeries/addNewSeriesTemplate",

    ui: {
        seriesSearch: '.search input'
    },

    onRender: function () {

        console.log('binding auto complete');
        var self = this;

        this.ui.seriesSearch
            .data('timeout', null)
            .keyup(function () {
                clearTimeout(self.$el.data('timeout'));
                self.$el.data('timeout', setTimeout(self.search, 500, self));
            });
    },

    search: function (context) {
        console.log(context.ui.seriesSearch.val());
    },
});

NzbDrone.ImportExistingView = Backbone.Marionette.ItemView.extend({
    template: "AddSeries/ImportExistingTemplate",

});

NzbDrone.RootFoldersView = Backbone.Marionette.ItemView.extend({
    template: "AddSeries/RootFoldersTemplate",

});

NzbDrone.AddSeriesLayout = Backbone.Marionette.Layout.extend({
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

        this.addNew.show(new NzbDrone.AddNewSeriesView());
        this.importExisting.show(new NzbDrone.ImportExistingView());
        this.rootFolders.show(new NzbDrone.RootFoldersView());
    },

});