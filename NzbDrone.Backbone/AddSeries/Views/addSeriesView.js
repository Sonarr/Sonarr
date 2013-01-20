NzbDrone.AddSeriesView = Backbone.Marionette.ItemView.extend({
    template: "#add-series",

    events: {
        'click #add-new': 'addNew',
        'click #add-existing': 'addExisting'
    },

    addNew: function () {

        NzbDrone.Router.navigate(NzbDrone.Routes.Series.AddNew, { trigger: true });
    },

    addExisting: function () {
        NzbDrone.Router.navigate(NzbDrone.Routes.Series.AddExisting, { trigger: true });
    }
});

NzbDrone.AddNewSeriesView = Backbone.Marionette.ItemView.extend({
    template: "#add-new-series",

    ui: {
        seriesSearch: '#series-search'
    },

    onRender: function () {


        console.log('binding auto complete')
        var self = this;

        this.ui.seriesSearch
            .autocomplete({
                source: "http://localhost:1232/api/series/lookup",
                minLength: 1,
                delay: 500,
                select: function (event, ui) {
                    $(this).val(ui.item.Title);
                    $(this).siblings('.seriesId').val(ui.item.Id);
                    return false;
                },
                open: function (event, ui) {
                    $('.ui-autocomplete').addClass('seriesLookupResults');
                },
                close: function (event, ui) {
                    $('.ui-autocomplete').removeClass('seriesLookupResults');
                }

            })
            .data("autocomplete")._renderItem = function (ul, item) {

                return $("<li></li>")
               .data("item.autocomplete", item)
               .append("<a>" + item.SeriesName + "<img src='../../Content/Images/thetvdb.png' class='tvDbLink' title='Click to see series details from TheTVDB' rel='" + item.Url + "' /></a>")
               .appendTo(ul);
            };
    },
});

NzbDrone.AddExistingSeriesView = Backbone.Marionette.ItemView.extend({
    template: "#add-existing-series",

    events: {
        'click #single': 'single',
        'click #multiple': 'multiple'
    },

    single: function () {
        NzbDrone.Router.navigate(NzbDrone.Routes.Series.AddExistingSingle, { trigger: true });
    },

    multiple: function () {
        NzbDrone.Router.navigate(NzbDrone.Routes.Series.AddExistingMultiple, { trigger: true });
    }
});

NzbDrone.AddExistingSeriesSingleView = Backbone.Marionette.ItemView.extend({
    template: "#add-existing-series-single"
});

NzbDrone.AddExistingSeriesMultipleView = Backbone.Marionette.ItemView.extend({
    template: "#add-existing-series-multiple"
})