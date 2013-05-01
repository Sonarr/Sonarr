NzbDrone.Missing.Row = Backgrid.Row.extend({
    events: {
        'click .x-search'  : 'search'
    },

    search: function () {
        window.alert('Episode Search');
    }
});