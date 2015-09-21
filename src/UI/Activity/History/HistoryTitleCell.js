var NzbDroneCell = require('../../Cells/NzbDroneCell');
var StatusModel = require('../../System/StatusModel');

module.exports = NzbDroneCell.extend({
    className : 'history-title-cell',

    render : function() {

        var series = this.model.get('series').get('title');
        var movie = this.model.get('movie').get('title');
        var title = '';
        var url = '';

        if (series) {
            title+=series;
            url+=StatusModel.get('urlBase') + '/series/' + this.model.get('series').get('titleSlug');
        } else {
            title+=movie;
            url+=StatusModel.get('urlBase') + '/movie/' + this.model.get('movie').get('cleanTitle');
        }

        this.$el.html('<a href="{0}">{1}</a>'.format(url, title));

/*        if (this.model.get('qualityCutoffNotMet')) {
            this.$el.html('<span class="badge badge-inverse" title="{0}">{1}</span>'.format(title, quality.quality.name));
        } else {
            this.$el.html('<span class="badge" title="{0}">{1}</span>'.format(title, quality.quality.name));
        }*/

        return this;
    }
});