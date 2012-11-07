$(function () {
    // Legacy support for templating
    utils.loadTemplate(['QualityProfilesView', 'QualityProfileView'],
    function () {
        NzbDrone.App.start();
    });
});