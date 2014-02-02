'use strict';
define(
    [
        'marionette',
        'Shared/Toolbar/ButtonCollection',
        'Shared/Toolbar/ButtonModel',
        'Shared/Toolbar/Radio/RadioButtonCollectionView',
        'Shared/Toolbar/Button/ButtonCollectionView',
        'Shared/Toolbar/Sorting/SortingButtonCollectionView',
        'underscore'
    ], function (Marionette, ButtonCollection, ButtonModel, RadioButtonCollectionView, ButtonCollectionView, SortingButtonCollectionView, _) {
        return Marionette.Layout.extend({
            template : 'Shared/Toolbar/ToolbarLayoutTemplate',
            className: 'toolbar',

            ui: {
                left_x : '.x-toolbar-left',
                right_x: '.x-toolbar-right'
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

                var groupCollection = new ButtonCollection();

                _.each(buttonGroup.items, function (button) {

                    if (buttonGroup.storeState && !button.key) {
                        throw 'must provide key for all buttons when storSstate is enabled';
                    }

                    var model = new ButtonModel(button);
                    model.set('menuKey', buttonGroup.menuKey);
                    model.ownerContext = this.toolbarContext;
                    groupCollection.add(model);

                }, this);

                var buttonGroupView;

                switch (buttonGroup.type) {
                    case 'radio':
                    {
                        buttonGroupView = new RadioButtonCollectionView({
                            collection: groupCollection,
                            menu      : buttonGroup
                        });
                        break;
                    }
                    case 'sorting':
                    {
                        buttonGroupView = new SortingButtonCollectionView({
                            collection    : groupCollection,
                            menu          : buttonGroup,
                            viewCollection: buttonGroup.viewCollection
                        });
                        break;
                    }
                    default :
                    {
                        buttonGroupView = new ButtonCollectionView({
                            collection: groupCollection,
                            menu      : buttonGroup
                        });
                        break;
                    }
                }
                
                var regionId = position + "_" + (index + 1);                
                var region = this[regionId];
                                
                if (!region) {
                    var regionClassName = "x-toolbar-" + position + "-" + (index + 1);
                    this.ui[position + '_x'].append('<div class="toolbar-group '+regionClassName+'" />\r\n');                
                    region = this.addRegion(regionId, "." + regionClassName);
                }
                
                region.show(buttonGroupView);
            }
        });
    });




