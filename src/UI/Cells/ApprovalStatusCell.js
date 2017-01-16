var Backgrid = require('backgrid');
var Marionette = require('marionette');
require('bootstrap');

module.exports = Backgrid.Cell.extend({
    className : 'approval-status-cell',
    template  : 'Cells/ApprovalStatusCellTemplate',

    render : function() {

        var rejections = this.model.get(this.column.get('name'));

        if (rejections.length === 0) {
            return this;
        }

        this.templateFunction = Marionette.TemplateCache.get(this.template);

        var html = this.templateFunction(rejections);
        this.$el.html('<i class="icon-sonarr-form-danger"/>');

        this.$el.popover({
            content   : html,
            html      : true,
            trigger   : 'hover',
            title     : this.column.get('title'),
            placement : 'left',
            container : this.$el
        });

        return this;
    }
});