var vent = require('vent');
var AppLayout = require('../../../../AppLayout');
var Marionette = require('marionette');
var DeleteView = require('../Delete/DelayProfileDeleteView');
var AsModelBoundView = require('../../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../../Mixins/AsValidatedView');
var AsEditModalView = require('../../../../Mixins/AsEditModalView');
require('../../../../Mixins/TagInput');
require('bootstrap');

var view = Marionette.ItemView.extend({
    template : 'Settings/Profile/Delay/Edit/DelayProfileEditViewTemplate',

    _deleteView : DeleteView,

    ui : {
        tags         : '.x-tags',
        usenetDelay  : '.x-usenet-delay',
        torrentDelay : '.x-torrent-delay',
        protocol     : '.x-protocol'
    },

    events : {
        'change .x-protocol' : '_updateModel'
    },

    initialize : function(options) {
        this.targetCollection = options.targetCollection;
    },

    onRender : function() {
        if (this.model.id !== 1) {
            this.ui.tags.tagInput({
                model    : this.model,
                property : 'tags'
            });
        }

        this._toggleControls();
    },

    _onAfterSave : function() {
        this.targetCollection.add(this.model, { merge : true });
        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _updateModel : function() {
        var protocol = this.ui.protocol.val();

        if (protocol === 'preferUsenet') {
            this.model.set({
                enableUsenet      : true,
                enableTorrent     : true,
                preferredProtocol : 'usenet'
            });
        }

        if (protocol === 'preferTorrent') {
            this.model.set({
                enableUsenet      : true,
                enableTorrent     : true,
                preferredProtocol : 'torrent'
            });
        }

        if (protocol === 'onlyUsenet') {
            this.model.set({
                enableUsenet      : true,
                enableTorrent     : false,
                preferredProtocol : 'usenet'
            });
        }

        if (protocol === 'onlyTorrent') {
            this.model.set({
                enableUsenet      : false,
                enableTorrent     : true,
                preferredProtocol : 'torrent'
            });
        }

        this._toggleControls();
    },

    _toggleControls : function() {
        var enableUsenet = this.model.get('enableUsenet');
        var enableTorrent = this.model.get('enableTorrent');
        var preferred = this.model.get('preferredProtocol');

        if (preferred === 'usenet') {
            this.ui.protocol.val('preferUsenet');
        }

        else {
            this.ui.protocol.val('preferTorrent');
        }

        if (enableUsenet) {
            this.ui.usenetDelay.show();
        }

        else {
            this.ui.protocol.val('onlyTorrent');
            this.ui.usenetDelay.hide();
        }

        if (enableTorrent) {
            this.ui.torrentDelay.show();
        }

        else {
            this.ui.protocol.val('onlyUsenet');
            this.ui.torrentDelay.hide();
        }
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
AsEditModalView.call(view);

module.exports = view;