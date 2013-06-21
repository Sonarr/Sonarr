"use strict";
define(
    [
        'app',
        'Shared/Toolbar/Radio/RadioButtonView',
        'Config'
    ], function (App, RadioButtonView, Config) {
        NzbDrone.Shared.Toolbar.RadioButtonCollectionView = Backbone.Marionette.CollectionView.extend({
            className: 'btn-group',
            itemView : NzbDrone.Shared.Toolbar.RadioButtonView,

            attributes: {
                'data-toggle': 'buttons-radio'
            },

            initialize: function (options) {
                this.menu = options.menu;

                if (this.menu.storeState) {
                    this.setActive();
                }
            },

            setActive: function () {
                var storedKey = Config.GetValue(this.menu.menuKey, this.menu.defaultAction);

                this.collection.each(function (model) {
                    if (model.get('key').toLocaleLowerCase() === storedKey.toLowerCase()) {
                        model.set('active', true);
                    }
                    else {
                        model.set('active, false');
                    }
                });
            }
        });
    });


