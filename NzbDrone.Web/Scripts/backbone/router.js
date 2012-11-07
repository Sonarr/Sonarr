(function (nzbDrone, backbone) {
    nzbDrone.AppRouter = backbone.Marionette.AppRouter.extend({

        controller: new nzbDrone.AppController(),

        appRoutes: {
            '': 'home',
            'wines': 'list',
            'wines/page/:page': 'list',
            'wines/add': 'addWine',
            'wines/:id': 'wineDetails',
            'about': 'about',
            'search/:searchTerm': 'handleSearch'
        }
    });
})(window.NzbDrone, window.Backbone);s