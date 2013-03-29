'use strict';

define(['app', 'Upcoming/UpcomingItemView'], function (app) {
    NzbDrone.Upcoming.UpcomingCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView         : NzbDrone.Upcoming.UpcomingItemView,
        template         : 'Upcoming/UpcomingCollectionTemplate',
        itemViewContainer: 'table',

        ui: {
            yesterday : 'tbody#yesterday',
            today     : 'tbody#today',
            tomorrow  : 'tbody#tomorrow',
            two_days  : 'tbody#two_days',
            three_days: 'tbody#three_days',
            four_days : 'tbody#four_days',
            five_days : 'tbody#five_days',
            six_days  : 'tbody#six_days',
            later     : 'tbody#later'
        },

        initialize: function () {
            this.collection = new NzbDrone.Upcoming.UpcomingCollection();
            this.collection.fetch();
        },

        serializeData: function () {
            var viewData = {};
            viewData.two_days = Date.create().addDays(2).format('{Weekday}');
            viewData.three_days = Date.create().addDays(3).format('{Weekday}');
            viewData.four_days = Date.create().addDays(4).format('{Weekday}');
            viewData.five_days = Date.create().addDays(5).format('{Weekday}');
            viewData.six_days = Date.create().addDays(6).format('{Weekday}');
            return viewData;
        },

        appendHtml: function (collectionView, itemView, index) {
            var date = Date.create(itemView.model.get('airTime'));

            if (date.isYesterday()) {
                collectionView.$(this.ui.yesterday).append(itemView.el);
                return;
            }

            if (date.isToday()) {
                collectionView.$(this.ui.today).append(itemView.el);
                return;
            }

            if (date.isTomorrow()) {
                collectionView.$(this.ui.tomorrow).append(itemView.el);
                return;
            }

            if (date.is(Date.create().addDays(2).short())) {
                collectionView.$(this.ui.two_days).append(itemView.el);
                return;
            }

            if (date.is(Date.create().addDays(3).short())) {
                collectionView.$(this.ui.three_days).append(itemView.el);
                return;
            }

            if (date.is(Date.create().addDays(4).short())) {
                collectionView.$(this.ui.four_days).append(itemView.el);
                return;
            }

            if (date.is(Date.create().addDays(5).short())) {
                collectionView.$(this.ui.five_days).append(itemView.el);
                return;
            }

            if (date.is(Date.create().addDays(6).short())) {
                collectionView.$(this.ui.six_days).append(itemView.el);
                return;
            }

            collectionView.$(this.ui.later).append(itemView.el);

            //if (date.isBefore(Date.create().addDays(7))) return date.format('{Weekday}');
        },

        onCompositeCollectionRendered: function () {
            //Might not need this :D
        }
    });
});