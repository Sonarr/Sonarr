'use strict';
define(
    [
        'AppLayout',
        'marionette',
        'Shared/NotFoundView'
    ], function (AppLayout, Marionette, NotFoundView) {
        return Marionette.AppRouter.extend({

            showNotFound: function () {
                this.setTitle('Not Found');
                AppLayout.mainRegion.show(new NotFoundView(this));
            },

            setTitle: function (title) {
                if (title.toLocaleLowerCase() === 'nzbdrone') {
                    document.title = 'NzbDrone';
                }
                else {
                    document.title = title + ' - NzbDrone';
                }
            }
        });
    });

