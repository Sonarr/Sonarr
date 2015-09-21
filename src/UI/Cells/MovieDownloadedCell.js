var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'movie-downloaded-cell',

    render : function() {
        this.$el.empty();
        var monitored = this.model.get('monitored');
        var file = this.model.get('movieFileId') > 0;

        if (file) {
            this.$el.html('<span class="legend"><ul><li><span class="progress-bar-success"></span></li></ul></span>');
            this._setStatusWeight(2);
        }
        else {
            if (monitored) {
                this.$el.html('<span class="legend"><ul><li><span class="progress-bar-danger"></span></li></ul></span>');
                this._setStatusWeight(1);
            } else {
                this.$el.html('<span class="legend"><ul><li><span class="progress-bar-warning"></span></li></ul></span>');
                this._setStatusWeight(0);
            }
        }

        return this;
    },

    _setStatusWeight : function(weight) {
        this.model.set('statusWeight', weight, { silent : true });
    }
});