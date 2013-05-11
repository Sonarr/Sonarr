"use strict";
define(['app', 'Shared/Toolbar/Radio/RadioButtonCollectionView','Shared/Toolbar/Button/ButtonCollectionView', 'Shared/Toolbar/ButtonCollection'], function () {
    NzbDrone.Shared.Toolbar.ToolbarLayout = Backbone.Marionette.Layout.extend({
        template: 'Shared/Toolbar/ToolbarLayoutTemplate',

        regions: {
            left_1 : '.x-toolbar-left-1',
            left_2 : '.x-toolbar-left-2',
            right_1: '.x-toolbar-right-1',
            right_2: '.x-toolbar-right-2'
        },

        initialize: function (options) {

            if (!options) {
                throw 'options needs to be passed';
            }

            if (!options.context) {
                throw 'context needs to be passed';
            }

            this.left = options.left;
            this.right = options.right;
            this.toolbarContext = options.context;

        },


        onShow: function () {
            if (this.left) {
                _.each(this.left, this._showToolbarLeft, this);
            }
            if (this.right) {
                _.each(this.right, this._showToolbarRight, this);
            }
        },

        _showToolbarLeft: function (element, index) {
            this._showToolbar(element, index, 'left');
        },

        _showToolbarRight: function (element, index) {
            this._showToolbar(element, index, 'right');
        },


        _showToolbar: function (buttonGroup, index, position) {

            var groupCollection = new NzbDrone.Shared.Toolbar.ButtonCollection();

            _.each(buttonGroup.items, function (button) {

                if (buttonGroup.storeState && !button.key) {
                    throw 'must provide key for all buttons when storSstate is enabled';
                }

                var model = new NzbDrone.Shared.Toolbar.ButtonModel(button);
                model.set('menuKey', buttonGroup.menuKey);
                model.ownerContext = this.toolbarContext;
                groupCollection.add(model);

            }, this);

            var buttonGroupView;

            switch (buttonGroup.type) {
                case 'radio':
                {
                    buttonGroupView = new NzbDrone.Shared.Toolbar.RadioButtonCollectionView(
                        {
                            collection: groupCollection,
                            menu      : buttonGroup
                        });
                    break;
                }
                default :
                {
                    buttonGroupView = new NzbDrone.Shared.Toolbar.ButtonCollectionView(
                        {
                            collection: groupCollection,
                            menu      : buttonGroup
                        });
                    break;
                }
            }

            this[position + '_' + (index + 1).toString()].show(buttonGroupView);
        }
    });

});




