//try to add ajax data as query string
(function (){

    var original = Backbone.ajax;

    Backbone.ajax = function (){

        var xhr = arguments[0];

        //check if ajax call was made with data option
        if(xhr && xhr.data && xhr.type=='DELETE')
        {
            if(xhr.url.indexOf('?') === -1)
            {
                xhr.url = xhr.url + '?' + $.param(xhr.data);
            }
            else
            {
                xhr.url = xhr.url + '&' + $.param(xhr.data);
            }
        }

        if (original){
            original.apply (this, arguments);
        }

    };
} ());

/*

var xhrMixin = function (){
    console.log ('mixing in xhr');

    var originalOnRender = Backbone.Marionette.View.prototype.onRender;
    var originalBeforeClose = Backbone.Marionette.View.prototype.beforeClose;

    Backbone.Marionette.View.prototype.onRender = function (){
        console.log ('render');
        if (originalOnRender){
            originalOnRender.call (this);
        }
    };
    Backbone.Marionette.View.prototype.beforeClose = function (){
        console.log ('beforeClose');
        if (originalBeforeClose){
            originalBeforeClose.call (this);
        }
    };
} ();*/
