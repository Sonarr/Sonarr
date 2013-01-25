https://github.com/marionettejs/backbone.marionette/blob/viewswap/docs/marionette.viewswapper.md

// View Swapper
// ------------
//
// Switch out views based on events that are triggered
// by the currently displayed view. Enables easy "edit in
// place" features, "loading" screens, and more.

    Marionette.ViewSwapper = Marionette.View.extend({
        constructor: function (options) {
            this._swapperViews = {};
            this._swapperBindings = new Marionette.EventBinder();
            this._currentViewBindings = new Marionette.EventBinder();

            Marionette.View.prototype.constructor.apply(this, arguments);

            this.views = Marionette.getOption(this, "views");
            this.swapOn = Marionette.getOption(this, "swapOn");
            this.initialView = Marionette.getOption(this, "initialView");

            this._setupViewEvents("swapper", this, this._swapperBindings);
        },

        // Render the current view. If no current view is set, it
        // will render the `initialView` that was configured.
        render: function () {
            // set up the initial view to display, on first render
            if (!this.currentView) {
                var initialViewName = Marionette.getOption(this, "initialView");
                this._swapView(initialViewName);
            }

            // render and show the new view
            this.currentView.render();
            this.$el.append(this.currentView.$el);

            // setup a callback for the showView call to recieve
            var done = _.bind(function () {
                // trigger show/onShow on the previous view
                if (this.currentView) {
                    Marionette.triggerMethod.call(this.currentView, "show");
                    Marionette.triggerMethod.call(this, "swap:in", this.currentView);
                }
            }, this);

            // show the view, passing it the done callback
            this.showView(this.currentView, done);
        },

        // Show a view that is being swapped in. Override this method to
        // set up your own custom fade in / show method
        showView: function (view, done) {
            view.$el.show();
            done();
        },

        // Hide a view that is being swapped out. Override this method to
        // set up your own custom fade out / hide method
        hideView: function (view, done) {
            view.$el.hide();
            done();
        },

        // Ensure the views that were configured for this view swapper get closed
        close: function () {

            // Close all of the configured views that we are swapping between
            _.each(this.views, function (view, name) {
                view.close();
            });

            // unbind all the events, and clean up any decorator views
            this._swapperViews = {};
            this._currentViewBindings.unbindAll();
            this._swapperBindings.unbindAll();

            // Close the base view that we extended from
            Marionette.View.prototype.close.apply(this, arguments);
        },

        // Get a view by name, throwing an exception if the view instance
        // is not found.
        _getView: function (viewName) {
            var originalView, error, views;
            var swapperView = this._swapperViews[viewName];

            // Do not allow the name "swapper" to be used as a target view
            // or initial view. This is reserved for the ViewSwapper instance,
            // when configuring `swapOn` events
            if (viewName === "swapper") {
                error = new Error("Cannot display 'swapper' as a view.");
                error.name = "InvalidViewName";
                throw error;
            }

            // Do we have a view with the specified name?
            if (!swapperView) {
                originalView = this.views[viewName];

                // No view, so throw an exception
                if (!originalView) {
                    error = new Error("Cannot show view in ViewSwapper. View '" + viewName + "' not found.");
                    error.name = "ViewNotFoundError";
                    throw error;
                }

                // Found the view, so build a Decorator around it
                swapperView = this._buildSwapperView(originalView, viewName);
                this._swapperViews[viewName] = swapperView;
            }

            return swapperView;
        },

        // Decorate the configured view with information that the view swapper
        // needs, to keep track of the view's current state.
        _buildSwapperView: function (originalView, viewName) {
            var swapperView = Marionette.createObject(originalView);
            _.extend(swapperView, {

                viewName: viewName,
                originalView: originalView,

                // Prevent the underlying view from being rendered more than once
                render: function () {
                    var value;

                    if (this._hasBeenRendered) {
                        return this;
                    } else {

                        // prevent any more rendering
                        this._hasBeenRendered = true;

                        // do the render
                        value = originalView.render.apply(originalView, arguments);

                        // trigger render/onRender
                        Marionette.triggerMethod.call(this, "render");

                        // return whatever was sent back to us
                        return value;
                    }
                }

            });

            return swapperView;
        },

        // Set up the event handlers for the individual views, so that the
        // swapping can happen when a view event is triggered
        _setupViewEvents: function (viewName, view, bindings) {
            if (!this.swapOn || !this.swapOn[viewName]) { return; }
            var that = this;

            // default to current view bindings, unless otherwise specified
            if (!bindings) {
                bindings = this._currentViewBindings;
            }

            // close the previous event bindings
            bindings.unbindAll();

            // set up the new view's event bindings
            _.each(this.swapOn[viewName], function (targetViewName, eventName) {

                bindings.bindTo(view, eventName, function () {
                    that._swapView(targetViewName);
                });

            });
        },

        // Do the swapping of the views to the new view, by name
        _swapView: function (viewName) {

            // only swap views if the target view is not the same
            // as the current view
            var view = this._getView(viewName);
            if (view === this.currentView) {
                return;
            }

            // Provide a callback function that will switch over to
            // the new view, when called
            var done = _.bind(function () {

                // trigger hide/onHide on the previous view
                if (this.currentView) {
                    Marionette.triggerMethod.call(this.currentView, "hide");
                    Marionette.triggerMethod.call(this, "swap:out", this.currentView);
                }

                // get the next view, configure it's events and render it
                this._setupViewEvents(viewName, view);
                this.currentView = view;
                this.render();

            }, this);

            if (this.currentView) {
                // if we have a current view, hide it so that the new
                // view can be show in it's place
                this.hideView(this.currentView, done);
            } else {
                // no current view, so just switch to the new view
                done();
            }
        }
    });