module.exports = (function(){
    window.onbeforeunload = function(){
        window.NzbDrone.unloading = true;
    };
}).call(this);