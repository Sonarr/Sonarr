var AppLayout = require('../../AppLayout');
var Marionette = require('marionette');
var EditProfileView = require('./Edit/EditProfileLayout');
var AsModelBoundView = require('../../Mixins/AsModelBoundView');
require('./AllowedLabeler');
require('./LanguageLabel');
require('bootstrap');

module.exports = (function(){
    var view = Marionette.ItemView.extend({
        template     : 'Settings/Profile/ProfileViewTemplate',
        tagName      : 'li',
        ui           : {
            "progressbar"  : '.progress .bar',
            "deleteButton" : '.x-delete'
        },
        events       : {"click" : '_editProfile'},
        initialize   : function(){
            this.listenTo(this.model, 'sync', this.render);
        },
        _editProfile : function(){
            var view = new EditProfileView({
                model             : this.model,
                profileCollection : this.model.collection
            });
            AppLayout.modalRegion.show(view);
        }
    });
    return AsModelBoundView.call(view);
}).call(this);