'use strict';

define(
    [
        'jquery',
        'vent',
        'Hotkeys/HotkeysView'
    ], function ($, vent, HotkeysView) {
        $(document).on('keydown', function (e) {
            if (e.ctrlKey && e.keyCode === 83) {
                vent.trigger(vent.Hotkeys.SaveSettings);
                e.preventDefault();
                return;
            }

            if (e.shiftKey && e.keyCode === 191) {
                vent.trigger(vent.Commands.OpenModalCommand, new HotkeysView());
//                vent.trigger(vent.Hotkeys.ShowHotkeys);
                e.preventDefault();
                return;
            }

            if ($(e.target).is('input') || $(e.target).is('textarea')) {
                return;
            }

            if (e.ctrlKey || e.metaKey || e.altKey) {
                return;
            }

            if (e.keyCode === 84) {
                vent.trigger(vent.Hotkeys.NavbarSearch);
                e.preventDefault();
            }
        });

    });
