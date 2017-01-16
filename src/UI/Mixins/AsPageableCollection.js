var _ = require('underscore');

module.exports = function() {
    var originalMakeCollectionEventHandler = this.prototype._makeCollectionEventHandler;

    this.prototype._makeCollectionEventHandler = function (pageCollection, fullCollection) {
        var self = this;
        this.pageCollection = pageCollection;
        this.fullCollection = fullCollection;
        var eventHandler = originalMakeCollectionEventHandler.apply(this, arguments);

        return _.wrap(eventHandler, _.bind(self._resetEventHandler, self));
    };

    this.prototype._resetEventHandler = function (originalEventHandler, event, model, collection, options) {
        if (event === 'reset') {
            var currentPage = this.state.currentPage;
            var pageSize = this.state.pageSize;

            originalEventHandler.apply(this, [].slice.call(arguments, 1));

            var totalPages = Math.max(1,Math.ceil(this.state.totalRecords / pageSize));
            var newPage =  Math.min(currentPage, totalPages);

            if (newPage !== this.state.currentPage) {
                this.state.currentPage = newPage;

                // If backbone pageable fixes their reset bug
                // (they reset the page number, but not the range),
                // we'll want to do this for all resets where the page number changed
                if (currentPage !== newPage) {
                    var pageStart = (newPage - 1) * pageSize;
                    var pageEnd = pageStart + pageSize;

                    this.pageCollection.reset(this.fullCollection.models.slice(pageStart, pageEnd),
                        _.extend({}, options, { parse : false }));
                }
            }
        } else {
            originalEventHandler.call(this, [].slice.call(arguments, 1));
        }
    };

    return this;
};
