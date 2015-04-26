module.exports = function() {
    var Backgrid = this;

    Backgrid.SonarrHeaderCell = Backgrid.HeaderCell.extend({
        events : {
            'click' : 'onClick'
        },

        _originalInit : Backgrid.HeaderCell.prototype.initialize,

        initialize : function(options) {
            this._originalInit.call(this, options);

            this.listenTo(this.collection, 'drone:sort', this.render);
        },

        render : function() {
            this.$el.empty();
            this.$el.append(this.column.get('label'));
            if (this.column.get('tooltip')) {
                this.$el.attr({
                    'title'          : this.column.get('tooltip'),
                    'data-container' : '.table'
                });
            }

            var column = this.column;
            var sortable = Backgrid.callByNeed(column.sortable(), column, this.collection);

            if (sortable) {
                this.$el.addClass('sortable');
                this.$el.prepend(' <i class="sort-direction-icon"></i>');
            }

            //Do we need this?
            this.$el.addClass(column.get('name'));

            if (column.has('className')) {
                this.$el.addClass(column.get('className'));
            }

            this.delegateEvents();
            this.direction(column.get('direction'));

            if (this.collection.state) {
                var name = this._getSortMapping().name;
                var order = this.collection.state.order;

                if (name === column.get('name')) {
                    this._setSortIcon(order);
                } else {
                    this._removeSortIcon();
                }
            }

            return this;
        },

        direction : function(dir) {
            this.$el.children('i.sort-direction-icon').removeClass('icon-sonarr-sort-asc icon-sonarr-sort-desc');

            if (arguments.length) {
                if (dir) {
                    this._setSortIcon(dir);
                }

                this.column.set('direction', dir);
            }

            var columnDirection = this.column.get('direction');

            if (!columnDirection && this.collection.state) {
                var name = this._getSortMapping().name;
                var order = this.collection.state.order;

                if (name === this.column.get('name')) {
                    columnDirection = order;
                }
            }

            return columnDirection;
        },

        _getSortMapping : function() {
            var sortKey = this.collection.state.sortKey;

            if (this.collection._getSortMapping) {
                return this.collection._getSortMapping(sortKey);
            }

            return {
                name    : sortKey,
                sortKey : sortKey
            };
        },

        onClick : function(e) {
            e.preventDefault();

            var collection = this.collection;
            var event = 'backgrid:sort';

            var column = this.column;
            var sortable = Backgrid.callByNeed(column.sortable(), column, collection);
            if (sortable) {
                var isSorted = this.$el.children('.icon-sonarr-sort-asc,.icon-sonarr-sort-desc').length !== 0;
                var direction = collection.state.order;
                if (column.get('sortType') === 'fixed' || !isSorted) {
                    direction = column.get('direction') || 'ascending';
                } else {
                    if (direction === 'ascending' || direction === -1) {
                        direction = 'descending';
                    } else {
                        direction = 'ascending';
                    }
                }

                if (collection.setSorting) {
                    collection.setSorting(column.get('name'), direction);
                } else {
                    collection.state.sortKey = column.get('name');
                    collection.state.order = direction;
                }
                collection.trigger(event, column, direction);
            }
        },

        _resetCellDirection : function(columnToSort, direction) {
            if (columnToSort !== this.column) {
                this.direction(null);
            } else {
                this.direction(direction);
            }
        },

        _convertDirectionToIcon : function(dir) {
            if (dir === 'ascending' || dir === -1) {
                return 'icon-sonarr-sort-asc';
            }

            return 'icon-sonarr-sort-desc';
        },

        _setSortIcon : function(dir) {
            this._removeSortIcon();
            this.$el.children('i.sort-direction-icon').addClass(this._convertDirectionToIcon(dir));
        },

        _removeSortIcon : function() {
            this.$el.children('i.sort-direction-icon').removeClass('icon-sonarr-sort-asc icon-sonarr-sort-desc');
        }
    });

    return Backgrid.SonarrHeaderCell;
};
