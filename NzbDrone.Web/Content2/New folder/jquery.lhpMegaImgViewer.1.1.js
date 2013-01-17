// Mega Image Viewer v1.1 - jQuery image viewer plugin - converting <div> element to an animated image viewer
// (c) 2012 lhp - http://codecanyon.net/user/lhp

/*
 * ----------------------------------------------------------------------------
 * settings:
 * viewportWidth                string (default: '100%'; accepted: string; Defines width of the area in which image will be displayed inside the outer div (myDiv). Size can be given in pixels, ems, percentages.)
 * viewportHeight               string (default: '100%'; accepted: string; Defines height of the area in which image will be displayed inside the outer div (myDiv). Size can be given in pixels, ems, percentages.)
 * startScale                   number (default: 1; accepted: 0...1; Defines start scale.)
 * startX                       number (default: 0; accepted: integer; Defines start coordinate x in px, in the display object frame of reference, which will be moved to the center of the viewport, if it is possible.)
 * startY                       number (defaabout:startpageult: 0; accepted: integer; Defines start coordinate y in px, in the display object frame of reference, which will be moved to the center of the viewport, if it is possible.)
 * animTime                     number (default: 500; accepted: integer; Defines duration in ms of the scale and position animations.)
 * draggInertia                 number (default: 10; accepted: integer; Defines inertia after dragging.)
 * contentUrl                   string (default: ''; accepted: string; Defines a path for an image source. This param is optional. Instead you can use the HTML image tag (see DOC -> STEP 2B - ACTIVATE THE PLUGIN (IMAGE SOURCE FROM HTML)).)
 * intNavEnable                 boolean (default: true; accepted: true, false; Defines the navigation bar enabled/disabled. )
 * intNavPos                    string (default: 'T'; accepted: 'TL', 'T', 'TR', 'L', 'R', 'BL', 'B', 'BR', false; Defines the navigation bar position. )
 * intNavAutoHide               boolean (default: false; accepted: true, false; Defines the navigation bar autohide. )
 * fitToViewportShortSide       boolean (default: false; accepted: true, false; Shorter side of the displayed object will fit the viewport. )
 * contentSizeOver100           boolean (default: false; accepted: true, false; If the viewport size (width and height) is greater than the size of the displayed object, allow the object scaled over 100% to fit the viewport (zoom is disabled). )
 * ----------------------------------------------------------------------------
 */
 
