module.exports = function(){

    window.NzbDrone.NameViews = window.NzbDrone.NameViews || !window.NzbDrone.Production;

    var regex = new RegExp('/', 'g');

    var _getViewName = function(template){
        if(template) {
            return template.toLocaleLowerCase().replace('template', '').replace(regex, '-');
        }
        return undefined;
    };

    var originalOnRender = this.onRender;
    this.onRender = function(){

        if(window.NzbDrone.NameViews) {
            this.$el.addClass('iv-' + _getViewName(this.template));
        }

        if(originalOnRender) {
            return originalOnRender.call(this);
        }

        return undefined;
    };

    return this;
};