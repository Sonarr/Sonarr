var AppLayout = require('../../AppLayout');
var Marionette = require('marionette');
var EditProfileView = require('./Edit/EditLanguageProfileLayout');
var AsModelBoundView = require('../../Mixins/AsModelBoundView');
require('./Languagelabel');
require('bootstrap');

var view = Marionette.ItemView.extend({
    template : 'Settings/LanguageProfile/LanguageProfileViewTemplate',
    tagName  : 'li',

    ui : {
        "progressbar"  : '.progress .bar',
        "deleteButton" : '.x-delete'
    },

    events : {
        'click' : '_editProfile'
    },

    initialize : function() {
        this.listenTo(this.model, 'sync', this.render);
    },

    _editProfile : function() {
        var view = new EditProfileView({
            model             : this.model,
            profileCollection : this.model.collection
        });
        AppLayout.modalRegion.show(view);
    }
});

module.exports = AsModelBoundView.call(view);