'use strict';
define(
    [
        'backbone',
        'marionette',
        'underscore'
    ], function (Backbone, Marionette, _) {

        return Marionette.ItemView.extend({
            template : 'Shared/Toolbar/Sorting/SortingButtonViewTemplate',
            tagName  : 'li',

            ui: {
                icon: 'i'
            },

            events: {
                'click': 'onClick'
            },

            initialize: function (options) {
                this.viewCollection = options.viewCollection;
                this.listenTo(this.viewCollection, 'drone:sort', this.render);
                this.listenTo(this.viewCollection, 'backgrid:sort', this.render);
            },

            onRender: function () {
                if (this.viewCollection.state) {
                    var key = this.viewCollection.state.sortKey;
                    var order = this.viewCollection.state.order;

                    if (key === this.model.get('name')) {
                        this._setSortIcon(order);
                    }

                    else {
                        this._removeSortIcon();
                    }
                }
            },

            onClick: function (e) {
                e.preventDefault();

                var collection = this.viewCollection;
                var event = 'drone:sort';

                collection.state.sortKey = this.model.get('name');
                var direction = collection.state.order;

                if (direction === 'ascending' || direction === -1)
                {
                    collection.state.order = 'descending';
                    collection.trigger(event, this.model, 'descending');
                }
                else
                {
                    collection.state.order = 'ascending';
                    collection.trigger(event, this.model, 'ascending');
                }
            },

            _convertDirectionToIcon: function (dir) {
                if (dir === 'ascending' || dir === -1) {
                    return 'icon-sort-up';
                }

                return 'icon-sort-down';
            },

            _setSortIcon: function (dir) {
                this._removeSortIcon();
                this.ui.icon.addClass(this._convertDirectionToIcon(dir));
            },

            _removeSortIcon: function () {
                this.ui.icon.removeClass('icon-sort-up icon-sort-down');
            }
        });
    });




