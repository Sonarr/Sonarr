var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var Marionette = require('marionette');
var DeleteView = require('../Delete/IndexerDeleteView');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
require('../../../Form/FormBuilder');
require('../../../Mixins/AutoComplete');
require('bootstrap');

var view = Marionette.ItemView.extend({
    template : 'Settings/Indexers/Edit/IndexerEditViewTemplate',

    events : {
        'click .x-back'            : '_back',
        'click .x-captcha-refresh' : '_onRefreshCaptcha'
    },

    _deleteView : DeleteView,

    initialize : function(options) {
        this.targetCollection = options.targetCollection;
    },

    _onAfterSave : function() {
        this.targetCollection.add(this.model, { merge : true });
        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _onAfterSaveAndAdd : function() {
        this.targetCollection.add(this.model, { merge : true });

        require('../Add/IndexerSchemaModal').open(this.targetCollection);
    },

    _back : function() {
        if (this.model.isNew()) {
            this.model.destroy();
        }

        require('../Add/IndexerSchemaModal').open(this.targetCollection);
    },

    _onRefreshCaptcha : function(event) {
        var self = this;

        var target = $(event.target).parents('.input-group');

        this.ui.indicator.show();

        this.model.requestAction("checkCaptcha")
            .then(function(result) {
                if (!result.captchaRequest) {
                    self.model.setFieldValue('CaptchaToken', '');

                    return result;
                }

                return self._showCaptcha(target, result.captchaRequest);
            })
            .always(function() {
                self.ui.indicator.hide();
            });
    },

    _showCaptcha : function(target, captchaRequest) {
        var self = this;

        var widget = $('<div class="g-recaptcha"></div>').insertAfter(target);

        return this._loadRecaptchaWidget(widget[0], captchaRequest.siteKey, captchaRequest.secretToken)
            .then(function(captchaResponse) {
                target.parents('.form-group').removeAllErrors();
                widget.remove();

                var queryParams = {
                    responseUrl    : captchaRequest.responseUrl,
                    ray            : captchaRequest.ray,
                    captchaResponse: captchaResponse
                };

                return self.model.requestAction("getCaptchaCookie", queryParams);
            })
            .then(function(response) {
                self.model.setFieldValue('CaptchaToken', response.captchaToken);
            });
    },

    _loadRecaptchaWidget : function(widget, sitekey, stoken) {
        var promise = $.Deferred();

        var renderWidget = function() {
            window.grecaptcha.render(widget, {
              'sitekey'  : sitekey,
              'stoken'   : stoken,
              'callback' : promise.resolve
            });
        };

        if (window.grecaptcha) {
            renderWidget();
        } else {
            window.grecaptchaLoadCallback = function() {
                delete window.grecaptchaLoadCallback;
                renderWidget();
            };

            $.getScript('https://www.google.com/recaptcha/api.js?onload=grecaptchaLoadCallback&render=explicit')
             .fail(function() { promise.reject(); });
        }

        return promise;
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;