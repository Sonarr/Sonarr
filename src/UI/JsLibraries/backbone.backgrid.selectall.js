/*
  backgrid-select-all
  http://github.com/wyuenho/backgrid

  Copyright (c) 2013 Jimmy Yuen Ho Wong and contributors
  Licensed under the MIT @license.
*/
(function (factory) {

  // CommonJS
  if (typeof exports == "object") {
    module.exports = factory(require("backbone"), require("backgrid"));
  }
  // Browser
  else if (typeof Backbone !== "undefined" && typeof Backgrid !== "undefined") {
    factory(Backbone, Backgrid);
  }

}(function (Backbone, Backgrid)  {

  "use strict";

  var $ = Backbone.$;

  /**
     Renders a checkbox for row selection.

     @class Backgrid.Extension.SelectRowCell
     @extends Backbone.View
  */
  var SelectRowCell = Backgrid.Extension.SelectRowCell = Backbone.View.extend({

    /** @property */
    className: "select-row-cell",

    /** @property */
    tagName: "td",

    /** @property */
    events: {
      "keydown :checkbox": "onKeydown",
      "change :checkbox": "onChange",
      "click :checkbox": "enterEditMode"
    },

    /**
       Initializer. If the underlying model triggers a `select` event, this cell
       will change its checked value according to the event's `selected` value.

       @param {Object} options
       @param {Backgrid.Column} options.column
       @param {Backbone.Model} options.model
    */
    initialize: function (options) {

      this.column = options.column;
      if (!(this.column instanceof Backgrid.Column)) {
        this.column = new Backgrid.Column(this.column);
      }

      this.listenTo(this.model, "backgrid:select", function (model, selected) {
        this.$el.find(":checkbox").prop("checked", selected).change();
      });

      var column = this.column, $el = this.$el;
      this.listenTo(column, "change:renderable", function (column, renderable) {
        $el.toggleClass("renderable", renderable);
      });

      if (column.get("renderable")) $el.addClass("renderable");
    },

    /**
       Focuses the checkbox.
    */
    enterEditMode: function () {
      this.$el.find(":checkbox").focus();
    },

    /**
       Unfocuses the checkbox.
    */
    exitEditMode: function () {
      this.$el.find(":checkbox").blur();
    },

    /**
       Process keyboard navigation.
    */
    onKeydown: function (e) {
      var command = new Backgrid.Command(e);
      if (command.passThru()) return true; // skip ahead to `change`
      if (command.cancel()) {
        e.stopPropagation();
        this.$el.find(":checkbox").blur();
      }
      else if (command.save() || command.moveLeft() || command.moveRight() ||
               command.moveUp() || command.moveDown()) {
        e.preventDefault();
        e.stopPropagation();
        this.model.trigger("backgrid:edited", this.model, this.column, command);
      }
    },

    /**
       When the checkbox's value changes, this method will trigger a Backbone
       `backgrid:selected` event with a reference of the model and the
       checkbox's `checked` value.
    */
    onChange: function (e) {
      var checked = $(e.target).prop('checked');
      this.$el.parent().toggleClass('selected', checked);
      this.model.trigger("backgrid:selected", this.model, checked);
    },

    /**
       Renders a checkbox in a table cell.
    */
    render: function () {
      this.$el.empty().append('<input tabindex="-1" type="checkbox" />');
      this.delegateEvents();
      return this;
    }

  });

  /**
     Renders a checkbox to select all rows on the current page.

     @class Backgrid.Extension.SelectAllHeaderCell
     @extends Backgrid.Extension.SelectRowCell
  */
  var SelectAllHeaderCell = Backgrid.Extension.SelectAllHeaderCell = SelectRowCell.extend({

    /** @property */
    className: "select-all-header-cell",

    /** @property */
    tagName: "th",

    /**
       Initializer. When this cell's checkbox is checked, a Backbone
       `backgrid:select` event will be triggered for each model for the current
       page in the underlying collection. If a `SelectRowCell` instance exists
       for the rows representing the models, they will check themselves. If any
       of the SelectRowCell instances trigger a Backbone `backgrid:selected`
       event with a `false` value, this cell will uncheck its checkbox. In the
       event of a Backbone `backgrid:refresh` event, which is triggered when the
       body refreshes its rows, which can happen under a number of conditions
       such as paging or the columns were reset, this cell will still remember
       the previously selected models and trigger a Backbone `backgrid:select`
       event on them such that the SelectRowCells can recheck themselves upon
       refreshing.

       @param {Object} options
       @param {Backgrid.Column} options.column
       @param {Backbone.Collection} options.collection
    */
    initialize: function (options) {

      this.column = options.column;
      if (!(this.column instanceof Backgrid.Column)) {
        this.column = new Backgrid.Column(this.column);
      }

      var collection = this.collection;
      var selectedModels = this.selectedModels = {};
      this.listenTo(collection, "backgrid:selected", function (model, selected) {
        if (selected) selectedModels[model.id || model.cid] = model;
        else {
          delete selectedModels[model.id || model.cid];
          this.$el.find(":checkbox").prop("checked", false);
        }
      });

      this.listenTo(collection, "remove", function (model) {
        delete selectedModels[model.id || model.cid];
      });

      this.listenTo(collection, "backgrid:refresh", function () {
        this.$el.find(":checkbox").prop("checked", false);
        for (var i = 0; i < collection.length; i++) {
          var model = collection.at(i);
          if (selectedModels[model.id || model.cid]) {
            model.trigger('backgrid:select', model, true);
          }
        }
      });

      var column = this.column, $el = this.$el;
      this.listenTo(column, "change:renderable", function (column, renderable) {
        $el.toggleClass("renderable", renderable);
      });

      if (column.get("renderable")) $el.addClass("renderable");
    },

    /**
       Progagates the checked value of this checkbox to all the models of the
       underlying collection by triggering a Backbone `backgrid:select` event on
       the models themselves, passing each model and the current `checked` value
       of the checkbox in each event.
    */
    onChange: function (e) {
      var checked = $(e.target).prop("checked");

      var collection = this.collection;
      collection.each(function (model) {
        model.trigger("backgrid:select", model, checked);
      });
    }

  });

  /**
     Convenient method to retrieve a list of selected models. This method only
     exists when the `SelectAll` extension has been included.

     @member Backgrid.Grid
     @return {Array.<Backbone.Model>}
  */
  Backgrid.Grid.prototype.getSelectedModels = function () {
    var selectAllHeaderCell;
    var headerCells = this.header.row.cells;
    for (var i = 0, l = headerCells.length; i < l; i++) {
      var headerCell = headerCells[i];
      if (headerCell instanceof SelectAllHeaderCell) {
        selectAllHeaderCell = headerCell;
        break;
      }
    }

    var result = [];
    if (selectAllHeaderCell) {
      for (var modelId in selectAllHeaderCell.selectedModels) {
        result.push(this.collection.get(modelId));
      }
    }

    return result;
  };

}));
