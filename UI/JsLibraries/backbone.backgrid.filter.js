/*
  backgrid-filter
  http://github.com/wyuenho/backgrid

  Copyright (c) 2013 Jimmy Yuen Ho Wong and contributors
  Licensed under the MIT @license.
*/

(function ($, _, Backbone, Backgrid, lunr) {

  "use strict";

  /**
     ServerSideFilter is a search form widget that submits a query to the server
     for filtering the current collection.

     @class Backgrid.Extension.ServerSideFilter
  */
  var ServerSideFilter = Backgrid.Extension.ServerSideFilter = Backbone.View.extend({

    /** @property */
    tagName: "form",

    /** @property */
    className: "backgrid-filter form-search",

    /** @property {function(Object, ?Object=): string} template */
    template: _.template('<div class="input-prepend input-append"><span class="add-on"><i class="icon-search"></i></span><input type="text" <% if (placeholder) { %> placeholder="<%- placeholder %>" <% } %> name="<%- name %>" /><span class="add-on"><a class="close" href="#">&times;</a></span></div>'),

    /** @property */
    events: {
      "click .close": "clear",
      "submit": "search"
    },

    /** @property {string} [name='q'] Query key */
    name: "q",

    /** @property The HTML5 placeholder to appear beneath the search box. */
    placeholder: null,

    /**
       @param {Object} options
       @param {Backbone.Collection} options.collection
       @param {String} [options.name]
       @param {String} [options.placeholder]
    */
    initialize: function (options) {
      Backgrid.requireOptions(options, ["collection"]);
      Backbone.View.prototype.initialize.apply(this, arguments);
      this.name = options.name || this.name;
      this.placeholder = options.placeholder || this.placeholder;

      var collection = this.collection, self = this;
      if (Backbone.PageableCollection &&
          collection instanceof Backbone.PageableCollection &&
          collection.mode == "server") {
        collection.queryParams[this.name] = function () {
          return self.$el.find("input[type=text]").val();
        };
      }
    },

    /**
       Upon search form submission, this event handler constructs a query
       parameter object and pass it to Collection#fetch for server-side
       filtering.
    */
    search: function (e) {
      if (e) e.preventDefault();
      var data = {};
      data[this.name] = this.$el.find("input[type=text]").val();
      this.collection.fetch({data: data});
    },

    /**
       Event handler for the close button. Clears the search box and refetch the
       collection.
    */
    clear: function (e) {
      if (e) e.preventDefault();
      this.$("input[type=text]").val(null);
      this.collection.fetch();
    },

    /**
       Renders a search form with a text box, optionally with a placeholder and
       a preset value if supplied during initialization.
    */
    render: function () {
      this.$el.empty().append(this.template({
        name: this.name,
        placeholder: this.placeholder,
        value: this.value
      }));
      this.delegateEvents();
      return this;
    }

  });

  /**
     ClientSideFilter is a search form widget that searches a collection for
     model matches against a query on the client side. The exact matching
     algorithm can be overriden by subclasses.

     @class Backgrid.Extension.ClientSideFilter
     @extends Backgrid.Extension.ServerSideFilter
  */
  var ClientSideFilter = Backgrid.Extension.ClientSideFilter = ServerSideFilter.extend({

    /** @property */
    events: {
      "click .close": function (e) {
        e.preventDefault();
        this.clear();
      },
      "change input[type=text]": "search",
      "keyup input[type=text]": "search",
      "submit": function (e) {
        e.preventDefault();
        this.search();
      }
    },

    /**
       @property {?Array.<string>} A list of model field names to search
       for matches. If null, all of the fields will be searched.
    */
    fields: null,

    /**
       @property wait The time in milliseconds to wait since for since the last
       change to the search box's value before searching. This value can be
       adjusted depending on how often the search box is used and how large the
       search index is.
    */
    wait: 149,

    /**
       Debounces the #search and #clear methods and makes a copy of the given
       collection for searching.

       @param {Object} options
       @param {Backbone.Collection} options.collection
       @param {String} [options.placeholder]
       @param {String} [options.fields]
       @param {String} [options.wait=149]
    */
    initialize: function (options) {
      ServerSideFilter.prototype.initialize.apply(this, arguments);

      this.fields = options.fields || this.fields;
      this.wait = options.wait || this.wait;

      this._debounceMethods(["search", "clear"]);

      var collection = this.collection;
      var shadowCollection = this.shadowCollection = collection.clone();
      shadowCollection.url = collection.url;
      shadowCollection.sync = collection.sync;
      shadowCollection.parse = collection.parse;

      this.listenTo(collection, "add", function (model, collection, options) {
        shadowCollection.add(model, options);
      });
      this.listenTo(collection, "remove", function (model, collection, options) {
        shadowCollection.remove(model, options);
      });
      this.listenTo(collection, "sort reset", function (collection, options) {
        options = _.extend({reindex: true}, options || {});
        if (options.reindex) shadowCollection.reset(collection.models);
      });
    },

    _debounceMethods: function (methodNames) {
      if (_.isString(methodNames)) methodNames = [methodNames];

      this.undelegateEvents();

      for (var i = 0, l = methodNames.length; i < l; i++) {
        var methodName = methodNames[i];
        var method = this[methodName];
        this[methodName] = _.debounce(method, this.wait);
      }

      this.delegateEvents();
    },

    /**
       This default implementation takes a query string and returns a matcher
       function that looks for matches in the model's #fields or all of its
       fields if #fields is null, for any of the words in the query
       case-insensitively.

       Subclasses overriding this method must take care to conform to the
       signature of the matcher function. In addition, when the matcher function
       is called, its context will be bound to this ClientSideFilter object so
       it has access to the filter's attributes and methods.

       @param {string} query The search query in the search box.
       @return {function(Backbone.Model):boolean} A matching function.
    */
    makeMatcher: function (query) {
      var regexp = new RegExp(query.trim().split(/\W/).join("|"), "i");
      return function (model) {
        var keys = this.fields || model.keys();
        for (var i = 0, l = keys.length; i < l; i++) {
          if (regexp.test(model.get(keys[i]) + "")) return true;
        }
        return false;
      };
    },

    /**
       Takes the query from the search box, constructs a matcher with it and
       loops through collection looking for matches. Reset the given collection
       when all the matches have been found.
    */
    search: function () {
      var matcher = _.bind(this.makeMatcher(this.$("input[type=text]").val()), this);
      this.collection.reset(this.shadowCollection.filter(matcher), {reindex: false});
    },

    /**
       Clears the search box and reset the collection to its original.
    */
    clear: function () {
      this.$("input[type=text]").val(null);
      this.collection.reset(this.shadowCollection.models, {reindex: false});
    }

  });

  /**
     LunrFilter is a ClientSideFilter that uses [lunrjs](http://lunrjs.com/) to
     index the text fields of each model for a collection, and performs
     full-text searching.

     @class Backgrid.Extension.LunrFilter
     @extends Backgrid.Extension.ClientSideFilter
  */
  Backgrid.Extension.LunrFilter = ClientSideFilter.extend({

    /**
       @property {string} [ref="id"]｀lunrjs` document reference attribute name.
    */
    ref: "id",

    /**
       @property {Object} fields A hash of `lunrjs` index field names and boost
       value. Unlike ClientSideFilter#fields, LunrFilter#fields is _required_ to
       initialize the index.
    */
    fields: null,

    /**
       Indexes the underlying collection on construction. The index will refresh
       when the underlying collection is reset. If any model is added, removed
       or if any indexed fields of any models has changed, the index will be
       updated.

       @param {Object} options
       @param {Backbone.Collection} options.collection
       @param {String} [options.placeholder]
       @param {string} [options.ref] ｀lunrjs` document reference attribute name.
       @param {Object} [options.fields] A hash of `lunrjs` index field names and
       boost value.
       @param {number} [options.wait]
    */
    initialize: function (options) {
      ClientSideFilter.prototype.initialize.apply(this, arguments);

      this.ref = options.ref || this.ref;

      var collection = this.collection;
      this.listenTo(collection, "add", this.addToIndex);
      this.listenTo(collection, "remove", this.removeFromIndex);
      this.listenTo(collection, "reset", this.resetIndex);
      this.listenTo(collection, "change", this.updateIndex);

      this.resetIndex(collection);
    },

    /**
       Reindex the collection. If `options.reindex` is `false`, this method is a
       no-op.

       @param {Backbone.Collection} collection
       @param {Object} [options]
       @param {boolean} [options.reindex=true]
    */
    resetIndex: function (collection, options) {
      options = _.extend({reindex: true}, options || {});

      if (options.reindex) {
        var self = this;
        this.index = lunr(function () {
          _.each(self.fields, function (boost, fieldName) {
            this.field(fieldName, boost);
            this.ref(self.ref);
          }, this);
        });

        collection.each(function (model) {
          this.addToIndex(model);
        }, this);
      }
    },

    /**
       Adds the given model to the index.

       @param {Backbone.Model} model
    */
    addToIndex: function (model) {
      var index = this.index;
      var doc = model.toJSON();
      if (index.documentStore.has(doc[this.ref])) index.update(doc);
      else index.add(doc);
    },

    /**
       Removes the given model from the index.

       @param {Backbone.Model} model
    */
    removeFromIndex: function (model) {
      var index = this.index;
      var doc = model.toJSON();
      if (index.documentStore.has(doc[this.ref])) index.remove(doc);
    },

    /**
       Updates the index for the given model.

       @param {Backbone.Model} model
    */
    updateIndex: function (model) {
      var changed = model.changedAttributes();
      if (changed && !_.isEmpty(_.intersection(_.keys(this.fields),
                                               _.keys(changed)))) {
        this.index.update(model.toJSON());
      }
    },

    /**
       Takes the query from the search box and performs a full-text search on
       the client-side. The search result is returned by resetting the
       underlying collection to the models after interrogating the index for the
       query answer.
    */
    search: function () {
      var searchResults = this.index.search(this.$("input[type=text]").val());
      var models = [];
      for (var i = 0; i < searchResults.length; i++) {
        var result = searchResults[i];
        models.push(this.shadowCollection.get(result.ref));
      }
      this.collection.reset(models, {reindex: false});
    }

  });

}(jQuery, _, Backbone, Backgrid, lunr));
