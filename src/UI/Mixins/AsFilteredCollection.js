'use strict';

define(
    [
        'underscore',
        'backbone'],
    function (_, Backbone) {

        return function () {

            this.prototype.setFilter = function(filter, options) {
                options = _.extend({ reset: true }, options || {});

                this.state.filterKey = filter[0];
                this.state.filterValue = filter[1];

                if (options.reset) {
                    if (this.mode != 'server') {
                        this.fullCollection.resetFiltered();
                    } else {
                        return this.fetch();
                    }
                }
            };
            
            this.prototype.setFilterMode = function(mode, options) {
                this.setFilter(this.filterModes[mode], options);
            };

            var originalMakeFullCollection = this.prototype._makeFullCollection;

            this.prototype._makeFullCollection = function (models, options) {
                var self = this;

                self.shadowCollection = originalMakeFullCollection.call(this, models, options);
                
                var filterModel = function(model) {
                    if (!self.state.filterKey || !self.state.filterValue)
                        return true;
                    else
                        return model.get(self.state.filterKey) === self.state.filterValue;
                };

                self.shadowCollection.filtered = function() {
                    return this.filter(filterModel);
                };

                var filteredModels = self.shadowCollection.filtered();
                var fullCollection = originalMakeFullCollection.call(this, filteredModels, options);

                fullCollection.resetFiltered = function(options) {
                    Backbone.Collection.prototype.reset.call(this, self.shadowCollection.filtered(), options);
                };
                
                fullCollection.reset = function (models, options) {
                    self.shadowCollection.reset(models, options);
                    self.fullCollection.resetFiltered();
                };

                return fullCollection;
            };

            _.extend(this.prototype.state, {
                filterKey   : null,
                filterValue : null
            });

            _.extend(this.prototype.queryParams, {
                filterKey   : 'filterKey',
                filterValue : 'filterValue'
            });

            return this;
        };
    }
);