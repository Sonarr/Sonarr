var _ = require('underscore');
var $ = require('jquery');
var vent = require('vent');
var Marionette = require('marionette');
var DeleteView = require('../Delete/NotificationDeleteView');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
require('../../../Form/FormBuilder');
require('../../../Mixins/TagInput');
require('../../../Mixins/FileBrowser');
require('bootstrap.tagsinput');

var view = Marionette.ItemView.extend({
    template : 'Settings/Notifications/Edit/NotificationEditViewTemplate',

    ui : {
        onDownloadToggle             : '.x-on-download',
        onUpgradeSection             : '.x-on-upgrade',
        tags                         : '.x-tags',
        modalBody                    : '.x-modal-body',
        formTag                      : '.x-form-tag',
        path                         : '.x-path',
        authorizedNotificationButton : '.AuthorizeNotification'
    },

    events : {
        'click .x-back'         : '_back',
        'change .x-on-download' : '_onDownloadChanged',
        'click .AuthorizeNotification' : '_onAuthorizeNotification'
    },

    _deleteView : DeleteView,

    initialize : function(options) {
        this.targetCollection = options.targetCollection;
    },

    onRender : function() {
        this._onDownloadChanged();

        this.ui.tags.tagInput({
            model    : this.model,
            property : 'tags'
        });

        this.ui.formTag.tagsinput({
            trimValue : true,
            tagClass  : 'label label-default'
        });
    },

    onShow : function() {
        if (this.ui.path.length > 0) {
            this.ui.modalBody.addClass('modal-overflow');
        }

        this.ui.path.fileBrowser();
    },

    _onAfterSave : function() {
        this.targetCollection.add(this.model, { merge : true });
        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _onAfterSaveAndAdd : function() {
        this.targetCollection.add(this.model, { merge : true });

        require('../Add/NotificationSchemaModal').open(this.targetCollection);
    },

    _back : function() {
        if (this.model.isNew()) {
            this.model.destroy();
        }

        require('../Add/NotificationSchemaModal').open(this.targetCollection);
    },

    _onDownloadChanged : function() {
        var checked = this.ui.onDownloadToggle.prop('checked');

        if (checked) {
            this.ui.onUpgradeSection.show();
        } else {
            this.ui.onUpgradeSection.hide();
        }
    },

    _onAuthorizeNotification : function() {
        this.ui.indicator.show();

        var self = this;

        var promise = this.model.requestAction('startOAuth', { callbackUrl: window.location.origin + '/oauth.html' })
            .then(function(response) {
                return self._showOAuthWindow(response.oauthUrl);
            })
            .then(function(responseQueryParams) {
                return self.model.requestAction('getOAuthToken', responseQueryParams);
            })
            .then(function(response) {
                self.model.setFieldValue('AccessToken', response.accessToken);
                self.model.setFieldValue('AccessTokenSecret', response.accessTokenSecret);
            });
            
        promise.always(function() {
                self.ui.indicator.hide();
            });
    },
    
    _showOAuthWindow : function(oauthUrl) {
        var promise = $.Deferred();
    
        window.open(oauthUrl);
        var selfWindow = window;
        selfWindow.onCompleteOauth = function(query, callback) {
            delete selfWindow.onCompleteOauth;

            var queryParams = {};
            var splitQuery = query.substring(1).split('&');
            _.each(splitQuery, function (param) {
                var paramSplit = param.split('=');
                queryParams[paramSplit[0]] = paramSplit[1];
            });

            callback();

            promise.resolve(queryParams);
        };
        
        return promise;
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;