'use strict';

define(
    ['AppLayout'],
    function (AppLayout) {

        return function () {

            var originalInitialize = this.prototype.initialize;
            var originalOnBeforeClose = this.prototype.onBeforeClose;

            var saveInternal = function () {
                var self = this;
                this.ui.indicator.show();

                if (this._onBeforeSave) {
                    this._onBeforeSave.call(this);
                }

                var promise = this.model.save();

                promise.always(function () {
                    self.ui.indicator.hide();
                });

                promise.done(function () {
                    self.originalModelData = self.model.toJSON();
                });

                return promise;
            };

            this.prototype.initialize = function (options) {

                if (!this.model) {
                    throw 'View has no model';
                }

                this.originalModelData = this.model.toJSON();

                this.events = this.events || {};
                this.events['click .x-save'] = '_save';
                this.events['click .x-save-and-add'] = '_saveAndAdd';
                this.events['click .x-test'] = '_test';
                this.events['click .x-delete'] = '_delete';

                this.ui = this.ui || {};
                this.ui.indicator = '.x-indicator';

                if (originalInitialize) {
                    originalInitialize.call(this, options);
                }
            };

            this.prototype._save = function () {

                var self = this;
                var promise = saveInternal.call(this);

                promise.done(function () {
                    if (self._onAfterSave) {
                        self._onAfterSave.call(self);
                    }

                    self.originalModelData = self.model.toJSON();
                });
            };

            this.prototype._saveAndAdd = function () {

                var self = this;
                var promise = saveInternal.call(this);

                promise.done(function () {
                    if (self._onAfterSaveAndAdd) {
                        self._onAfterSaveAndAdd.call(self);
                    }
                });
            };

            this.prototype._test = function () {
                var self = this;

                this.ui.indicator.show();

                this.model.test().always(function () {
                    self.ui.indicator.hide();
                });
            };

            this.prototype._delete = function () {
                var view = new this._deleteView({ model: this.model });
                AppLayout.modalRegion.show(view);
            };

            this.prototype.onBeforeClose = function () {
                this.model.set(this.originalModelData);

                if (originalOnBeforeClose) {
                    originalOnBeforeClose.call(this);
                }
            };

            return this;
        };
    }
);
