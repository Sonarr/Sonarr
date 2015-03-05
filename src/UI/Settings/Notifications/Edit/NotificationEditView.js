var vent = require('vent');
var Marionette = require('marionette');
var $ = require('jquery');
var Messenger = require('../../../Shared/Messenger');
var DeleteView = require('../Delete/NotificationDeleteView');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../Mixins/AsEditModalView');
require('../../../Form/FormBuilder');
require('../../../Mixins/TagInput');

var view = Marionette.ItemView.extend({
    template : 'Settings/Notifications/Edit/NotificationEditViewTemplate',

    ui : {
        onDownloadToggle : '.x-on-download',
        onUpgradeSection : '.x-on-upgrade',
        tags             : '.x-tags'
    },

    events : {
        'click .x-back'         : '_back',
        'change .x-on-download' : '_onDownloadChanged'
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

        if ('PushBullet' === this.model.get('implementation')) {
            this._pushbullet();
        }
    },

    _pushbullet : function() {
        var $pushbullet = this.$('.x-pushbullet').show();
        var $deviceIds = $pushbullet.find(".x-pushbullet-deviceIds");
        var $button = $pushbullet.find("button");
        var $deviceId = this.$("input[validation-name=DeviceId]");
        var $accessToken = this.$("input[validation-name=ApiKey]");

        function populateList(response) {
            // 'All devices' in not included in the response, so prepopulate the array
            var devices = [{name: 'All devices', id: ''}];

            response.devices
              .filter(function(v) { return v.pushable; })
              .forEach(function(v) {
                  devices.push({
                    name: v.nickname + (v.active ? "" : " (Inactive}"),
                    id: v.iden
                  });
              });

            var $options = devices.map(function(v) {
                return $("<option>").html(v.name).val(v.id);
            });

            $deviceIds.html($options).change();
        }

        function getDevices() {
            // Use the entered API key as access token
            var headers = { "Authorization": "Bearer " + $accessToken.val() };
            $.ajax({
                type: 'GET',
                url: 'https://api.pushbullet.com/v2/devices',
                headers: headers,
                success: populateList,
                error: function(xhr) {
                    var error = 'Unknown.';
                    if (xhr.responseJSON && xhr.responseJSON.error && xhr.responseJSON.error.message) {
                        error = xhr.responseJSON.error.message;
                    }

                    Messenger.show({
                        type    : 'error',
                        message : 'Could not retrieve devices IDs: ' + error
                    });
                }
            });
        }

        $button.click(getDevices);
        $deviceIds.change(function() {
            $deviceId.val(this.value);
        });
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
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;
