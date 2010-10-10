/*
Copyright (c) 2010 Diego Uría Martínez

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

/**
 * Add a message to the body.
 * 
 * Example:
 *  $('a').click(function() {
 *      $.fn.jQueryMsg({
 *          msg: 'Hello world!!'
 *      });
 *  });
 *
 *  TODO:
 *  - don't set 'speed' too high, it may loose some events
 *  - option: message tag
 *  - option: content tag
 */
(function($,undefined){
    var name = 'jQueryMsg';
    var timeout;

    $.fn.jQueryMsg = function(params)
    {        
        var settings = $.extend(
        {},
        {
            msgClass : 'jquerymsgclass', //container class
            speed : 0, //effects' speed
            delay: 100, //delay between messages
            timeout: 3000, //maximum time the message is shown on the screen. 0 for permanent
            fx: 'none' //effect: set it to none, fade or slide
        },
        params);

        if(typeof(settings.msg) === 'string')
        {   
            var show = {width: 'show', height: 'show'};
            var hide = {width: 'hide', height: 'hide'};
            switch(settings.fx) {
                case 'fade':
                    show = {opacity: 'show'};
                    hide = {opacity: 'hide'};
                    break;
                case 'slide':
                    show = {height: 'show'};
                    hide = {height: 'hide'};
                    break;
            }
            
            var msg;
            if($('p.'+name).size() > 0) {
                msg = $('p.'+name);
                msg.click().delay(settings.delay);
            }
            else {
                msg = $('<p class="'+name+'"></p>');
                msg.hide().appendTo('body');
            }

            clearTimeout(timeout);
            
            msg.one('click',function() {
                msg.animate(hide, settings.speed, function() {
                    msg.removeClass().addClass(name);
                });
            }).queue(function() {
                msg.html(settings.msg).addClass(settings.msgClass).animate(show, settings.speed).dequeue();

               if(settings.timeout > 0) {
                    timeout = setTimeout(function() {
                        msg.click();
                    }, settings.timeout);
                }
            });
        }
    }
})(jQuery);