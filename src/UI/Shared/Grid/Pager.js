'use strict';
define(
    [
        'jquery',
        'marionette',
        'backgrid.paginator'
    ], function ($, Marionette, Paginator) {

        return Paginator.extend({

            template: 'Shared/Grid/PagerTemplate',

            events: {
                'click .pager-btn'             : 'changePage',
                'click .x-page-number'         : '_showPageJumper',
                'change .x-page-number select' : '_jumpToPage',
                'blur .x-page-number select'   : 'render'
            },

            windowSize: 1,

            fastForwardHandleLabels: {
                first: 'icon-fast-backward',
                prev : 'icon-backward',
                next : 'icon-forward',
                last : 'icon-fast-forward'
            },

            changePage: function (e) {
                e.preventDefault();

                var target = this.$(e.target);

                if (target.closest('li').hasClass('disabled')) {
                    return;
                }

                target.closest('li i').addClass('icon-spinner icon-spin');

                var label = target.attr('data-action');
                var ffLabels = this.fastForwardHandleLabels;

                var collection = this.collection;

                if (ffLabels) {
                    switch (label) {
                        case 'first':
                            collection.getFirstPage();
                            return;
                        case 'prev':
                            if (collection.hasPrevious()) {
                                collection.getPreviousPage();
                            }
                            return;
                        case 'next':
                            if (collection.hasNext()) {
                                collection.getNextPage();
                            }
                            return;
                        case 'last':
                            collection.getLastPage();
                            return;
                    }
                }

                var state = collection.state;
                var pageIndex = target.text();
                collection.getPage(state.firstPage === 0 ? pageIndex - 1 :pageIndex);
            },

            makeHandles: function () {

                var handles =
                    [
                    ];
                var collection = this.collection;
                var state = collection.state;

                // convert all indices to 0-based here
                var firstPage = state.firstPage;
                var lastPage = +state.lastPage;
                lastPage = Math.max(0, firstPage ? lastPage - 1 :lastPage);
                var currentPage = Math.max(state.currentPage, state.firstPage);
                currentPage = firstPage ? currentPage - 1 : currentPage;
                var windowStart = Math.floor(currentPage / this.windowSize) * this.windowSize;
                var windowEnd = Math.min(lastPage + 1, windowStart + this.windowSize);

                if (collection.mode !== 'infinite') {
                    for (var i = windowStart; i < windowEnd; i++) {
                        handles.push({
                            label       : i + 1,
                            title       : 'No. ' + (i + 1),
                            className   : currentPage === i ? 'active' :undefined,
                            pageNumber  : i + 1,
                            lastPage    : lastPage + 1
                        });
                    }
                }

                var ffLabels = this.fastForwardHandleLabels;
                if (ffLabels) {

                    if (ffLabels.prev) {
                        handles.unshift({
                            label    : ffLabels.prev,
                            className: collection.hasPrevious() ? void 0 :'disabled',
                            action   : 'prev'
                        });
                    }

                    if (ffLabels.first) {
                        handles.unshift({
                            label    : ffLabels.first,
                            className: collection.hasPrevious() ? void 0 :'disabled',
                            action   : 'first'
                        });
                    }

                    if (ffLabels.next) {
                        handles.push({
                            label    : ffLabels.next,
                            className: collection.hasNext() ? void 0 :'disabled',
                            action   : 'next'
                        });
                    }

                    if (ffLabels.last) {
                        handles.push({
                            label    : ffLabels.last,
                            className: collection.hasNext() ? void 0 :'disabled',
                            action   : 'last'
                        });
                    }
                }

                return handles;
            },

            render: function () {
                this.$el.empty();

                var templateFunction = Marionette.TemplateCache.get(this.template);

                this.$el.html(templateFunction({
                    handles: this.makeHandles(),
                    state: this.collection.state
                }));

                this.delegateEvents();

                return this;
            },

            _showPageJumper: function (e) {
                if ($(e.target).is('select')) {
                    return;
                }

                var templateFunction = Marionette.TemplateCache.get('Shared/Grid/JumpToPageTemplate');
                var state = this.collection.state;
                var currentPage = Math.max(state.currentPage, state.firstPage);
                currentPage = state.firstPage ? currentPage - 1 : currentPage;

                var pages = [];

                for (var i = 0; i < this.collection.state.lastPage; i++) {
                    if (i === currentPage) {
                        pages.push({ page: i + 1, current: true });
                    }

                    else {
                        pages.push({ page: i + 1 });
                    }
                }

                this.$el.find('.x-page-number').html(templateFunction({
                    pages      : pages
                }));

                this.$el.find('.x-page-number select').trigger('click');
            },

            _jumpToPage: function () {
                var selectedPage = parseInt(this.$el.find('.x-page-number select').val(), 10);

                this.$el.find('.x-page-number').html('<i class="icon-spinner icon-spin"></i>');
                this.collection.getPage(selectedPage);
            }
        });
    });
