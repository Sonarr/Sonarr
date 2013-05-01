/*
  backgrid-paginator
  http://github.com/wyuenho/backgrid

  Copyright (c) 2013 Jimmy Yuen Ho Wong and contributors
  Licensed under the MIT @license.
*/

(function ($, _, Backbone, Backgrid) {

  "use strict";

  /**
     Paginator is a Backgrid extension that renders a series of configurable
     pagination handles. This extension is best used for splitting a large data
     set across multiple pages. If the number of pages is larger then a
     threshold, which is set to 10 by default, the page handles are rendered
     within a sliding window, plus the fast forward, fast backward, previous and
     next page handles. The fast forward, fast backward, previous and next page
     handles can be turned off.

     @class Backgrid.Extension.Paginator
  */
  Backgrid.Extension.Paginator = Backbone.View.extend({

    /** @property */
    className: "backgrid-paginator",

    /** @property */
    windowSize: 10,

    /**
       @property {Object} fastForwardHandleLabels You can disable specific
       handles by setting its value to `null`.
    */
    fastForwardHandleLabels: {
      first: "《",
      prev: "〈",
      next: "〉",
      last: "》"
    },

    /** @property */
    template: _.template('<ul><% _.each(handles, function (handle) { %><li <% if (handle.className) { %>class="<%= handle.className %>"<% } %>><a href="#" <% if (handle.title) {%> title="<%= handle.title %>"<% } %>><%= handle.label %></a></li><% }); %></ul>'),

    /** @property */
    events: {
      "click a": "changePage"
    },

    /**
       Initializer.

       @param {Object} options
       @param {Backbone.Collection.<Backgrid.Column>|Array.<Backgrid.Column>|Array.<Object>} options.columns
       Column metadata.
       @param {Backbone.Collection} options.collection
       @param {boolean} [options.fastForwardHandleLabels] Whether to render fast forward buttons.
    */
    initialize: function (options) {
      Backgrid.requireOptions(options, ["columns", "collection"]);

      this.columns = options.columns;
      if (!(this.columns instanceof Backbone.Collection)) {
        this.columns = new Backgrid.Columns(this.columns);
      }

      var columns = this.columns;
      this.listenTo(columns, "add", this.render);
      this.listenTo(columns, "remove", this.render);
      this.listenTo(columns, "change:renderable", this.render);
      var collection = this.collection;
      var fullCollection = collection.fullCollection;
      if (fullCollection) {
        this.listenTo(fullCollection, "add", this.render);
        this.listenTo(fullCollection, "remove", this.render);
        this.listenTo(fullCollection, "reset", this.render);
      }
      else {
        this.listenTo(collection, "add", this.render);
        this.listenTo(collection, "remove", this.render);
        this.listenTo(collection, "reset", this.render);
      }
    },

    /**
       jQuery event handler for the page handlers. Goes to the right page upon
       clicking.

       @param {Event} e
     */
    changePage: function (e) {
      e.preventDefault();

      var label = $(e.target).text();
      var ffLabels = this.fastForwardHandleLabels;

      var collection = this.collection;

      if (ffLabels) {
        switch (label) {
        case ffLabels.first:
          collection.getFirstPage();
          return;
        case ffLabels.prev:
          if (collection.hasPrevious()) collection.getPreviousPage();
          return;
        case ffLabels.next:
          if (collection.hasNext()) collection.getNextPage();
          return;
        case ffLabels.last:
          collection.getLastPage();
          return;
        }
      }

      var state = collection.state;
      var pageIndex = $(e.target).text() * 1;
      collection.getPage(state.firstPage === 0 ? pageIndex - 1 : pageIndex);
    },

    /**
       Internal method to create a list of page handle objects for the template
       to render them.

       @return {Array.<Object>} an array of page handle objects hashes
     */
    makeHandles: function () {

      var handles = [];
      var collection = this.collection;
      var state = collection.state;

      // convert all indices to 0-based here
      var lastPage = state.lastPage ? state.lastPage : state.firstPage;
      lastPage = state.firstPage === 0 ? lastPage : lastPage - 1;
      var currentPage = state.firstPage === 0 ? state.currentPage : state.currentPage - 1;
      var windowStart = Math.floor(currentPage / this.windowSize) * this.windowSize;
      var windowEnd = windowStart + this.windowSize;
      windowEnd = windowEnd <= lastPage ? windowEnd : lastPage + 1;

      if (collection.mode !== "infinite") {
        for (var i = windowStart; i < windowEnd; i++) {
          handles.push({
            label: i + 1,
            title: "No. " + (i + 1),
            className: currentPage === i ? "active" : undefined
          });
        }
      }

      var ffLabels = this.fastForwardHandleLabels;
      if (ffLabels) {

        if (ffLabels.prev) {
          handles.unshift({
            label: ffLabels.prev,
            className: collection.hasPrevious() ? void 0 : "disabled"
          });
        }

        if (ffLabels.first) {
          handles.unshift({
            label: ffLabels.first,
            className: collection.hasPrevious() ? void 0 : "disabled"
          });
        }

        if (ffLabels.next) {
          handles.push({
            label: ffLabels.next,
            className: collection.hasNext() ? void 0 : "disabled"
          });
        }

        if (ffLabels.last) {
          handles.push({
            label: ffLabels.last,
            className: collection.hasNext() ? void 0 : "disabled"
          });
        }
      }

      return handles;
    },

    /**
       Render the paginator handles inside an unordered list placed inside a
       cell that spans all the columns.
    */
    render: function () {
      this.$el.empty();

      this.$el.append(this.template({
        handles: this.makeHandles()
      }));

      this.delegateEvents();

      return this;
    }

  });

}(jQuery, _, Backbone, Backgrid));
