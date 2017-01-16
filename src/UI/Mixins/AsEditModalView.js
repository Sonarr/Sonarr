var AppLayout = require('../AppLayout');

module.exports = function() {
    var originalInitialize = this.prototype.initialize;
    var originalOnBeforeClose = this.prototype.onBeforeClose;

    var saveInternal = function() {
        var self = this;

        if (this.saving) {
            return this.savePromise;
        }

        this.saving = true;
        this.ui.indicator.show();

        if (this._onBeforeSave) {
            this._onBeforeSave.call(this);
        }

        this.savePromise = this.model.save();

        this.savePromise.always(function() {
            self.saving = false;

            if (!self.isClosed) {
                self.ui.indicator.hide();
            }
        });

        this.savePromise.done(function() {
            self.originalModelData = JSON.stringify(self.model.toJSON());
        });

        return this.savePromise;
    };

    this.prototype.initialize = function(options) {
        if (!this.model) {
            throw 'View has no model';
        }

        this.testing = false;
        this.saving = false;

        this.originalModelData = JSON.stringify(this.model.toJSON());

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

    this.prototype._save = function() {
        var self = this;
        var promise = saveInternal.call(this);

        promise.done(function() {
            if (self._onAfterSave) {
                self._onAfterSave.call(self);
            }
        });
    };

    this.prototype._saveAndAdd = function() {
        var self = this;
        var promise = saveInternal.call(this);

        promise.done(function() {
            if (self._onAfterSaveAndAdd) {
                self._onAfterSaveAndAdd.call(self);
            }
        });
    };

    this.prototype._test = function() {
        var self = this;

        if (this.testing) {
            return;
        }

        this.testing = true;
        this.ui.indicator.show();

        this.model.test().always(function() {
            self.testing = false;
            self.ui.indicator.hide();
        });
    };

    this.prototype._delete = function() {
        var view = new this._deleteView({ model : this.model });
        AppLayout.modalRegion.show(view);
    };

    this.prototype.onBeforeClose = function() {
        this.model.set(JSON.parse(this.originalModelData));

        if (originalOnBeforeClose) {
            originalOnBeforeClose.call(this);
        }
    };

    return this;
};
