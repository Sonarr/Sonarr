var Marionette = require('marionette');
var NzbDroneCell = require('./NzbDroneCell');

module.exports = NzbDroneCell.extend({
    render : function() {

        var templateName = this.column.get('template') || this.template;

        this.templateFunction = Marionette.TemplateCache.get(templateName);
        this.$el.empty();

        if (this.cellValue) {
            var data = this.cellValue.toJSON();
            var html = this.templateFunction(data);
            this.$el.html(html);
        }

        this.delegateEvents();
        return this;
    }
});