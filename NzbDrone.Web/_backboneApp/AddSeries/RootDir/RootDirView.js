/// <reference path="../../app.js" />
/// <reference path="RootDirModel.js" />
/// <reference path="RootDirCollection.js" />

NzbDrone.AddSeries.RootDirItemView = Backbone.Marionette.ItemView.extend({

    template: "AddSeries/RootDir/RootDirItemTemplate",
    className: 'row',

    onRender: function () {
        NzbDrone.ModelBinder.bind(this.model, this.el);
    }

});

NzbDrone.AddSeries.RootDirListView = Backbone.Marionette.CollectionView.extend({
    className: 'result',
    itemView: NzbDrone.AddSeries.RootDirItemView,
});

NzbDrone.AddSeries.RootDirView = Backbone.Marionette.Layout.extend({
    template: "AddSeries/RootDir/RootDirTemplate",

    ui: {
        pathInput: '.path input'
    },

    regions: {
        currentDirs: "#current-dirs",
    },

    collection: new NzbDrone.AddSeries.RootDirCollection(),

    onRender: function () {
        var self = this;

/*
        this.ui.seriesSearch
            .data('timeout', null)
            .keyup(function () {
                clearTimeout(self.$el.data('timeout'));
                self.$el.data('timeout', setTimeout(self.search, 500, self));
            });
*/

        this.currentDirs.show(new NzbDrone.AddSeries.RootDirListView({ collection: this.collection }));
    },

    search: function (context) {

        var term = context.ui.seriesSearch.val();

        if (term == "") {
            context.collection.reset();
        } else {
            console.log(term);
            context.collection.fetch({ data: $.param({ term: term }) });
        }


    },
});