'use strict';
define(
    [
        'vent',
        'marionette',
        'Profile/ProfileCollection',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'Mixins/AutoComplete'
    ], function (vent, Marionette, Profiles, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Series/Edit/EditSeriesViewTemplate',

            ui: {
                profile: '.x-profile',
                path          : '.x-path'
            },

            events: {
                'click .x-save'  : '_saveSeries',
                'click .x-remove': '_removeSeries'
            },


            initialize: function () {
                this.model.set('profiles', Profiles);
            },

            _saveSeries: function () {

                var self = this;
                var profileId = this.ui.profile.val();
                this.model.set({ profileId: profileId});

                this.model.save().done(function () {
                    self.trigger('saved');
                    vent.trigger(vent.Commands.CloseModalCommand);
                });
            },

            onRender: function () {
                this.ui.path.autoComplete('/directories');
            },

            _removeSeries: function () {
                vent.trigger(vent.Commands.DeleteSeriesCommand, {series:this.model});
            }
        });


        AsModelBoundView.apply(view);
        return AsValidatedView.apply(view);
    });
