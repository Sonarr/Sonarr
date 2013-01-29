/// <reference path="../../app.js" />
/// <reference path="RootDirModel.js" />
/// <reference path="RootDirCollection.js" />

NzbDrone.AddSeries.RootDirItemView = Backbone.Marionette.ItemView.extend({

    template: "AddSeries/RootDir/RootDirItemTemplate",
    tagName: 'tr',

    events: {
        'click #remove-dir': 'removeDir',
    },

    onRender: function () {
        NzbDrone.ModelBinder.bind(this.model, this.el);
    },

    removeDir: function () {
        this.model.destroy({ wait: true });
        this.model.collection.remove(this.model);
    },

});

NzbDrone.AddSeries.RootDirListView = Backbone.Marionette.CollectionView.extend({
    itemView: NzbDrone.AddSeries.RootDirItemView,

    tagName: 'table',
    className: 'table table-hover',
});

NzbDrone.AddSeries.RootDirView = Backbone.Marionette.Layout.extend({
    template: "AddSeries/RootDir/RootDirTemplate",
    route: "series/add/rootdir",

    ui: {
        pathInput: '.path input'
    },

    regions: {
        currentDirs: "#current-dirs",
    },

    events: {
        'click #add-dir': 'addDir',
    },

    shortcuts: {
        'enter': 'addDir'
    },


    collection: new NzbDrone.AddSeries.RootDirCollection(),

    onRender: function () {
        var self = this;

        //NzbDrone.Router.navigate(this.route, { trigger: true });

        /*
                this.ui.seriesSearch
                    .data('timeout', null)
                    .keyup(function () {
                        clearTimeout(self.$el.data('timeout'));
                        self.$el.data('timeout', setTimeout(self.search, 500, self));
                    });
        */

        this.currentDirs.show(new NzbDrone.AddSeries.RootDirListView({ collection: this.collection }));

        this.collection.fetch();
    },


    addDir: function () {
        var newDir = new NzbDrone.AddSeries.RootDirModel(
        {
            Path: this.ui.pathInput.val()
        });

        var self = this;

        this.collection.create(newDir, {
            wait: true, success: function () {
                self.collection.fetch();
            }
        });
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