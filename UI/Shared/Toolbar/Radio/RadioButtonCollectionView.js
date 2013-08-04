'use strict';
define(
    [
        'marionette',
        'Shared/Toolbar/Radio/RadioButtonView',
        'Config'
    ], function (Marionette, RadioButtonView, Config) {
        return Marionette.CollectionView.extend({
            className: 'btn-group',
            itemView : RadioButtonView,

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
                var storedKey = Config.getValue(this.menu.menuKey, this.menu.defaultAction);

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


