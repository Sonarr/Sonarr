(function (nzbDrone) {

    var appController = function () {
        return {
            home: function (id) {

                // if (!this.homeView) {
                // this.homeView = new HomeView();
                // }
                // $('#content').html(this.homeView.el);

                nzbDrone.App.Layout.content.show(new nzbDrone.Views.HomeView());

                this.menuItemSelected('home-menu');
            },

            list: function (page) {

                var p = page ? parseInt(page, 10) : 1;
                var profileList = new ProfileCollection();
                profileList.fetch({
                    success: function () {
                        $('#content').html(new QualityProfilesView({ model: profileList, page: p }).el);
                    }
                });
                this.menuItemSelected('home-menu');
            },

            wineDetails: function (id) {
                var profile = new Profile({ id: id });
                profile.fetch({
                    success: function () {
                        $('#content').html(new QualityProfileView({ model: profile }).el);
                    }
                });
                this.menuItemSelected();
            },

            addWine: function () {
                var wine = new Profile();
                $('#content').html(new QualityProfileView({ model: wine }).el);
                this.menuItemSelected('add-menu');
            },

            menuItemSelected: function (item) {

                // Using the application vent object as our global event aggregator
                nzbDrone.App.vent.trigger(
                    nzbDrone.Constants.Events.MenuItemSelected, // Event name
                    item); // Options
            }
        };
    };

    nzbDrone.AppController = appController;

})(window.NodeCellar);