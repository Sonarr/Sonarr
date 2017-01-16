var AppLayout = require('../AppLayout');
var Marionette = require('marionette');
var UpcomingCollectionView = require('./UpcomingCollectionView');
var CalendarView = require('./CalendarView');
var CalendarFeedView = require('./CalendarFeedView');
var ToolbarLayout = require('../Shared/Toolbar/ToolbarLayout');

module.exports = Marionette.Layout.extend({
    template : 'Calendar/CalendarLayoutTemplate',

    regions : {
        upcoming : '#x-upcoming',
        calendar : '#x-calendar',
        toolbar  : '#x-toolbar'
    },

    onShow : function() {
        this._showUpcoming();
        this._showCalendar();
        this._showToolbar();
    },

    _showUpcoming : function() {
        this.upcomingView = new UpcomingCollectionView();
        this.upcoming.show(this.upcomingView);
    },

    _showCalendar : function() {
        this.calendarView = new CalendarView();
        this.calendar.show(this.calendarView);
    },

    _showiCal : function() {
        var view = new CalendarFeedView();
        AppLayout.modalRegion.show(view);
    },

    _showToolbar    : function() {
        var leftSideButtons = {
            type       : 'default',
            storeState : false,
            items      : [
                {
                    title        : 'Get iCal Link',
                    icon         : 'icon-sonarr-calendar-o',
                    callback     : this._showiCal,
                    ownerContext : this
                }
            ]
        };

        var filterOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'calendar.show',
            defaultAction : 'monitored',
            items         : [
                {
                    key      : 'all',
                    title    : '',
                    tooltip  : 'All',
                    icon     : 'icon-sonarr-all',
                    callback : this._setCalendarFilter
                },
                {
                    key      : 'monitored',
                    title    : '',
                    tooltip  : 'Monitored Only',
                    icon     : 'icon-sonarr-monitored',
                    callback : this._setCalendarFilter
                }
            ]
        };

        this.toolbar.show(new ToolbarLayout({
            left          : [leftSideButtons],
            right         : [filterOptions],
            context       : this,
            floatOnMobile : true
        }));
    },

    _setCalendarFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        if (mode === 'all') {
            this.calendarView.setShowUnmonitored(true);
            this.upcomingView.setShowUnmonitored(true);
        }

        else {
            this.calendarView.setShowUnmonitored(false);
            this.upcomingView.setShowUnmonitored(false);
        }
    }
});