(function ($) {
    
    var pubMet, constSett, defaultSett;about:startpage
    
    constSett = {
        'dragSmooth' : 8
    };
    
    defaultSett = {
        'viewportWidth' : '100%',
        'viewportHeight' : '100%',
        'startScale' : 1,
        'startX' : 0,
        'startY' : 0,
        'animTime' : 500,
        'draggInertia' : 10,
        'contentUrl' : '',
        'intNavEnable' : true,
        'intNavPos' : 'T',
        'intNavAutoHide' : false,
        'fitToViewportShortSide' : false,  
        'contentSizeOver100' : false
    };

    pubMet = {
        init : function (options) {
            
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV'), interImgs = $t.find('img'), sett = {};
                $.extend(sett, defaultSett, options);
                $.extend(sett, constSett);
                
                if (!data) {
                    
                    if(sett.draggInertia < 0) {
                        sett.draggInertia = 0;
                    }
                    
                    sett.animTime = parseInt(sabout:startpageett.animTime);
                    if(sett.animTime < 0) {
                        sett.animTime = 0;
                    }
                    
                    /*img tag*/
                    if(interImgs.length > 0) {
                        sett.contentUrl = interImgs[0].src;
                        interImgs.remove();
                    }
                    
                    $t.data('lhpMIV', {});
                    $t.data('lhpMIV').interImgsTmp = interImgs;
                    $t.data('lhpMIV').lc = new LocationChanger(sett, $t);
                }
            });
        },
        /*
        * Sets the position and size of the displayed object. The second parameter is optional - if empty, the size remains unchanged.
        * @param {integer} x Coordinate x in px, in the display object frame of reference, which will be moved to the center of the viewport (if it is possible).
        * @param {integer} y Coordinate y in px, in the display object frame of reference, which will be moved to the center of the viewport (if it is possible).
        * @param {number} scale The size to which the display object will be scaled (if it is possible); optional.
        * @return {Object} Returns jQuery object.
        */
        setPosition : function (x, y, scale) {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.setProperties(x, y, scale);
                }
            });
        },
        /*about:startpage
        * Initializes the movement of the display object to the top, to the boundary of the viewport or untill the moveStop method is called.
        * @return {Object} Returns jQuery object.
        */
        moveUp : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.beginDirectMove('U');
                }
            });
        },
        /*
        * Initializes the movement of the display object to the bottom, to the boundary of the viewport or untill the moveStop method is called.
        * @return {Object} Returns jQuery object.
        */
        moveDown : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.beginDirectMove('D');
                }
            });
        },
        /*
        * Initializes the movement of the display object to the left, to the boundary of the viewport or untill the moveStop method is called.
        * @return {Object} Returns jQuery object.
        */
        moveLeft : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.beginDirectMove('L');
                }
            });
        },
        /*
        * Initializes the movement of the display object to the right, to the boundary of the viewport or untill the moveStop method is called.
        * @return {Object} Returns jQuery object.about:startpage
        */
        moveRight : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.beginDirectMove('R');
                }
            });
        },
        /*
        * Stops the movement of the display object.
        * @return {Object} Returns jQuery object.
        */
        moveStop : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.stopDirectMoving();
                }
            });
        },
        /*
        * Initializes the zooming of the display object up to 100% or untill the zoomStop method is called.
        * @return {Object} Returns jQuery object.
        */
        zoom : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.beginZooming('Z');
                }
            });
        },
        /*
        * Initializes the unzooming of the display object up to the viewport's size or untill the zoomStop method is called.about:startpage
        * @return {Object} Returns jQuery object.
        */
        unzoom : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.beginZooming('U');
                }
            });
        },
        /*
        * Stops the zooming/unzooming of the display object.
        * @return {Object} Returns jQuery object.
        */
        zoomStop : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.stopZooming();
                }
            });
        },
        /*
        * Fits the display obejct's size to the viewport size and moves the object to the center of the viewport.
        * @return {Object} Returns jQuery object.
        */
        fitToViewport : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.setProperties(null, null, 0.0001);
                }
            });
        },
        /*
        * Sets the initial size of the display object anabout:startpaged moves the object to the center of the viewport.
        * @return {Object} Returns jQuery object.
        */
        fullSize : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.setProperties(null, null, 1);
                }
            });
        },
        /*
        * Control the correct position and size of the object displayed inside the viewport.
        * @return {Object} Returns jQuery object.
        */
        adaptsToContainer : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.adaptsToContainer();
                }
            });
        },
        /*
        * Destructor. Removes the viewer from the page. Restores the original appearance and functionality of the outer <div> element. Allows to efficiently clean the memory.
        * @return {Object} Returns jQuery object.
        */
        destroy : function () {
            return this.each(function () {
                var $t = $(this), data = $t.data('lhpMIV');
                if (data) {
                    $t.data('lhpMIV').lc.destroy();
                    $t.prepend($t.data('lhpMIV').interImgsTmp);
                    $t.removeData('lhpMIV');
                }
            });about:startpage
        }
    };
    
    $.fn.lhpMegaImgViewer = function (method) {
        if (pubMet[method]) {
            return pubMet[method].apply(this, Array.prototype.slice.call(arguments, 1));
        } else if (typeof method === 'object' || !method) {
            return pubMet.init.apply(this, arguments);
        } else {
            $.error('Method ' +  method + ' does not exist on jQuery.lhpMegaImgViewer');
        }
    };
    
    /*location changer*/
    var LocationChanger = function (sett, $mainHolder) {
        this.isTouchDev = (typeof(window.ontouchstart) != 'undefined') ? true : false;
        this.sett = sett;
        this.$mainHolder = $mainHolder;
        this.lastMousePageCoor = this.lastDrag = this.contentFullSize = {};
        this.$mivHol = this.$contentHol = this.$content = null;
        this.$preloadHol = this.$blackScreen = this.$navHol = null;
        this.movingIntreval = this.movingDirectIntreval = this.navAutohideInterval = null;
        this.speedX = this.speedY = null;
        this.targetX = this.targetY = null;
        this.allow = {allowDown : null, allowUp : null, allowLeft : null, allowRight : null, allowZoom : null, allowUnzoom : null};
        this.sm = new ScaleManager();

        this.createHolders();
        
        /*load content*/
        this.contentLoader = new LoaderImgContent(this.sett.contentUrl, this.$contentHol, function(that) { 
            return function($content) {
                that.imgContentStart($content);
            }about:startpage
        }(this));
        /**/
    };
    //initialization
    LocationChanger.prototype.createHolders = function () {
        this.$mivHol = $('<div />')
        .addClass('lhp_miv_holder')
        .css({position : 'relative', overflow : 'hidden', width : this.sett.viewportWidth, height : this.sett.viewportHeight});
        
        this.$preloadHol = $('<div />')
        .addClass('lhp_miv_preload_holder');
        
        this.$contentHol = $('<div />')
        .addClass('lhp_miv_content_holder')
        .css({position : 'absolute'});
        
        this.$blackScreen = $('<div />')
        .addClass('lhp_miv_blackScreen')
        .css({position : 'absolute', 'z-index' : '1', width : '100%', height : '100%', background : '#ffffff'});
        
        this.$mivHol.append(this.$preloadHol);
        this.$mivHol.append(this.$blackScreen);
        this.$mivHol.append(this.$contentHol);
        this.$mainHolder.append(this.$mivHol);
    }
    LocationChanger.prototype.iniNav = function () {
        var $ul = $('<ul />').addClass('ui-widget ui-helper-clearfix'),
        $mainHolder = this.$mainHolder,
        $navHol = this.$navHol,
        $li, $span,
        _this = this,
        btt = [['moveDown', 'moveStop', 'ui-icon-carat-1-n'], ['moveUp', 'moveStop', 'ui-icon-carat-1-s'], ['moveRight', 'moveStop', 'ui-icon-carat-1-w'], ['moveLeft', 'moveStop', 'ui-icon-carat-1-e'],
               ['zoom', 'zoomStop', 'ui-icon-zoomin'], ['unzoom', 'zoomStop', 'ui-icon-zoomout'], ['fitToViewport', null, 'ui-icon-stop'], ['fullSize', null, 'ui-icon-arrow-4-diag']];
               
        $.each(btt, function(i) {
            mousedownFunc = btt[i][0],
            mouseupFunc = btt[i][1],
            $li = $('<li />').addClass('ui-state-default ui-corner-all ' + mousedownFunc);
            $span = $('<span />').addClass('ui-icon ' + btt[i][2]);
            
            $li.append($span);
            $ul.append($li);
            
            $li.bind('mouseenter.lhpMIV touchstart.lhpMIV', function() { 
                if(!$(this).hasClass('lhp_miv_nav_btt_disab')) {
                    $(this).addClass('ui-state-hover');
                }
            });
            
            $li.bind('mouseleave.lhpMIV touchend.lhpMIV', function() { 
                $(this).removeClass('ui-state-hover');
            });
            
            $li.bind('mousedown.lhpMIV touchstart.lhpMIV', function(func) { 
                return function(e) { 
                    if(!$(this).hasClass('lhp_miv_nav_btt_disab')) {
                        $mainHolder.lhpMegaImgViewer(func);
                    }
                    e.preventDefault();
                } 
            }(mousedownFunc));
            
            if(mouseupFunc) {
                $li.bind('mouseup.lhpMIV touchend.lhpMIV', function(func) { 
                    return function(e) { 
                        if(!$(this).hasClass('lhp_miv_nav_btt_disab')) {
                            $mainHolder.lhpMegaImgViewer(func);
                        }
                        e.preventDefault();
                    } 
                }(mouseupFunc));
            }
        });
        
        $mainHolder.bind('mivChange.lhpMIV', function(e) {
            
            var c1 = 'lhp_miv_nav_btt_disab', c2 = 'ui-state-hover';
            
            if(e.allowDown) {
                $navHol.find('.moveDown').removeClass(c1);
            } else {
                $navHol.find('.moveDown').removeClass(c2).addClass(c1);
            }
            
            if(e.allowUp) {
                $navHol.find('.moveUp').removeClass(c1);
            } else {
                $navHol.find('.moveUp').removeClass(c2).addClass(c1);
            }
            
            if(e.allowLeft) {
                $navHol.find('.moveLeft').removeClass(c1);
            } else {
                $navHol.find('.moveLeft').removeClass(c2).addClass(c1);
            }
            
            if(e.allowRight) {
                $navHol.find('.moveRight').removeClass(c1);
            } else {
                $navHol.find('.moveRight').removeClass(c2).addClass(c1);
            }
            
            if(e.allowZoom) {
                $navHol.find('.zoom').removeClass(c1);
                $navHol.find('.fullSize').removeClass(c1);
            } else {
                $navHol.find('.zoom').removeClass(c2).addClass(c1);
                $navHol.find('.fullSize').removeClass(c2).addClass(c1);
            }
            
            if(e.allowUnzoom) {
                $navHol.find('.unzoom').removeClass(c1);
                $navHol.find('.fitToViewport').removeClass(c1);
            } else {
                $navHol.find('.unzoom').removeClassabout:startpage(c2).addClass(c1);
                $navHol.find('.fitToViewport').removeClass(c2).addClass(c1);
            }

        });
        
        if(this.sett.intNavAutoHide) {
            $navHol.css('display', 'none');
            $mainHolder.bind('mouseenter.lhpMIV touchstart.lhpMIV', function() { 
                clearInterval(_this.navAutohideInterval);
                $navHol.fadeIn('fast'); 
            });
            $mainHolder.bind('mouseleave.lhpMIV touchend.lhpMIV', function() { 
                clearInterval(_this.navAutohideInterval);
                _this.navAutohideInterval = setInterval(function($navHol){
                    return function () {
                        $navHol.stop().clearQueue().fadeOut('fast');
                    };
                }($navHol), 1000);
            });
        }
        
        $navHol.append($ul);
    }
    LocationChanger.prototype.imgContentStart = function ($content) {
        this.$content = $content;
        $content.addClass('lhp_miv_content').css({'float' : 'left'});
        this.contentFullSize = {'w' : $content.width(), 'h' : $content.height()};
        this.start();
        this.$blackScreen.animate({ opacity : 0 }, { duration : 500, complete : function(){ $(this).remove(); }});about:startpage
    }
    LocationChanger.prototype.start = function () {
        
        if(this.sett.intNavEnable) {
            this.$navHol = $('<div class="lhp_miv_nav"/>').addClass('lhp_miv_nav_pos_' + this.sett.intNavPos);
            this.iniNav();
            this.$mivHol.prepend(this.$navHol);
        }
        
        this.$preloadHol.remove();
        
        this.$contentHol.bind('mouseenter.lhpMIV', {'_this' : this}, this.mouseenterHandler);
        if(this.isTouchDev) {
            this.$contentHol.bind('touchstart.lhpMIV', {'_this' : this}, this.mousedownHandler);
        } else {
            this.$contentHol.bind('mousedown.lhpMIV', {'_this' : this}, this.mousedownHandler);
            this.$contentHol.bind('mouseup.lhpMIV', {'_this' : this}, this.mouseupHandler);
            this.$contentHol.bind('mouseleave.lhpMIV', {'_this' : this}, this.mouseupHandler);
        }
        this.$contentHol.bind('mousewheel.lhpMIV', {'_this' : this}, this.mousewheelHandler);
        this.setProperties(this.sett.startX, this.sett.startY, this.sett.startScale, true);
    }   
    LocationChanger.prototype.destroy = function () {
        /*clear content*/
        this.contentLoader.dispose();
        
        /*clear callback*/
        this.animStop();
        this.stopMoving();
        this.stopDirectMoving();
        
        /*clear nav handler*/
        if(this.$navHol) {
            this.$navHol.find('li').each(function(i) {
                $(this).unbind();
            });
        }

        /*clear handler*/
        this.$mainHolder.unbind('.lhpMIV');
        this.$contentHol.unbind();
        
        /*clear holders*/
        this.$mivHol.remove();

        /*clear properties*/
        $.each(this, function(k, v) {
            if(!$.isFunction(v)) {
                k = null;
            }
        });
    }
    //mouse handlers
    LocationChanger.prototype.mousePageCoor = function (e) {
        var r = {x : e.pageX, y : e.pageY};
        e = e.originalEvent;
        if(this.isTouchDev && e) {
            r.x = e.changedTouches[0].pageX; 
            r.y = e.changedTouches[0].pageY;
        }
        return r;
    }
    LocationChanger.prototype.mouseenterHandler = function (e) {
        e.data._this.$contentHol.css('cursor', 'url(cursorHand.png),default');
    }
    LocationChanger.prototype.mousedownHandler = function (e) {
        var _this = e.data._this;
        
        _this.animStop(true);
        _this.stopMoving();
        _this.stopDirectMoving();
        
        if(_this.isTouchDev) {
            _this.$contentHol.unbind('touchmove.lhpMIV', _this.mousemoveHandler).bind('touchmove.lhpMIV', {'_this' : _this}, _this.mousemoveHandler);
            _this.$contentHol.unbind({'touchend.lhpMIV' : _this.positioning}).bind('touchend.lhpMIV' , {'_this' : _this}, _this.positioning);
        } else {
            _this.$contentHol.unbind('mousemove.lhpMIV', _this.mousemoveHandler).bind('mousemove.lhpMIV', {'_this' : _this}, _this.mousemoveHandler);
            _this.$contentHol.unbind({'mouseup.lhpMIV' : _this.positioning}).bind('mouseup.lhpMIV' , {'_this' : _this}, _this.positioning);
        }
        
        _this.lastMousePageCoor = _this.mousePageCoor(e);
        _this.$contentHol.css('cursor', 'url(cursorDrag.png), move');
        e.preventDefault();
    }
    LocationChanger.prototype.mousemoveHandler = function (e) {
        var _this = e.data._this;

        if(_this.isTouchDev) {
            _this.$contentHol.unbind({'touchend.lhpMIV' : _this.positioning});
            _this.$contentHol.unbind({'touchend.lhpMIV' : _this.stopDraggingHandler}).bind('touchend.lhpMIV' , {'_this' : _this}, _this.stopDraggingHandler);
        } else {
            _this.$contentHol.unbind('mouseup.lhpMIV', _this.positioning);
            _this.$contentHol.unbind({'mouseup.lhpMIV' : _this.stopDraggingHandler}).bind('mouseup.lhpMIV' , {'_this' : _this}, _this.stopDraggingHandler);
            _this.$contentHol.unbind({'mouseleave.lhpMIV' : _this.stopDraggingHandler}).bind('mouseleave.lhpMIV' , {'_this' : _this}, _this.stopDraggingHandler);
        }

        _this.dragging(e, 'hard');
        e.preventDefault();
    }
    LocationChanger.prototype.mouseupHandler = function (e) {
        var _this = e.data._this;
        _this.$contentHol.unbind('mousemove.lhpMIV', _this.mousemoveHandler);
        _this.$contentHol.unbind('mouseup.lhpMIV', _this.positioning);
        _this.$contentHol.css('cursor', 'url(cursorHand.png),default');
    }
    LocationChanger.prototype.stopDraggingHandler = function (e) {
        var _this = e.data._this;
        _this.$contentHol.unbind({'mouseup.lhpMIV' : _this.stopDraggingHandler});
        _this.$contentHol.unbind({'mouseleave.lhpMIV' : _this.stopDraggingHandler});
        _this.dragging(e, 'inertia');
    }
    LocationChanger.prototype.mousewheelHandler = function (e, delta) {
        var _this = e.data._this,
        newScale = (delta > 0) ? _this.sm.nextScale() : _this.sm.prevScale(),
        newProp = _this.calculateScale(e, newScale);
        
        _this.animStop();
        _this.stopMoving();
        _this.stopDirectMoving();
        _this.animSizeAndPos(newProp.x, newProp.y, newProp.w, newProp.h);
        return false;
    }
    //changers
    LocationChanger.prototype.adaptsToContainer = function () {
        if(this.$content) {
            var iterimScale =  this.$content.width() / this.contentFullSize.w;
            iterimScale = (iterimScale > 1) ? 1 : iterimScale;
            
            this.animStop();
            this.stopMoving();
            this.stopDirectMoving();
            this.setProperties(null, null, iterimScale, true);
        }
    }
    LocationChanger.prototype.beginZooming = function (t) {
        var delta = (t == 'Z') ? 1 : -1,
        data = {_this : this},
        mivCenter = {'x' : (this.$mivHol.width() / 2), 'y' : (this.$mivHol.height() / 2)},
        mivHolOff = this.$mivHol.offset(),
        mouseGivCenter = {'x' : (mivCenter.x + mivHolOff.left), 'y' : (mivCenter.y + mivHolOff.top)},
        e = {data : data, pageX : mouseGivCenter.x, pageY : mouseGivCenter.y }; //pseudo event object
        
        this.animStop(true);
        this.stopMoving();
        this.stopDirectMoving();

        if(!this.movingIntreval) {
            this.movingIntreval = setInterval(function(_this, e, delta){
                    return function () {
                        _this.zooming(e, delta);
                    };
            }(this, e, delta), this.sett.animTime / 5);
        }
        
        this.zooming(e, delta);
    }
    LocationChanger.prototype.zooming = function (e, delta) {
        var newScale = (delta > 0) ? this.sm.nextScale() : this.sm.prevScale(),
        newProp = this.calculateScale(e, newScale);
        
        this.animStop();
        this.animSizeAndPos(newProp.x, newProp.y, newProp.w, newProp.h);
        
        if(this.sett.fitToViewportShortSide) {
            if(newScale >= 1 || newProp.w <= this.$mivHol.width() || newProp.h <= this.$mivHol.height()) {
                this.stopZooming();
            }
        } else {
            if(newScale >= 1 || (newProp.w <= this.$mivHol.width() && newProp.h <= this.$mivHol.height())) {
                this.stopZooming();
            }
        }
    }
    LocationChanger.prototype.stopZooming = function () {
        this.stopMoving();
    }
    LocationChanger.prototype.beginDirectMove = function (direct) {
        this.animStop(true);
        this.stopMoving();
        this.sm.setScale(this.$content.width() / this.contentFullSize.w);
        this.speedX = this.speedY = 0;
        
        switch(direct) {
            case 'U':
                this.speedY = -50000 / this.sett.animTime;
                break;
            case 'D':
                this.speedY = 50000 / this.sett.animTime;
                break;
            case 'L':
                this.speedX = -50000 / this.sett.animTime;
                break;
            case 'R':
                this.speedX = 50000 / this.sett.animTime;
                break;
        }
        
        if(!this.movingDirectIntreval && (this.speedX || this.speedY)) {
            this.movingDirectIntreval = setInterval(function(_this){
                    return function () {
                        _this.directMoveWithInertia();
                    };
            }(this), 10);
        }
    }
    LocationChanger.prototype.directMoveWithInertia = function () {
        var holLeft = this.$contentHol.position().left,
        holTop = this.$contentHol.position().top,
        targetX = Math.ceil(holLeft + this.speedX),
        targetY = Math.ceil(holTop + this.speedY);
        
        if(!this.movingIntreval) {
            this.movingIntreval = setInterval(function(_this){
                    return function () {
                        _this.moveWithInertia();
                    };
            }(this), 10);
        }
        
        safeTarget = this.getSafeTarget(targetX, targetY, this.speedX, this.speedY);
        
        this.targetX = Math.round(safeTarget.x); 
        this.targetY = Math.round(safeTarget.y);
    }
    LocationChanger.prototype.stopDirectMoving = function () {
        clearInterval(this.movingDirectIntreval);
        this.movingDirectIntreval = null;
    }
    LocationChanger.prototype.dragging = function (e, type) {
        var draggIner = this.sett.draggInertia,
        mousePageCoor = this.mousePageCoor(e),
        draggShftX = mousePageCoor.x - this.lastMousePageCoor.x,
        draggShftY = mousePageCoor.y - this.lastMousePageCoor.y;
        
        if(type == 'inertia' && this.lastDragg) {
            this.draggingWithInertia(this.lastDragg.x * draggIner, this.lastDragg.y * draggIner);
        } else {
            this.draggingHard(draggShftX, draggShftY);
        }
        
        this.lastDragg = {x : (Math.abs(draggShftX) < 5) ? 0 : draggShftX, 
                           y : (Math.abs(draggShftY) < 5) ? 0 : draggShftY};
        
        this.lastMousePageCoor = mousePageCoor;
    }
    LocationChanger.prototype.draggingHard = function (draggShftX, draggShftY) {
        var contentHolPos = this.$contentHol.position()
        targetX = contentHolPos.left + draggShftX,
        targetY = contentHolPos.top + draggShftY,
        safeTarget = this.getSafeTarget(targetX, targetY, draggShftX, draggShftY);
        
        this.animStop();
        this.$contentHol.css({'left' : safeTarget.x, 'top' : safeTarget.y});
    }
    LocationChanger.prototype.draggingWithInertia = function (draggShftX, draggShftY) {
        var targetX = this.targetX + draggShftX,
        targetY = this.targetY + draggShftY,
        safeTarget;
        
        if(!this.movingIntreval) {
            this.movingIntreval = setInterval(function(_this){
                    return function () {
                        _this.moveWithInertia();
                    };
            }(this), 10);
            targetX = this.$contentHol.position().left + draggShftX;
            targetY = this.$contentHol.position().top + draggShftY;
        }
        
        safeTarget = this.getSafeTarget(targetX, targetY, draggShftX, draggShftY);
        
        this.targetX = Math.round(safeTarget.x); 
        this.targetY = Math.round(safeTarget.y);
    }
    LocationChanger.prototype.getSafeTarget = function (targetX, targetY, moveDirectX, moveDirectY) {
        var limits = this.getLimit(this.sm.getScale()),
        xMin = limits.xMin,
        xMax = limits.xMax,
        yMin = limits.yMin,
        yMax = limits.yMax,
        mivHolW = this.$mivHol.width(),
        mivHolH = this.$mivHol.height(),
        mivHolCentX = mivHolW/2,
        mivHolCentY = mivHolH/2,
        newContentW = this.contentFullSize.w * this.sm.getScale(),
        newContentH = this.contentFullSize.h * this.sm.getScale();
        
        /*Y*/
        if((moveDirectY < 0) && (targetY < yMin)) { //move up limit
            targetY = yMin;
        } else if((moveDirectY > 0) && (targetY > yMax)) { // move down limit
            targetY = yMax;
        }
        
        if (newContentH < mivHolH)
        {
            targetY = mivHolCentY - newContentH / 2;
        }
        
        /*X*/
        if((moveDirectX < 0) && (targetX < xMin)) { //move left limit
            targetX = xMin;
        } else if((moveDirectX > 0) && (targetX > xMax)) { //move right limit 
            targetX = xMax;
        }
        
        if (newContentW < mivHolW)
        {
            targetX = mivHolCentX - newContentW / 2;
        }
        
        return {x : targetX, y : targetY};
    }
    LocationChanger.prototype.moveWithInertia = function () {
        var contentHolPos = this.$contentHol.position(),
        damping = this.sett.dragSmooth,
        distX, distY;
        
        contentHolPos.left = Math.ceil(contentHolPos.left);
        contentHolPos.top = Math.ceil(contentHolPos.top);
        distX = (this.targetX - contentHolPos.left)/damping;
        distY = (this.targetY - contentHolPos.top)/damping;
        
        if(Math.abs(distX) < 1) {
            distX = (distX > 0) ? 1 : -1
        }
        
        if(Math.abs(distY) < 1) {
            distY = (distY > 0) ? 1 : -1
        }
        
        if(contentHolPos.left == this.targetX) {
            distX = 0;
        }
        
        if(contentHolPos.top == this.targetY) {
            distY = 0;
        }
        
        this.$contentHol.css({'left' : contentHolPos.left + distX, 'top' : contentHolPos.top + distY});
        this.dispatchEventChange();
        
        if(contentHolPos.left == this.targetX && contentHolPos.top == this.targetY) {
            this.stopDirectMoving();
            this.stopMoving();
        }
    }
    LocationChanger.prototype.stopMoving = function () {
        clearInterval(this.movingIntreval);
        this.movingIntreval = null;
    }
    LocationChanger.prototype.positioning = function (e) {
        var _this = e.data._this, 
        newProp = _this.calculatePosInCenter(e);
        
        _this.animStop();
        _this.stopMoving();
        _this.stopDirectMoving();
        _this.animSizeAndPos(newProp.x, newProp.y);
    }
    LocationChanger.prototype.setProperties = function (x, y, scale, noAnim) {
        if(this.$content) {
            var data = {_this : this},
            mivCenter = {'x' : (this.$mivHol.width() / 2), 'y' : (this.$mivHol.height() / 2)},
            mivHolOff = this.$mivHol.offset(),
            mouseGivCenter = {'x' : (mivCenter.x + mivHolOff.left), 'y' : (mivCenter.y + mivHolOff.top)},
            e = {data : data, pageX : mouseGivCenter.x, pageY : mouseGivCenter.y }, //pseudo event object
            contentHolPos = this.$contentHol.position(),
            newProp, iterimScale,
            newX = contentHolPos.left, newY = contentHolPos.top, newW = this.$content.width(), newH = this.$content.height();
            
            x = parseFloat(x); 
            y = parseFloat(y); 
            scale = parseFloat(scale);
            
            if(!isNaN(scale)) {
                if(scale > 1) {
                    scale = 1;
                }
                newProp = this.calculateScale(e, scale);
                newX = newProp.x;
                newY = newProp.y;
                newW = newProp.w;
                newH = newProp.h;
            }
            
            iterimScale =  newW / this.contentFullSize.w;
            
            if(!isNaN(x)) {
                newX = -(x * iterimScale) + mivCenter.x;
            }
            
            if(!isNaN(y)) {
                newY = -(y * iterimScale) + mivCenter.y;
            }
            
            this.animStop();
            this.stopMoving();
            this.stopDirectMoving();
            this.animSizeAndPos(newX, newY, newW, newH, noAnim);
        }
    }
    LocationChanger.prototype.calculatePosInCenter = function (e) {
        var contentHolPos = this.$contentHol.position(),
        mivHolOff = this.$mivHol.offset(),
        mivCenter = {'x' : (this.$mivHol.width() / 2), 'y' : (this.$mivHol.height() / 2)},
        mousePageCoor = this.mousePageCoor(e),
        mouseHolCoor = {'x' : (mousePageCoor.x - mivHolOff.left), 'y' : (mousePageCoor.y - mivHolOff.top)},
        shftX, shftY,
        newX, newY;
        
        shftX = mivCenter.x - mouseHolCoor.x;
        shftY = mivCenter.y - mouseHolCoor.y;
        newX = contentHolPos.left + shftX;
        newY = contentHolPos.top + shftY;
        
        return {x : newX, y : newY, 'shftX' : shftX, 'shftY' : shftY};
    }
    LocationChanger.prototype.calculateScale = function (e, newScale) {
        var mivHolOff = this.$mivHol.offset(),
        contentOff = this.$content.offset(),
        mousePageCoor = this.mousePageCoor(e), 
        iterimScale,
        mouseContentCoor,
        newX, newY, newW, newH;
        
        newScale = this.getSafeScale(newScale);
        this.sm.setScale(newScale);
        iterimScale =  this.$content.width() / this.contentFullSize.w;
        
        mouseContentCoor = {'x' : (mousePageCoor.x - contentOff.left) / iterimScale,
                            'y' : (mousePageCoor.y - contentOff.top) / iterimScale};
                            
        newW = Math.round(this.contentFullSize.w * newScale);
        newH = Math.round(this.contentFullSize.h * newScale);
        newX = Math.round(contentOff.left - mivHolOff.left + mouseContentCoor.x * (iterimScale - newScale));
        newY = Math.round(contentOff.top - mivHolOff.top + mouseContentCoor.y * (iterimScale - newScale));
        
        return {x : newX, y : newY, w : newW, h : newH};
    }
    LocationChanger.prototype.getSafeScale = function (newScale) {
        var safeScale = (newScale <= 0) ? 0.00001 : newScale, 
        mivHolW = this.$mivHol.width(),
        mivHolH = this.$mivHol.height(),
        defContentW = this.contentFullSize.w,
        defContentH = this.contentFullSize.h,
        newContentW = defContentW * safeScale,
        newContentH = defContentH * safeScale,
        horScale = mivHolW/defContentW, 
        verScale = mivHolH/defContentH,
        mivHolProp = mivHolW/mivHolH, //viewport proportion; p < 1 -  vertical; p > 1 - horizontal
        contentProp = newContentW/newContentH; //content proportion
            
        if (!this.sett.contentSizeOver100 && defContentW <= mivHolW && defContentH <= mivHolH)
        {
            safeScale = 1;
            return safeScale;
        }
        
        if(this.sett.fitToViewportShortSide) {
            if(newContentW < mivHolW || newContentH < mivHolH) {
                horScale = mivHolW / this.contentFullSize.w; 
                verScale = mivHolH / this.contentFullSize.h; 
                safeScale = Math.max(horScale, verScale);
            }
        } else {
            if (newContentW < mivHolW && newContentH < mivHolH) {
                if (contentProp <= mivHolProp)
                    safeScale = verScale;
                else
                    safeScale = horScale;
            }
        }

        return safeScale;   
    }
    LocationChanger.prototype.getLimit = function (inScale) {
        var xMin = -(Math.round(this.contentFullSize.w * inScale) - this.$mivHol.width()),
        yMin = -(Math.round(this.contentFullSize.h * inScale) - this.$mivHol.height());
        return {'xMin' : xMin, 'xMax' : 0, 'yMin' : yMin, 'yMax' : 0};
    }
    LocationChanger.prototype.getSafeXY = function (x, y, inScale) {
        var limits = this.getLimit(inScale),
        mivHolW = this.$mivHol.width(),
        mivHolH = this.$mivHol.height(),
        mivHolCentX = mivHolW/2,
        mivHolCentY = mivHolH/2,
        defContentW = this.contentFullSize.w,
        defContentH = this.contentFullSize.h,
        newContentW = defContentW * inScale,
        newContentH = defContentH * inScale,
        safeX = x, safeY = y;
        
        
        /*X*/
        if (newContentW < mivHolW)
        {
            if (x < limits.xMin || x > limits.xMax)
                safeX = mivHolCentX - newContentW / 2;
        }
        else
        {
            if(x < limits.xMin) {
                safeX = limits.xMin;
            } else if(x > limits.xMax) {
                safeX = limits.xMax;
            }
        }
        
        /*Y*/
        if (newContentH < mivHolH)
        {
            if (y < limits.yMin || y > limits.yMax)
                safeY = mivHolCentY - newContentH / 2;
        }
        else
        {
            if(y < limits.yMin) {
                safeY = limits.yMin;
            } else if(y > limits.yMax) {
                safeY = limits.yMax;
            }
        }
        
        return {'x' : safeX, 'y' : safeY};
    }
    LocationChanger.prototype.animSizeAndPos = function (x, y, w, h, noAnim) {
        var safeXY, iterimScale, 
        changeHandler =  function(_this) { 
            return function() { 
                _this.dispatchEventChange(); 
            } 
        }(this);
        
        if(w != undefined) {
            iterimScale =  w / this.contentFullSize.w;
        } else {
            iterimScale =  this.$content.width() / this.contentFullSize.w;
        }
        
        if(x != undefined && y != undefined) {
            safeXY = this.getSafeXY(x, y, iterimScale);
            if(noAnim) {
                this.$contentHol.css({ left : safeXY.x, top : safeXY.y });
                this.dispatchEventChange();
            } else {
                this.$contentHol.animate({ left : safeXY.x, top : safeXY.y }, 
                                         { duration : this.sett.animTime, easing : 'easeOutCubic', 
                                           step : changeHandler,
                                           complete : changeHandler });
            }
        }
        if(w != undefined && h != undefined) {
            if(noAnim) {
                this.$content.css({ width : w, height : h });
                this.dispatchEventChange();
            } else {
                this.$content.animate({ width : w, height : h }, 
                                      { duration : this.sett.animTime, easing : 'easeOutCubic',
                                        step : changeHandler,
                                        complete : changeHandler }); 
            }
        }
    }
    LocationChanger.prototype.animStop = function (saveScale) {
        if(this.$contentHol && this.$content) {
            this.$contentHol.stop().clearQueue();
            this.$content.stop().clearQueue();
            
            if(saveScale) {
                this.sm.setScale(this.$content.width() / this.contentFullSize.w);
            }
            
            this.dispatchEventChange();
        }
    }
    LocationChanger.prototype.dispatchEventChange = function () {
        var e, a = {},
        contentHolPos = this.$contentHol.position(),
        limits = this.getLimit(this.sm.getScale()),
        contnetW = this.$content.width(),
        contnetH = this.$content.height();

        /*position*/
        a.allowDown = (Math.ceil(contentHolPos.top) < Math.ceil(limits.yMax));
        a.allowUp = (Math.ceil(contentHolPos.top) > Math.ceil(limits.yMin));
        a.allowRight = (Math.ceil(contentHolPos.left) < Math.ceil(limits.xMax));
        a.allowLeft = (Math.ceil(contentHolPos.left) > Math.ceil(limits.xMin));
        
        /*scale*/
        a.allowZoom = (contnetW / this.contentFullSize.w < 1);
        if(this.sett.fitToViewportShortSide) {
            a.allowUnzoom = (contnetW > this.$mivHol.width() && contnetH > this.$mivHol.height());
        } else {
            a.allowUnzoom = (contnetW > this.$mivHol.width() || contnetH > this.$mivHol.height());
        }
        
        if(!this.allowCompare(a, this.allow)) {
            e = $.Event("mivChange", a);
            this.$mainHolder.trigger(e);
        }
        
        this.allow = a;
    }
    LocationChanger.prototype.allowCompare = function (_new, _old) {
        var res = true;
        
        $.each(_new, function(k){
            if(_new[k] != _old[k]) {
                res = false;
                return;
            }
        });
        
        return res;
    }
    
    /*scale manager*/
    var ScaleManager = function() {
        this.step = .1;
        this.curr = 1;
    };
    ScaleManager.prototype.getScale = function() {
        return this.curr;
    }
    ScaleManager.prototype.setScale = function(v) {
        this.curr = v;
    }
    ScaleManager.prototype.nextScale = function() {
        var scale = this.curr + this.step;
        if(scale > 1) {
            this.curr = 1;
        } else {
            this.curr = scale;
        }
        return this.getScale();
    }
    ScaleManager.prototype.prevScale = function() {
        var scale = this.curr - this.step;
        if(scale < this.step) {
            this.curr = this.step;
        } else {
            this.curr = scale;
        }
        return this.getScale();
    }
    /**/
    
    /*content loaders*/
    var LoaderImgContent = function (url, $imgHolder, callback) {
        var $img = $('<img/>');
        this.callback = callback;
        
        $img.one('load', function (that){ 
            return function (e) {
                that.loadComplete(e);
            }
        }(this));
        
        $imgHolder.append($img);
        $img.attr('src',url); //load
    }
    LoaderImgContent.prototype.loadComplete = function(e) {
        if(this.callback) {
            this.callback($(e.currentTarget));
        }
    }
    LoaderImgContent.prototype.dispose = function() {
        this.callback = null;
    }
    /**/

})(jQuery);