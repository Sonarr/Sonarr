/*
	Smooth Zoom Pan - jQuery Image Viewer
 	Copyright (c) 2011-12 Ramesh Kumar
	http://codecanyon.net/user/VF
	
	Version:	1.6.7
	RELEASE:	09 SEP 2011
	UPDATE:		27 SEP 2012
	
	Built using:
	jQuery 		version:1.7.1	http://jquery.com/
	Modernizr 	version:2.5.3	http://www.modernizr.com/
	MouseWheel	version:3.0.6	http://brandonaaron.net/code/mousewheel/docs
	
*/

(function ($, window, document) {

    /*****************************************************************************
		Default settings:
		For detailed description of individual parameters, see the help document
	******************************************************************************/
    var defaults = {

        width: '',									//Width of the view area [480, '480px', '100%']
        height: '',									//Height of the view area [480, '480px', '100%']

        initial_ZOOM: '',							//Initial zoom level to start with (in percentage) [100]
        initial_POSITION: '',						//Initial location to be focused in pixel value [150,150 or 150 150]

        animation_SMOOTHNESS: 5.5,					//Ease or smoothness of all movements [Any number from 0]				
        animation_SPEED_ZOOM: 5.5,					//Speed of zoom movements [Any number from 0] 
        animation_SPEED_PAN: 5.5,					//Speed of pan movements [Any number from 0] 

        zoom_MAX: 800,								//Maximum limit for zooming (in percentage)
        zoom_MIN: '',								//Minimum limit for zooming (in percentage)
        zoom_SINGLE_STEP: false,					//To reach maximum and minimum zoom levels in single click 
        zoom_OUT_TO_FIT: true,						//To allow image to be zoomed out with whitespace on sides
        zoom_BUTTONS_SHOW: true,					//To enable/disable the + and - buttons

        pan_BUTTONS_SHOW: true,						//To enable/disable the arrow and reset buttons
        pan_LIMIT_BOUNDARY: true,					//To allow/restrict moving the image beyond boundaries
        pan_REVERSE: false,

        reset_ALIGN_TO: 'center center', 			//Image can be aligned to desired position on reset. Example: 'Top Left'
        reset_TO_ZOOM_MIN: true,					//How it should behave if zoom_MIN value set and while clicking reset button, 

        button_SIZE: 18,							//Button width and height (in pixels)
        button_SIZE_TOUCH_DEVICE: 30,				//Button width and height (in pixels) on touch devices
        button_COLOR: '#FFFFFF',					//Button color in hexadecimal
        button_BG_COLOR: '#000000',					//Button set's background color in hexadecimal
        button_BG_TRANSPARENCY: 55,					//Background transparency (in percentage)
        button_AUTO_HIDE: false,					//To hide the button set when mouse moved outside the view area
        button_AUTO_HIDE_DELAY: 1,					//Auto hide delay time in seconds
        button_ALIGN: 'bottom right',				//Button set can be aligned to any side or center
        button_MARGIN: 10,							//Space between button set and view port's edge
        button_ROUND_CORNERS: true,					//To enable disable roundness of button corner

        touch_DRAG: true,							//Enable/disable the dragability of image (touch)
        mouse_DRAG: true,							//Enable/disable the dragability of image (mouse)
        mouse_WHEEL: true,							//Enable/disable mousewheel zoom
        mouse_WHEEL_CURSOR_POS: true,				//Enable/disable position sensitive mousewheel zoom
        mouse_DOUBLE_CLICK: true,					//Enable/disable zoom action with double click

        background_COLOR: '#FFFFFF',				//Background colour of image container
        border_SIZE: 1,								//Border size of view area
        border_COLOR: '#000000',					//Border color of view area
        border_TRANSPARENCY: 10,					//Border transparency of view area

        image_url: '',								//Set url or image to be zoomed
        image_original_width: '',					//Original width of main image
        image_original_height: '',					//Original height of main image
        container: '',								//Set container element of image (id of container)

        on_IMAGE_LOAD: '',							//To Call external function immediatly after image loaded
        on_ZOOM_PAN_UPDATE: '',						//To Call external function for each zoom, pan animation frame
        on_ZOOM_PAN_COMPLETE: '',					//To Call external function whenever zoom, pan animation completes
        on_LANDMARK_STATE_CHANGE: '',				//To Call external function whenever the zoom leval crosses global "data-show-at-zoom" value

        use_3D_Transform: true,						//To enable / disable Hardware acceleration on webkit browsers

        responsive: false,							//To enable / disable Responsive / fluid layout
        responsive_maintain_ratio: true,			//To maintain view area width/height ratio or not
        max_WIDTH: '',								//Maximum allowed width of view area (helpful when 'width' parameter set with % and need limit)
        max_HEIGHT: ''								//Maximum allowed height of view area (helpful when 'height' parameter set with % and need limit)
    };

    function Zoomer($elem, params) {

        var self = this;
        this.$elem = $elem;
        var op = $.extend({}, defaults, params);

        /**********************************************************
		Option values verified and formated if needed
		**********************************************************/
        this.sW = op.width;
        this.sH = op.height;

        this.init_zoom = op.initial_ZOOM / 100;
        this.init_pos = op.initial_POSITION.replace(/,/g, ' ').replace(/\s{2,}/g, ' ').split(' ');

        this.zoom_max = op.zoom_MAX / 100;
        this.zoom_min = op.zoom_MIN / 100;

        this.zoom_single = checkBoolean(op.zoom_SINGLE_STEP);
        this.zoom_fit = checkBoolean(op.zoom_OUT_TO_FIT);
        this.zoom_speed = 1 + (((op.animation_SPEED === 0 || op.animation_SPEED ? op.animation_SPEED : op.animation_SPEED_ZOOM) + 1) / 20);
        this.zoom_show = checkBoolean(op.zoom_BUTTONS_SHOW);

        this.pan_speed_o = (op.animation_SPEED === 0 || op.animation_SPEED ? op.animation_SPEED : op.animation_SPEED_PAN);
        this.pan_show = checkBoolean(op.pan_BUTTONS_SHOW);
        this.pan_limit = checkBoolean(op.pan_LIMIT_BOUNDARY);
        this.pan_rev = checkBoolean(op.pan_REVERSE);

        this.reset_align = op.reset_ALIGN_TO.toLowerCase().split(' ');
        this.reset_to_zmin = checkBoolean(op.reset_TO_ZOOM_MIN);

        if (supportsTouch) {
            this.bu_size = parseInt(op.button_SIZE_TOUCH_DEVICE / 2) * 2;
        } else {
            this.bu_size = parseInt(op.button_SIZE / 2) * 2;
        }
        this.bu_color = op.button_COLOR;
        this.bu_bg = op.button_BG_COLOR;
        this.bu_bg_alpha = op.button_BG_TRANSPARENCY / 100;
        this.bu_icon = op.button_ICON_IMAGE;
        this.bu_auto = checkBoolean(op.button_AUTO_HIDE);

        this.bu_delay = op.button_AUTO_HIDE_DELAY * 1000;
        this.bu_align = op.button_ALIGN.toLowerCase().split(' ');
        this.bu_margin = op.button_MARGIN;
        this.bu_round = checkBoolean(op.button_ROUND_CORNERS);

        this.touch_drag = checkBoolean(op.touch_DRAG);
        this.mouse_drag = checkBoolean(op.mouse_DRAG);
        this.mouse_wheel = checkBoolean(op.mouse_WHEEL);
        this.mouse_wheel_cur = checkBoolean(op.mouse_WHEEL_CURSOR_POS);
        this.mouse_dbl_click = checkBoolean(op.mouse_DOUBLE_CLICK);

        this.ani_smooth = Math.max(1, (op.animation_SMOOTHNESS + 1) / 1.45);

        this.bg_color = op.background_COLOR;
        this.bord_size = op.border_SIZE;
        this.bord_color = op.border_COLOR;
        this.bord_alpha = op.border_TRANSPARENCY / 100;

        this.container = op.container;
        this.image_url = op.image_url;
        this.image_width = op.image_original_width;
        this.image_height = op.image_original_height;

        this.responsive = checkBoolean(op.responsive);
        this.maintain_ratio = checkBoolean(op.responsive_maintain_ratio);
        this.w_max = op.max_WIDTH;
        this.h_max = op.max_HEIGHT;

        this.onLOAD = op.on_IMAGE_LOAD;
        this.onUPDATE = op.on_ZOOM_PAN_UPDATE;
        this.onZOOM_PAN = op.on_ZOOM_PAN_COMPLETE;
        this.onLANDMARK = op.on_LANDMARK_STATE_CHANGE;

        /***********************************************************
		Variables for inner operation.	
		x, y, width, height and scale value of image
		***********************************************************/
        this._x;
        this._y;
        this._w;
        this._h;
        this._sc = 0;

        this.rA = 1;
        this.rF = 1;
        this.rR = 1;
        this.iW = 0;
        this.iH = 0;
        this.tX = 0;
        this.tY = 0;
        this.oX = 0;
        this.oY = 0;
        this.fX = 0;
        this.fY = 0;
        this.dX = 0;
        this.dY = 0;
        this.cX = 0;
        this.cY = 0;

        this.transOffX = 0;
        this.transOffY = 0;
        this.focusOffX = 0;
        this.focusOffY = 0;
        this.offX = 0;
        this.offY = 0;

        /***********************************************************
		Flags that convey current states and events 
		***********************************************************/
        this._playing = false;
        this._dragging = false;
        this._onfocus = false;
        this._moveCursor = false;
        this._wheel = false;
        this._recent = 'zoomOut';
        this._pinching = false;
        this._landmark = false;
        this._rA;
        this._centx;
        this._centy;
        this._onButton = false;
        this._onHitArea = false;
        this.cFlag = {
            _zi: false,
            _zo: false,
            _ml: false,
            _mr: false,
            _mu: false,
            _md: false,
            _rs: false,
            _nd: false
        };

        /***********************************************************
		Elements and arrays that references elements
		***********************************************************/
        this.$holder;
        this.$hitArea;
        this.$controls;
        this.$loc_cont;
        this.map_coordinates = [];
        this.locations = [];
        this.buttons = [];
        this.border = [];

        /***********************************************************
		miscellaneous
		***********************************************************/
        this.buttons_total = 7;
        this.cButtId = 0;
        this.pan_speed;
        this.auto_timer;
        this.ani_timer;
        this.ani_end;
        this.focusSpeed = this.reduction = .5;
        this.orig_style;
        this.mapAreas;
        this.icons;
        this.show_at_zoom;
        this.assetsLoaded = false;
        this.zStep = 0;
        this.sRed = 300;
        this.use3D = op.use_3D_Transform && supportsTrans3D;

        if (supportsTouch) {
            this.event_down = 'touchstart.sz';
            this.event_up = 'touchend.sz';
            this.event_move = 'touchmove.sz';
        } else {
            this.event_down = 'mousedown.sz';
            this.event_up = 'mouseup.sz';
            this.event_move = 'mousemove.sz';
        }

        //Case 1: Image specificed (possibly) through img tag:
        if (this.image_url == '') {
            this.$image = $elem;
            this.id = this.$image.attr('id');

            //Case 2: Image url specificed through parameter:
        } else {
            var img = new Image();
            if (this.image_width) {
                img.width = this.image_width;
            }
            if (this.image_height) {
                img.height = this.image_height;
            }
            img.src = this.image_url;
            this.$image = $(img).appendTo($elem);
        }


        //Prepare container div (Basically the element that masks image with overflow hidden)
        this.setContainer();

        //Get button icon image's url
        var testOb;
        if (!this.bu_icon) {
            var regx = /url\(["']?([^'")]+)['"]?\)/;
            testOb = $('<div class="smooth_zoom_icons"></div>');
            this.$holder.append(testOb);
            this.bu_icon = testOb.css("background-image").replace(regx, '$1');
            if (this.bu_icon == 'none') {
                this.bu_icon = 'zoom_assets/icons.png';
            }
            testOb.remove();
        }

        //Firefox feature checkup
        if (this.$image.css('-moz-transform') && prop_transform) {
            testOb = $('<div style="-moz-transform: translate(1px, 1px)"></div>');
            this.$holder.append(testOb);
            this.fixMoz = testOb.position().left === 1 ? false : true;
            testOb.remove();
        } else {
            this.fixMoz = false;
        }

        //Preload icons and main image.	
        this.$image.hide();
        this.imgList = [
			{ loaded: false, src: this.bu_icon || 'zoom_assets/icons.png' }, //Icon image
			{ loaded: false, src: this.image_url == '' ? this.$image.attr('src') : this.image_url } // Main image
        ];

        $.each(this.imgList, function (i) {
            var _img = new Image();
            $(_img).bind('load', { id: i, self: self }, self.loadComplete)
					.bind('error', { id: i, self: self }, self.loadComplete); //Allow initiation even if image is not there :(
            _img.src = self.imgList[i].src;
        });

    }

    Zoomer.prototype = {

        /*Preload the icon and main image
		*********************************************************************************************************************/
        loadComplete: function (e) {
            var self = e.data.self,
				complete = true;

            self.imgList[e.data.id].loaded = true;
            for (var j = 0; j < self.imgList.length; j++) {
                if (!self.imgList[j].loaded) {
                    complete = false;
                }
            }
            if (complete) {
                self.assetsLoaded = true;
                if (self.onLOAD !== '') {
                    self.onLOAD();
                }

                //Assets loaded, initiate plugin
                self.init();
            }
        },


        /*Initiate after assets loaded
		***********************************************************************************************************************/
        init: function () {
            var self = this,
				$image = self.$image,
				sW = self.sW,
				sH = self.sH,
				container = self.container,
				cBW, cBH, pan_show = self.pan_show,
				zoom_show = self.zoom_show,
				$controls = self.$controls,
				buttons = self.buttons,
				cFlag = self.cFlag,
				bu_align = self.bu_align,
				bu_margin = self.bu_margin,
				$holder = self.$holder;

            //Store the default image properties so that it can be reverted back when plugin needs to be destroyed
            self.orig_style = self.getStyle();

            //IE 6 Image tool bar disabled
            $image.attr('galleryimg', 'no');

            if (!navigator.userAgent.toLowerCase().match(/(iphone|ipod|ipad)/)) {
                $image.removeAttr('width');
                $image.removeAttr('height');
            }

            //In case parent element's display property set to 'none', we need to first set them 'block', measure the width and height and then set them back to 'none'
            var temp = $image,
			dispArray = [];
            for (var i = 0; i < 5; i++) {
                if (temp && temp[0].tagName !== 'BODY' && temp[0].tagName !== 'HTML') {
                    if (temp.css('display') == 'none') {
                        temp.css('display', 'block');
                        dispArray.push(temp);
                    }
                    temp = temp.parent();
                } else {
                    break;
                }
            }

            self.iW = $image.width();
            self.iH = $image.height();

            for (var i = 0; i < dispArray.length; i++) {
                dispArray[i].css('display', 'none');
            }

            //Initially the image needs to be resized to fit container. To do so, first measure the scaledown ratio	
            self.rF = self.rR = self.checkRatio(sW, sH, self.iW, self.iH, self.zoom_fit);

            //If NO Minimum zoom value set
            if (self.zoom_min == 0 || self.init_zoom != 0) {
                if (self.init_zoom != '') {
                    self.rA = self._sc = self.init_zoom;
                } else {
                    self.rA = self._sc = self.rF;
                }
                if (self.zoom_min != 0) {
                    self.rF = self.zoom_min;
                }

                //If Minimum zoom value set
            } else {
                if (self.rF < self.zoom_min) {
                    self.rF = self.zoom_min;
                    if (self.reset_to_zmin) {
                        self.rR = self.zoom_min
                    }
                    self.rA = self._sc = self.zoom_min;
                } else {
                    self.rA = self._sc = self.rR;
                }
            }

            //Width and Height to be applied to the image
            self._w = self._sc * self.iW;
            self._h = self._sc * self.iH;

            //Resize the image and position it centered inside the wrapper
            if (self.init_pos == '') {
                self._x = self.tX = (sW - self._w) / 2;
                self._y = self.tY = (sH - self._h) / 2;
            } else {
                self._x = self.tX = (sW / 2) - parseInt(self.init_pos[0]) * self._sc;
                self._y = self.tY = (sH / 2) - parseInt(self.init_pos[1]) * self._sc;
                self.oX = (self.tX - ((sW - self._w) / 2)) / (self._w / sW);
                self.oY = (self.tY - ((sH - self._h) / 2)) / (self._h / sH);
            }

            if ((!self.pan_limit || self._moveCursor || self.init_zoom != self.rF) && self.mouse_drag) {
                $image.css('cursor', 'move');
                self.$hitArea.css('cursor', 'move');
            }

            if ($.browser.mozilla && supportsTrans3D) {
                $image.css('opacity', 0);
            }

            if (prop_transform) {
                self.$image.css(prop_origin, '0 0');
            }
            if (self.use3D) {
                $image.css({
                    '-webkit-backface-visibility': 'hidden',
                    '-webkit-perspective': 1000
                });
            }

            //Start displaying the image		
            $image.css({
                position: 'absolute',
                'z-index': 2,
                left: '0px',
                top: '0px',
                '-webkit-box-shadow': '1px 1px rgba(0,0,0,0)'
            })
				.hide()
				.fadeIn(500, function () {
				    $holder.css('background-image', 'none');
				    if ($.browser.mozilla && supportsTrans3D) {
				        $image.css('opacity', 1);
				    }
				});

            //Create Control buttons and events				
            var self = self,
				bs = self.bu_size,
				iSize = 50,
				sDiff = 2,
				bSpace = 3,
				mSize = Math.ceil(self.bu_size / 4),
				iconOff = bs < 16 ? 50 : 0,
				bsDiff = bs - sDiff;

            //Show all buttons		
            if (pan_show) {
                if (zoom_show) {
                    cBW = parseInt(bs + (bs * .85) + (bsDiff * 3) + (bSpace * 2) + (mSize * 2));
                } else {
                    cBW = parseInt((bsDiff * 3) + (bSpace * 2) + (mSize * 2));
                }
                cBH = parseInt((bsDiff * 3) + (bSpace * 2) + (mSize * 2));

                //Show zoom buttons only		
            } else {
                if (zoom_show) {
                    cBW = parseInt(bs + mSize * 2);
                    cBH = parseInt(bs * 2 + mSize * 3);
                    cBW = parseInt(cBW / 2) * 2;
                    cBH = parseInt(cBH / 2) * 2;
                } else {
                    cBW = 0;
                    cBH = 0;
                }
            }

            var pOff = (iSize - bs) / 2,
				resetCenterX = cBW - ((bs - (pan_show ? sDiff : 0)) * 2) - mSize - bSpace,
				resetCenterY = (cBH / 2) - ((bs - (pan_show ? sDiff : 0)) / 2);

            var hProp, vProp, hVal, vVal;
            //Align button set as per settings
            if (bu_align[0] == 'top') {
                vProp = 'top';
                vVal = bu_margin;
            } else if (bu_align[0] == 'center') {
                vProp = 'top';
                vVal = parseInt((sH - cBH) / 2);
            } else {
                vProp = 'bottom';
                vVal = bu_margin;
            }
            if (bu_align[1] == 'right') {
                hProp = 'right';
                hVal = bu_margin;
            } else if (bu_align[1] == 'center') {
                hProp = 'right';
                hVal = parseInt((sW - cBW) / 2);
            } else {
                hProp = 'left';
                hVal = bu_margin;
            }

            //Buttons Container		
            $controls = $(
				'<div style="position: absolute; ' + hProp + ':' + hVal + 'px; ' + vProp + ': ' + vVal + 'px; width: ' + cBW + 'px; height: ' + cBH + 'px; z-index: 20;" class="noSel;">\
					<div class="noSel controlsBg" style="position: relative; width: 100%; height: 100%; z-index: 1;">\
					</div>\
				</div>'
			);

            $holder.append($controls);
            var $controlsBg = $controls.find('.controlsBg');

            //Make the corners rounded
            if (self.bu_round) {
                if (prop_radius) {
                    $controlsBg
						.css(prop_radius, (iconOff > 0 ? 4 : 5) + 'px')
						.css('background-color', self.bu_bg);
                } else {
                    self.roundBG(
						$controlsBg,
						'cBg',
						cBW,
						cBH,
						iconOff > 0 ? 4 : 5,
						375,
						self.bu_bg,
						self.bu_icon,
						1,
						iconOff ? 50 : 0
					);
                }
            } else {
                $controlsBg.css('background-color', self.bu_bg);
            }

            $controlsBg.css('opacity', self.bu_bg_alpha);

            //Generating Button properties	(7 buttons)			
            buttons[0] = {
                _var: '_zi',
                l: mSize,
                t: pan_show ? (cBH - (bs * 2) - (bSpace * 2) + 2) / 2 : mSize,
                w: bs,
                h: bs,
                bx: -pOff,
                by: -pOff - iconOff
            };

            buttons[1] = {
                _var: '_zo',
                l: mSize,
                t: pan_show ? ((cBH - (bs * 2) - (bSpace * 2) + 2) / 2) + bs + (bSpace * 2) - 2 : cBH - bs - mSize,
                w: bs,
                h: bs,
                bx: -iSize - pOff,
                by: -pOff - iconOff
            };

            buttons[2] = {
                _var: self.pan_rev ? '_ml' : '_mr',
                l: resetCenterX - bsDiff - bSpace,
                t: resetCenterY,
                w: bsDiff,
                h: bsDiff,
                bx: -(sDiff / 2) - iSize * 2 - pOff,
                by: -(sDiff / 2) - pOff - iconOff
            };

            buttons[3] = {
                _var: self.pan_rev ? '_mr' : '_ml',
                l: resetCenterX + bsDiff + bSpace,
                t: resetCenterY,
                w: bsDiff,
                h: bsDiff,
                bx: -(sDiff / 2) - iSize * 3 - pOff,
                by: -(sDiff / 2) - pOff - iconOff
            };

            buttons[4] = {
                _var: self.pan_rev ? '_md' : '_mu',
                l: resetCenterX,
                t: resetCenterY + bsDiff + bSpace,
                w: bsDiff,
                h: bsDiff,
                bx: -(sDiff / 2) - iSize * 4 - pOff,
                by: -(sDiff / 2) - pOff - iconOff
            };

            buttons[5] = {
                _var: self.pan_rev ? '_mu' : '_md',
                l: resetCenterX,
                t: resetCenterY - bsDiff - bSpace,
                w: bsDiff,
                h: bsDiff,
                bx: -(sDiff / 2) - iSize * 5 - pOff,
                by: -(sDiff / 2) - pOff - iconOff
            };

            buttons[6] = {
                _var: '_rs',
                l: resetCenterX,
                t: resetCenterY,
                w: bsDiff,
                h: bsDiff,
                bx: -(sDiff / 2) - iSize * 6 - pOff,
                by: -(sDiff / 2) - pOff - iconOff
            };

            for (var i = 0; i < 7; i++) {
                buttons[i].$ob = $(
						'<div style="position: absolute; display: ' + (i < 2 ? zoom_show ? 'block' : 'none' : pan_show ? 'block' : 'none') + '; left: ' + (buttons[i].l - 1) + 'px; top: ' + (buttons[i].t - 1) + 'px; width: ' + (buttons[i].w + 2) + 'px; height: ' + (buttons[i].h + 2) + 'px; z-index:' + (i + 1) + ';" class="noSel">\
						</div>'
					)
				.css('opacity', .7)
				.bind((supportsTouch ? "" : 'mouseover.sz mouseout.sz ') + self.event_down, {
				    id: i

				}, function (e) {
				    self._onfocus = false;
				    $this = $(this);

				    //Button over 
				    if (e.type == 'mouseover') {
				        if ($this.css('opacity') > .5) {
				            $this.css('opacity', 1);
				        }

				        //Button out 
				    } else if (e.type == 'mouseout') {
				        if ($this.css('opacity') > .5) {
				            $this.css('opacity', .7);
				        }

				        //Button press/down
				    } else if (e.type == 'mousedown' || e.type == 'touchstart') {
				        self.cButtId = e.data.id;
				        self._onButton = true;
				        self._wheel = false;

				        //If NOT already down..
				        if ($this.css('opacity') > .5) {
				            $this.css('opacity', 1);
				            $holder.find('#' + buttons[self.cButtId]._var + 'norm').hide();
				            $holder.find('#' + buttons[self.cButtId]._var + 'over').show();

				            //CASE 1: If zoomIn pressed and single step zoom enabled
				            if (self.cButtId <= 1 && self.zoom_single) {
				                if (!cFlag[buttons[self.cButtId]._var]) {
				                    self.sRed = 300;
				                    cFlag[buttons[self.cButtId]._var] = true;
				                }

				                //CASE 2: If any button except RESET pressed
				            } else if (self.cButtId < 6) {
				                cFlag[buttons[self.cButtId]._var] = true;

				                //CASE 3: RESET pressed							
				            } else {
				                cFlag._rs = true;
				                self.rA = self.rR;
				                if (self.reset_align[0] == 'top') {
				                    self.fY = (self.sH / 2) * (self.rA / 2);
				                } else if (self.reset_align[0] == 'bottom') {
				                    self.fY = -(self.sH / 2) * (self.rA / 2);
				                } else {
				                    self.fY = 0;
				                }
				                if (self.reset_align[1] == 'left') {
				                    self.fX = (self.sW / 2) * (self.rA / 2);
				                } else if (self.reset_align[1] == 'right') {
				                    self.fX = -(self.sW / 2) * (self.rA / 2);
				                } else {
				                    self.fX = 0;
				                }
				            }

				            self.focusOffX = self.focusOffY = 0;
				            self.changeOffset(true, true);
				            if (!self._playing) {
				                self.Animate();
				            }
				        }
				        e.preventDefault();
				        e.stopPropagation();
				    }
				});

                //Make 2 BGs for Button Normal and Over state
                //Button BG normal
                var tpm = $(
					'<div id="' + buttons[i]._var + 'norm" style="position: absolute; left: 1px; top: 1px; width: ' + buttons[i].w + 'px; height: ' + buttons[i].h + 'px; ' + (prop_radius || !self.bu_round ? 'background:' + self.bu_color : '') + '">\
					</div>'
				);

                //Button BG hover
                var tpmo = $(
					'<div id="' + buttons[i]._var + 'over" style="position: absolute; left: 0px; top: 0px; width: ' + (buttons[i].w + 2) + 'px; height: ' + (buttons[i].h + 2) + 'px; display: none; ' + (prop_radius || !self.bu_round ? 'background:' + self.bu_color : '') + '">\
					</div>'
				);

                //Add the button icons
                var cont = $(
					'<div id="' + buttons[i]._var + '_icon" style="position: absolute; left: 1px; top: 1px; width: ' + buttons[i].w + 'px; height: ' + buttons[i].h + 'px; background: transparent url(' + self.bu_icon + ') ' + buttons[i].bx + 'px ' + buttons[i].by + 'px no-repeat;" >\
					</div>'
				);

                buttons[i].$ob.append(tpm, tpmo, cont);
                $controls.append(buttons[i].$ob);

                //Apply corner radius
                if (self.bu_round) {
                    if (prop_radius) {
                        tpm.css(prop_radius, '2px');
                        tpmo.css(prop_radius, '2px');
                    } else {
                        self.roundBG(
							tpm,
							buttons[i]._var + "norm",
							buttons[i].w,
							buttons[i].h,
							2,
							425,
							self.bu_color,
							self.bu_icon,
							i + 1,
							iconOff ? 50 : 0
						);
                        self.roundBG(
							tpmo,
							buttons[i]._var + "over",
							buttons[i].w + 2,
							buttons[i].h + 2,
							2,
							425,
							self.bu_color,
							self.bu_icon,
							i + 1,
							iconOff ? 50 : 0
						);
                    }
                }
            }

            //Add Events for mouse drag / touch swipe action
            $(document).bind(self.event_up + self.id, { self: self }, self.mouseUp);

            if ((self.mouse_drag && !supportsTouch) || (self.touch_drag && supportsTouch)) {
                self.$holder.bind(self.event_down, { self: self }, self.mouseDown);
                if (supportsTouch) {
                    $(document).bind(self.event_move + self.id, { self: self }, self.mouseDrag);
                }
            }

            //Add Double click / Double tap zoom
            if (self.mouse_dbl_click) {
                var dClickedX,
					dClickedY,
					dbl_click_dir = 1;

                self.$holder.bind('dblclick.sz', function (e) {
                    self.focusOffX = e.pageX - $holder.offset().left - (self.sW / 2);
                    self.focusOffY = e.pageY - $holder.offset().top - (self.sH / 2);
                    self.changeOffset(true, true);
                    self._wheel = false;

                    if (self.rA < self.zoom_max && dbl_click_dir == -1 && dClickedX != self.focusOffX && dClickedY != self.focusOffY) {
                        dbl_click_dir = 1;
                    }

                    dClickedX = self.focusOffX;
                    dClickedY = self.focusOffY;

                    if (self.rA >= self.zoom_max && dbl_click_dir == 1) {
                        dbl_click_dir = -1;
                    }
                    if (self.rA <= self.rF && dbl_click_dir == -1) {
                        dbl_click_dir = 1;
                    }
                    if (dbl_click_dir > 0) {
                        self.rA *= 2;
                        self.rA = self.rA > self.zoom_max ? self.zoom_max : self.rA;
                        cFlag._zi = true;
                        clearTimeout(self.ani_timer);
                        self._playing = true;
                        self.Animate();
                        cFlag._zi = false;

                    } else {
                        self.rA /= 2;
                        self.rA = self.rA < self.rF ? self.rF : self.rA;
                        cFlag._zo = true;
                        clearTimeout(self.ani_timer);
                        self._playing = true;
                        self.Animate();
                        cFlag._zo = false;
                    }
                    e.preventDefault();
                    e.stopPropagation();
                });
            }

            //Add mouse wheel event if enabled
            if (self.mouse_wheel) {
                $holder.bind('mousewheel.sz', { self: this }, self.mouseWheel);
            }

            //Auto Hide the control buttons if enabled
            if (self.bu_auto) {
                $holder.bind('mouseleave.sz', { self: this }, self.autoHide);
            }

            //Prevent Controls Bg from start dragging image
            $controls.bind(self.event_down, function (e) {
                e.preventDefault();
                e.stopPropagation();
            });

            //Prevent Controls Bg from double click zoom
            if (self.mouse_dbl_click) {
                $controls.bind('dblclick.sz', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                });
            }

            //Prevent text selection for smoother dragging and button focus
            $('.noSel').each(function () {
                this.onselectstart = function () {
                    return false;
                };
            });

            self.$holder = $holder;
            self.$controls = $controls;
            self.sW = sW;
            self.sH = sH;
            self.cBW = cBW;
            self.cBH = cBH;

            //Apply initial transformation
            self.Animate();
        },


        /*Prepare the container (holder) element and get landmarks if available
		***********************************************************************************************************************/
        setContainer: function () {
            var self = this,
				$image = self.$image,
				bord_size = self.bord_size,
				border = self.border,
				$holder = self.$holder;

            //Wrap a container for image or get the container if specified through options:
            if (self.container == '' && self.image_url == '') {
                $holder = self.$image.wrap(
					'<div class="noSel smooth_zoom_preloader">\
					</div>'
				).parent();

            } else {
                if (self.image_url == '') {
                    $holder = $('#' + self.container);
                } else {
                    $holder = self.$elem;
                }
                $holder.addClass('noSel smooth_zoom_preloader');
                self.locations = [];
                self.$loc_cont = $holder.find('.landmarks');
                if (self.$loc_cont[0]) {
                    var locs = self.$loc_cont.children('.item');
                    self.loc_clone = self.$loc_cont.clone();
                    self.show_at_zoom = parseInt(self.$loc_cont.data('show-at-zoom'), 10) / 100;
                    self.allow_scale = checkBoolean(self.$loc_cont.data('allow-scale'));
                    self.allow_drag = checkBoolean(self.$loc_cont.data('allow-drag'));
                    locs.each(function () {
                        self.setLocation($(this));
                    });
                }
            }

            $holder.css({
                'position': 'relative',
                'overflow': 'hidden',
                'text-align': 'left',
                '-moz-user-select': 'none',
                '-khtml-user-select': 'none',
                '-webkit-user-select': 'none',
                'user-select': 'none',
                '-webkit-touch-callout': 'none',
                '-webkit-tap-highlight-color': 'rgba(255, 255, 255, 0)',
                'background-color': self.bg_color,
                'background-position': 'center center',
                'background-repeat': 'no-repeat'
            })

            self.$hitArea = $('<div style="position: absolute; z-index: 1; top: 0px; left: 0px; width: 100%; height: 100%;" ></div>').appendTo($holder);

            self.getContainerSize(self.sW, self.sH, $holder, self.w_max, self.h_max);

            if (self.responsive) {
                $(window).bind("orientationchange.sz" + self.id + " resize.sz" + self.id, { self: self }, self.resize);
            }
            var sW = self.sW;
            var sH = self.sH;

            //Add Image container properties			
            $holder.css({
                'width': sW,
                'height': sH
            });

            //Add border if needed
            if (bord_size > 0) {
                border[0] = $('<div style="position: absolute;	width: ' + bord_size + 'px; height: ' + sH + 'px;	top: 0px; left: 0px; z-index: 3; background-color: ' + self.bord_color + ';"></div>').css('opacity', self.bord_alpha);
                border[1] = $('<div style="position: absolute;	width: ' + bord_size + 'px; height: ' + sH + 'px;	top: 0px; left: ' + (sW - bord_size) + 'px; z-index: 4; background-color: ' + self.bord_color + ';"></div>').css('opacity', self.bord_alpha);
                border[2] = $('<div style="position: absolute;	width: ' + (sW - (bord_size * 2)) + 'px; height: ' + bord_size + 'px; top: 0px; left: ' + bord_size + 'px; z-index: 5; background-color: ' + self.bord_color + '; line-height: 1px;"></div>').css('opacity', self.bord_alpha);
                border[3] = $('<div style="position: absolute;	width: ' + (sW - (bord_size * 2)) + 'px; height: ' + bord_size + 'px; top: ' + (sH - bord_size) + 'px; left: ' + bord_size + 'px; z-index: 6; background-color: ' + self.bord_color + '; line-height: 1px;"></div>').css('opacity', self.bord_alpha);
                $holder.append(border[0], border[1], border[2], border[3]);
            }

            //Get Image maps if exists
            if ($image.attr('usemap') != undefined) {
                self.mapAreas = $("map[name='" + ($image.attr('usemap').split('#').join('')) + "']").children('area');
                self.mapAreas.each(function (i) {
                    var area = $(this);
                    area.css('cursor', 'pointer');
                    if (self.mouse_drag) {
                        area.bind(self.event_down, { self: self }, self.mouseDown);
                    }
                    if (self.mouse_wheel) {
                        area.bind('mousewheel.sz', { self: self }, self.mouseWheel);
                    }
                    self.map_coordinates.push(area.attr('coords').split(','));
                });
            }

            self.$holder = $holder;
            self.sW = sW;
            self.sH = sH;
        },

        getContainerSize: function (sW, sH, $holder, w_max, h_max) {
            if (sW === '' || sW === 0) {
                if (this.image_url == '') {
                    sW = Math.max($holder.parent().width(), 100);
                } else {
                    sW = Math.max($holder.width(), 100);
                }

            } else if (!isNaN(sW) || String(sW).indexOf('px') > -1) {
                sW = this.oW = parseInt(sW);
                if (this.responsive) {
                    sW = Math.min($holder.parent().width(), sW);
                }
            } else if (String(sW).indexOf('%') > -1) {
                sW = $holder.parent().width() * (sW.split('%')[0] / 100);
            } else {
                sW = 100;
            }
            if (w_max !== 0 && w_max !== '') {
                sW = Math.min(sW, w_max);
            }
            if (sH === '' || sH === 0) {
                if (this.image_url == '') {
                    sH = Math.max($holder.parent().height(), 100);
                } else {
                    sH = Math.max($holder.height(), 100);
                }
            } else if (!isNaN(sH) || String(sH).indexOf('px') > -1) {
                sH = this.oH = parseInt(sH);

            } else if (String(sH).indexOf('%') > -1) {
                sH = $holder.parent().height() * (sH.split('%')[0] / 100);
            } else {
                sH = 100;
            }
            if (h_max !== 0 && h_max !== '') {
                sH = Math.min(sH, h_max);
            }

            if (this.oW && sW !== this.oW) {
                if (this.oH && this.maintain_ratio) {
                    sH = sW / (this.oW / this.oH);
                }
            }

            this.sW = sW;
            this.sH = sH;
        },


        /*Each landmark / location / lable initiated here
		***********************************************************************************************************************/
        setLocation: function (lc) {
            var self = this,
				ob = lc,
				w2, h2, pos, sc;

            if (prop_origin) {
                ob.css(prop_origin, '0 0');
            }

            ob.css({
                'display': 'block',
                'z-index': 2
            })
            if (self.use3D) {
                ob.css({
                    '-webkit-backface-visibility': 'hidden',
                    '-webkit-perspective': 1000
                });
            }

            w2 = ob.outerWidth() / 2;
            h2 = ob.outerHeight() / 2;
            pos = ob.data('position').split(',');
            sc = ob.data('allow-scale');
            if (sc == undefined) {
                sc = self.allow_scale;
            } else {
                sc = checkBoolean(sc);
            }

            if (ob.hasClass('mark')) {
                var imgw = ob.find('img').css('vertical-align', 'bottom').width();
                $(ob.children()[0]).css({
                    'position': 'absolute',
                    'left': (-ob.width() / 2),
                    'bottom': parseInt(ob.css('padding-bottom')) * 2
                });
                var txt = ob.find('.text');
                self.locations.push({
                    ob: ob,
                    x: parseInt(pos[0]),
                    y: parseInt(pos[1]),
                    w2: w2,
                    h2: h2,
                    w2pad: w2 + (txt[0] ? parseInt(txt.css('padding-left')) : 0),
                    vis: false,
                    lab: false,
                    lpx: '0',
                    lpy: '0',
                    showAt: isNaN(ob.data('show-at-zoom')) ? self.show_at_zoom : parseInt(ob.data('show-at-zoom'), 10) / 100,
                    scale: sc
                });

            } else if (ob.hasClass('lable')) {
                var bg = ob.data('bg-color'),
					opacity = ob.data('bg-opacity'),
					cont = $(ob.eq(0).children()[0])
							.css({
							    'position': 'absolute',
							    'z-index': 2,
							    left: -w2,
							    top: -h2
							});

                self.locations.push({
                    ob: ob,
                    x: parseInt(pos[0]),
                    y: parseInt(pos[1]),
                    w2: w2,
                    h2: h2,
                    w2pad: w2,
                    vis: false,
                    lab: true,
                    lpx: '0',
                    lpy: '0',
                    showAt: isNaN(ob.data('show-at-zoom')) ? self.show_at_zoom : parseInt(ob.data('show-at-zoom'), 10) / 100,
                    scale: sc
                });

                if (bg !== "") {
                    if (!bg) {
                        bg = "#000000";
                        opacity = .7;
                    }
                    var bgob = $('<div style="position: absolute; left: ' + (-w2) + 'px; top: ' + (-h2) + 'px; width: ' + ((w2 - parseInt(cont.css('padding-left'))) * 2) + 'px; height:' + ((h2 - parseInt(cont.css('padding-top'))) * 2) + 'px; background-color: ' + bg + ';"></div>').appendTo(ob);
                    if (opacity) {
                        bgob.css('opacity', opacity);
                    }
                }
            }
            ob.hide();
            if (prop_transform) {
                ob.css('opacity', 0);
            }
            if (!self.allow_drag) {
                ob.bind(self.event_down, function (e) {
                    //e.preventDefault();
                    e.stopPropagation();
                })
            }
        },

        /*Storing the original style of image (needed only when destroying)
		***********************************************************************************************************************/
        getStyle: function () {
            var el = this.$image;
            return {
                prop_origin: [prop_origin, prop_origin !== false && prop_origin !== undefined ? el.css(prop_origin) : null],
                prop_transform: [prop_transform, prop_transform !== false && prop_transform !== undefined ? el.css(prop_transform) : null],
                'position': ['position', el.css('position')],
                'z-index': ['z-index', el.css('z-index')],
                'cursor': ['cursor', el.css('cursor')],
                'left': ['left', el.css('left')],
                'top': ['top', el.css('top')],
                'width': ['width', el.css('width')],
                'height': ['height', el.css('height')]
            };
        },

        /*Find the scale ratios
		***********************************************************************************************************************/
        checkRatio: function (sW, sH, iW, iH, zoom_fit) {
            var rF;
            if (iW == sW && iH == sH) {
                rF = 1;
            } else if (iW < sW && iH < sH) {
                rF = sW / iW;
                if (zoom_fit) {
                    if (rF * iH > sH) {
                        rF = sH / iH;
                    }
                } else {
                    if (rF * iH < sH) {
                        rF = sH / iH;
                    }
                    if (sW / iW !== sH / iH && this.mouse_drag) {
                        this._moveCursor = true;
                        this.$image.css('cursor', 'move');
                        this.$hitArea.css('cursor', 'move');
                    }
                }
            } else {

                rF = sW / iW;
                if (zoom_fit) {
                    if (rF * iH > sH) {
                        rF = sH / iH;
                    }
                    if (rF < this.init_zoom && this.mouse_drag) {
                        this._moveCursor = true;
                        this.$image.css('cursor', 'move');
                        this.$hitArea.css('cursor', 'move');
                    }
                } else {
                    if (rF * iH < sH) {
                        rF = sH / iH;
                    }
                    if (sW / iW !== sH / iH && this.mouse_drag) {
                        this._moveCursor = true;
                        this.$image.css('cursor', 'move');
                        this.$hitArea.css('cursor', 'move');
                    }
                }
            }
            return rF;
        },


        /*Returns distance between 2 points (used for touch gesture)
		***********************************************************************************************************************/
        getDistance: function (x1, y1, x2, y2) {
            return Math.sqrt(Math.abs(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1))));
        },


        /*Image Events for Dragging and Mouse Wheel
		***********************************************************************************************************************/
        mouseDown: function (e) {
            var self = e.data.self;
            self._onfocus = self._dragging = false;
            if (self.cFlag._nd) {
                if (self.fixMoz) {
                    self.correctTransValue();
                }
                self.samePointRelease = false;
                if (e.type == 'mousedown') {
                    self.stX = e.pageX;
                    self.stY = e.pageY;
                    self.offX = e.pageX - self.$holder.offset().left - self.$image.position().left;
                    self.offY = e.pageY - self.$holder.offset().top - self.$image.position().top;
                    $(document).bind(self.event_move + self.id, { self: self }, self.mouseDrag);
                } else {
                    var te = e.originalEvent;
                    if (te.targetTouches.length > 1) {
                        self._pinching = true;
                        self._rA = self.rA;
                        self.dStart = self.getDistance(te.touches[0].pageX, te.touches[0].pageY, te.touches[1].pageX, te.touches[1].pageY);
                    } else {
                        self.offX = te.touches[0].pageX - self.$holder.offset().left - self.$image.position().left;
                        self.offY = te.touches[0].pageY - self.$holder.offset().top - self.$image.position().top;
                        self.setDraggedPos(te.touches[0].pageX - self.$holder.offset().left - self.offX, te.touches[0].pageY - self.$holder.offset().top - self.offY, self._sc);
                        self._recent = 'drag';
                        self._dragging = true;
                    }
                }
                self._onHitArea = true;
            }
            if (e.type == 'mousedown') {
                e.preventDefault();
            }
        },


        /*Mouse Drag / Touch swipe operations handled here
		***********************************************************************************************************************/
        mouseDrag: function (e) {
            var self = e.data.self;

            //Mouse
            if (e.type == 'mousemove') {
                self.setDraggedPos(e.pageX - self.$holder.offset().left - self.offX, e.pageY - self.$holder.offset().top - self.offY, self._sc);
                self._recent = 'drag';
                self._dragging = true;
                if (!self._playing) {
                    self.Animate();
                }
                return false;

                //Touch				
            } else {
                if (self._dragging || self._pinching) {
                    e.preventDefault();
                }
                if (self._onHitArea) {
                    var touches = e.originalEvent.touches;
                    if (self._pinching || touches.length > 1) {
                        if (!self._pinching) {
                            self._pinching = true;
                            self._rA = self.rA;
                            if (touches.length > 1) {
                                self.dStart = self.getDistance(touches[0].pageX, touches[0].pageY, touches[1].pageX, touches[1].pageY);
                            }
                        }
                        if (touches.length > 1) {
                            self._centx = (touches[0].pageX + touches[1].pageX) / 2;
                            self._centy = (touches[0].pageY + touches[1].pageY) / 2;
                            self.focusOffX = self._centx - self.$holder.offset().left - (self.sW / 2);
                            self.focusOffY = self._centy - self.$holder.offset().top - (self.sH / 2);
                            self.changeOffset(true, true);
                            self._wheel = true;
                            self._dragging = false;
                            if (self.zoom_single) {
                                self.sRed = 300;

                            } else {
                                self.dEnd = self.getDistance(touches[0].pageX, touches[0].pageY, touches[1].pageX, touches[1].pageY);
                                self.rA = self._rA * (self.dEnd / self.dStart);
                                self.rA = self.rA > self.zoom_max ? self.zoom_max : self.rA;
                                self.rA = self.rA < self.rF ? self.rF : self.rA;
                            }
                            if (self._sc < self.rA) {
                                self.cFlag._zo = false;
                                self.cFlag._zi = true;
                            } else {
                                self.cFlag._zi = false;
                                self.cFlag._zo = true;
                            }
                            if (!self._playing) {
                                self.Animate();
                            }
                        }

                    } else if (self._dragging && touches.length < 2) {
                        self.setDraggedPos(touches[0].pageX - self.$holder.offset().left - self.offX, touches[0].pageY - self.$holder.offset().top - self.offY, self._sc);
                        self._recent = 'drag';
                        if (!self._playing) {
                            self.Animate();
                        }
                    }
                }
            }
        },


        /*Global Mouse Up / Touch End
		***********************************************************************************************************************/
        mouseUp: function (e) {
            var self = e.data.self;

            //If one of the buttons released
            if (self._onButton) {
                self.$holder.find('#' + self.buttons[self.cButtId]._var + 'norm').show();
                self.$holder.find('#' + self.buttons[self.cButtId]._var + 'over').hide();
                if (self.cButtId !== 6) {
                    self.cFlag[self.buttons[self.cButtId]._var] = false;
                }
                if (e.type == 'touchend' && self.buttons[self.cButtId].$ob.css('opacity') > .5) {
                    self.buttons[self.cButtId].$ob.css('opacity', .7);
                }
                self._onButton = false;
                e.stopPropagation();
                return false;

                //If the mouse drag or touch swipe completed
            } else if (self._onHitArea) {
                if (self.mouse_drag || self.touch_drag) {
                    //Mouse					
                    if (e.type == 'mouseup') {
                        $(document).unbind(self.event_move + self.id);
                        if (self.stX == e.pageX && self.stY == e.pageY) {
                            self.samePointRelease = true;
                        }
                        self._recent = 'drag';
                        self._dragging = false;
                        if (!self._playing) {
                            self.Animate();
                        }
                        self._onHitArea = false;

                        //Touch
                    } else {
                        e.preventDefault();
                        self._dragging = false;
                        if (self._pinching) {
                            self._pinching = false;
                            self._wheel = false;
                            self.cFlag._nd = true;
                            self.cFlag._zi = false;
                            self.cFlag._zo = false;
                        } else {
                            self._recent = 'drag';
                            if (!self._playing) {
                                self.Animate();
                            }
                        }
                        self._onHitArea = false;
                    }
                }
            }
        },


        /*Mouse wheel zoom in-out
		***********************************************************************************************************************/
        mouseWheel: function (e, delta) {
            var self = e.data.self;
            self._onfocus = self._dragging = false;
            if (self.mouse_wheel_cur) {
                self.focusOffX = e.pageX - self.$holder.offset().left - (self.sW / 2);
                self.focusOffY = e.pageY - self.$holder.offset().top - (self.sH / 2);
                self.changeOffset(true, true);
            }

            self._dragging = false;
            if (delta > 0) {
                if (self.rA != self.zoom_max) {
                    if (self.zoom_single) {
                        if (!self._wheel) {
                            self.sRed = 300;
                        }
                    } else {
                        self.rA *= delta < 1 ? 1 + (.3 * delta) : 1.3;
                        self.rA = self.rA > self.zoom_max ? self.zoom_max : self.rA;
                    }
                    self._wheel = true;
                    self.cFlag._zi = true;
                    clearTimeout(self.ani_timer);
                    self._playing = true;
                    self.Animate();
                    self.cFlag._zi = false;
                }
            } else {
                if (self.rA != self.rF) {
                    if (self.zoom_single) {
                        if (!self._wheel) {
                            self.sRed = 300;
                        }
                    } else {
                        self.rA /= delta > -1 ? 1 + (.3 * -delta) : 1.3;
                        self.rA = self.rA < self.rF ? self.rF : self.rA;
                    }
                    self._wheel = true;
                    self.cFlag._zo = true;
                    clearTimeout(self.ani_timer);
                    self._playing = true;
                    self.Animate();
                    self.cFlag._zo = false;
                }
            }
            return false;
        },


        /*Control buttons Auto hide
		***********************************************************************************************************************/
        autoHide: function (e) {
            var self = e.data.self;

            clearTimeout(self.auto_timer);
            self.auto_timer = setTimeout(function () {
                self.$controls.fadeOut(600);
            }, self.bu_delay);

            self.$holder.bind('mouseenter.sz', function (e) {
                clearTimeout(self.auto_timer);
                self.$controls.fadeIn(300);
            });
        },


        /*Mozilla works differently than others when getting translated positions. So this correction needed
		***********************************************************************************************************************/
        correctTransValue: function () {
            var v = this.$image.css('-moz-transform').toString().replace(')', '').split(',');
            this.transOffX = parseInt(v[4]);
            this.transOffY = parseInt(v[5]);
        },


        /*Make sure the dragged position obeying limits
		***********************************************************************************************************************/
        setDraggedPos: function (xp, yp, s) {
            var self = this;

            if (xp !== '') {
                self.dX = xp + self.transOffX;
                if (self.pan_limit) {
                    self.dX = self.dX + (s * self.iW) < self.sW ? self.sW - (s * self.iW) : self.dX;
                    self.dX = self.dX > 0 ? 0 : self.dX;
                    if ((s * self.iW) < self.sW) {
                        self.dX = (self.sW - (s * self.iW)) / 2;
                    }
                } else {
                    self.dX = self.dX + (s * self.iW) < self.sW / 2 ? (self.sW / 2) - (s * self.iW) : self.dX;
                    self.dX = self.dX > self.sW / 2 ? self.sW / 2 : self.dX;
                }
            }
            if (yp !== '') {
                self.dY = yp + self.transOffY;
                if (self.pan_limit) {
                    self.dY = self.dY + (s * self.iH) < self.sH ? self.sH - (s * self.iH) : self.dY;
                    self.dY = self.dY > 0 ? 0 : self.dY;
                    if ((s * self.iH) < self.sH) {
                        self.dY = (self.sH - (s * self.iH)) / 2;
                    }
                } else {
                    self.dY = self.dY + (s * self.iH) < self.sH / 2 ? (self.sH / 2) - (s * self.iH) : self.dY;
                    self.dY = self.dY > self.sH / 2 ? self.sH / 2 : self.dY;
                }
            }
        },

        /*Called to animate image transformation whenever the navigation events occur
		***********************************************************************************************************************/
        Animate: function () {

            var self = this;
            var pixTol = .5;

            self.cFlag._nd = true;
            self.ani_end = false;

            //Zoom In
            if (self.cFlag._zi) {
                if (!self._wheel && !self.zoom_single) {
                    self.rA *= self.zoom_speed;
                }
                if (self.rA > self.zoom_max) {
                    self.rA = self.zoom_max;
                }
                self.cFlag._nd = false;
                self.cFlag._rs = false;
                self._recent = 'zoomIn';
                self._onfocus = self._dragging = false;
            }

            //Zoom Out
            if (self.cFlag._zo) {
                if (!self._wheel && !self.zoom_single) {
                    self.rA /= self.zoom_speed;
                }
                if (self.zoom_min != 0) {
                    if (self.rA < self.zoom_min) {
                        self.rA = self.zoom_min;
                    }
                } else {
                    if (self.rA < self.rF) {
                        self.rA = self.rF;
                    }
                }

                self.cFlag._nd = false;
                self.cFlag._rs = false;
                self._recent = 'zoomOut';
                self._onfocus = self._dragging = false;
            }

            //Zoom In or Out - Single Step
            if (self.zoom_single && !self.cFlag._rs) {
                if (self._recent == 'zoomIn') {
                    self.sRed += (10 - self.sRed) / 6;
                    self.rA += (self.zoom_max - self.rA) / (((1 / (self.pan_speed_o + 1)) * self.sRed) + 1);

                } else if (self._recent == 'zoomOut') {
                    self.sRed += (3 - self.sRed) / 3;
                    self.rA += (self.rF - self.rA) / (((1 / self.pan_speed_o + 1) * self.sRed) + 1);
                }
            }

            //Pan speed needs to adjust according to application dimension and zoom level
            self.pan_speed = (Math.max(1, 1 + ((self.sW + self.sH) / 500)) + (self.pan_speed_o * self.pan_speed_o / 4)) / Math.max(1, self.rA / 2);

            //Move Left
            if (self.cFlag._ml) {
                self.oX -= self.pan_speed;
                self.cFlag._nd = false;
                self.cFlag._rs = false;
                self._recent = 'left';
                self._onfocus = self._dragging = false;
            }

            //Move Right
            if (self.cFlag._mr) {
                self.oX += self.pan_speed;
                self.cFlag._nd = false;
                self.cFlag._rs = false;
                self._recent = 'right';
                self._onfocus = self._dragging = false;
            }

            //Move Up
            if (self.cFlag._mu) {
                self.oY -= self.pan_speed;
                self.cFlag._nd = false;
                self.cFlag._rs = false;
                self._recent = 'up';
                self._onfocus = self._dragging = false;
            }

            //Move Down
            if (self.cFlag._md) {
                self.oY += self.pan_speed;
                self.cFlag._nd = false;
                self.cFlag._rs = false;
                self._recent = 'down';
                self._onfocus = self._dragging = false;
            }

            //Reset
            if (self.cFlag._rs) {
                self.oX += (self.fX - self.oX) / 8;
                self.oY += (self.fY - self.oY) / 8;
                self.cFlag._nd = false;
                self._recent = 'reset';
                self._onfocus = self._dragging = false;
            }

            //Find scale value, width and height
            //Case 1: Single Step Zoom
            if (self.zoom_single && (self._recent !== 'reset')) {
                if (self._onfocus) {
                    self._sc += (self.rA - self._sc) / self.reduction;
                } else {
                    self._sc = self.rA;
                }

                //Case 2: Normal Zoom
            } else {
                self._sc += (self.rA - self._sc) / (self.ani_smooth / (self._onfocus ? self.reduction : 1));
            }

            self._w = self._sc * self.iW;
            self._h = self._sc * self.iH;

            //Dragging
            if (self._dragging) {
                self.tX = self.dX;
                self.tY = self.dY;
                self.changeOffset(true, true);
            }

            //Check if Zoom In completed
            if (self._recent == "zoomIn") {
                if (self._w > (self.rA * self.iW) - pixTol && !self.zoom_single) {
                    if (self.cFlag._nd) {
                        self.ani_end = true;
                    }
                    self._sc = self.rA;
                } else if (self._w > (self.zoom_max * self.iW) - pixTol && self.zoom_single) {
                    if (self.cFlag._nd) {
                        self.ani_end = true;
                    }
                    self._sc = self.rA = self.zoom_max;
                }
                if (self.ani_end) {
                    self._w = self._sc * self.iW;
                    self._h = self._sc * self.iH;
                }

                //Check if Zoom Out completed
            } else if (self._recent == "zoomOut") {
                if (self._w < (self.rA * self.iW) + pixTol && !self.zoom_single) {
                    if (self.cFlag._nd) {
                        self.ani_end = true;
                    }
                    self._sc = self.rA;
                } else if (self._w < (self.rF * self.iW) + pixTol && self.zoom_single) {
                    if (self.cFlag._nd) {
                        self.ani_end = true;
                    }
                    self._sc = self.rA = self.rF;
                }
                if (self.ani_end) {
                    self._w = self._sc * self.iW;
                    self._h = self._sc * self.iH;
                }
            }

            //Move image according to boundary limits
            self.limitX = (((self._w - self.sW) / (self._w / self.sW)) / 2);
            self.limitY = (((self._h - self.sH) / (self._h / self.sH)) / 2);

            if (!self._dragging) {
                if (self.pan_limit) {
                    if (self.oX < -self.limitX - self.focusOffX) {
                        self.oX = -self.limitX - self.focusOffX;
                    }
                    if (self.oX > self.limitX - self.focusOffX) {
                        self.oX = self.limitX - self.focusOffX;
                    }
                    if (self._w < self.sW) {
                        self.tX = (self.sW - self._w) / 2;
                        self.changeOffset(true, false);
                    }
                    if (self.oY < -self.limitY - self.focusOffY) {
                        self.oY = -self.limitY - self.focusOffY;
                    }
                    if (self.oY > self.limitY - self.focusOffY) {
                        self.oY = self.limitY - self.focusOffY;
                    }
                    if (self._h < self.sH) {
                        self.tY = (self.sH - self._h) / 2;
                        self.changeOffset(false, true);
                    }
                } else {
                    if (self.oX < -self.limitX - (self.focusOffX / self._w * self.sW) - ((self.sW / 2) / (self._w / self.sW))) {
                        self.oX = -self.limitX - (self.focusOffX / self._w * self.sW) - ((self.sW / 2) / (self._w / self.sW));
                    }

                    if (self.oX > self.limitX - (self.focusOffX / self._w * self.sW) + ((self.sW / 2) / (self._w / self.sW))) {
                        self.oX = self.limitX - (self.focusOffX / self._w * self.sW) + ((self.sW / 2) / (self._w / self.sW));
                    }

                    if (self.oY < -self.limitY - (self.focusOffY / self._h * self.sH) - (self.sH / (self._h / self.sH * 2))) {
                        self.oY = -self.limitY - (self.focusOffY / self._h * self.sH) - (self.sH / (self._h / self.sH * 2));
                    }

                    if (self.oY > self.limitY - (self.focusOffY / self._h * self.sH) + (self.sH / (self._h / self.sH * 2))) {
                        self.oY = self.limitY - (self.focusOffY / self._h * self.sH) + (self.sH / (self._h / self.sH * 2));
                    }
                }
            }
            if (!self._dragging && self._recent != "drag") {
                self.tX = ((self.sW - self._w) / 2) + self.focusOffX + (self.oX * (self._w / self.sW));
                self.tY = ((self.sH - self._h) / 2) + self.focusOffY + (self.oY * (self._h / self.sH));
                if (self.ani_smooth === 1) {
                    self.cFlag._nd = true;
                    self.ani_end = true;
                }
            }
            if (self._recent == "zoomIn" || self._recent == "zoomOut" || self.cFlag._rs) {
                self._x = self.tX;
                self._y = self.tY;
            } else {
                self._x += (self.tX - self._x) / (self.ani_smooth / (self._onfocus ? self.reduction : 1));
                self._y += (self.tY - self._y) / (self.ani_smooth / (self._onfocus ? self.reduction : 1));
            }

            //Check if Left movement completed
            if (self._recent == "left") {
                if (self._x < self.tX + pixTol || self.ani_smooth === 1) {
                    self.cFlag._nd ? self.ani_end = true : "";
                    self._recent = '';
                    self._x = self.tX;
                }

                //Check if Right  movement completed
            } else if (self._recent == "right") {
                if (self._x > self.tX - pixTol || self.ani_smooth === 1) {
                    self.cFlag._nd ? self.ani_end = true : "";
                    self._recent = '';
                    self._x = self.tX;
                }

                //Check if Up movement completed
            } else if (self._recent == "up") {
                if (self._y < self.tY + pixTol || self.ani_smooth === 1) {
                    self.cFlag._nd ? self.ani_end = true : "";
                    self._recent = '';
                    self._y = self.tY;
                }

                //Check if Down movement completed
            } else if (self._recent == "down") {
                if (self._y > self.tY - pixTol || self.ani_smooth === 1) {
                    self.cFlag._nd ? self.ani_end = true : "";
                    self._recent = '';
                    self._y = self.tY;
                }

                //Check if Dragging completed
            } else if (self._recent == "drag") {
                if (self._x + pixTol >= self.tX && self._x - pixTol <= self.tX && self._y + pixTol >= self.tY && self._y - pixTol <= self.tY || self.ani_smooth === 1) {
                    if (self._onfocus) {
                        self._dragging = false;
                    }
                    self.cFlag._nd ? self.ani_end = true : "";
                    self._recent = '';
                    self._x = self.tX;
                    self._y = self.tY;
                }
            }

            //Check if Reset completed
            if (self.cFlag._rs && self._w + pixTol >= (self.rA * self.iW) && self._w - pixTol <= (self.rA * self.iW) && self.oX <= self.fX + pixTol && self.oX >= self.fX - pixTol && self.oY <= self.fY + pixTol && self.oY >= self.fY - pixTol) {
                self.ani_end = true;
                self._recent = '';
                self.cFlag._rs = false;
                self.cFlag._nd = true;
                self._x = self.tX;
                self._y = self.tY;
                self._sc = self.rA;
                self._w = self._sc * self.iW;
                self._h = self._sc * self.iH;
            }

            //When the image fits exactly to container size, disable the pan, zoom out and reset buttons
            if (self.rA == self.rF && self.iW * self.rA <= self.sW && self.iH * self.rA <= self.sH) {
                if (self.buttons[1].$ob.css('opacity') > .5) {
                    if (self.rA >= self.rF && (self.init_zoom == '' || self.rA < self.init_zoom) && (self.zoom_min == '' || self.rA < self.zoom_min)) {
                        if (self.pan_limit && self._moveCursor && !self._moveCursor) {
                            self.$image.css('cursor', 'default');
                            self.$hitArea.css('cursor', 'default');
                        }
                        for (var bEn = 1; bEn < (self.pan_limit && !self._moveCursor ? self.buttons_total : 2) ; bEn++) {
                            self.buttons[bEn].$ob.css({
                                'opacity': .4
                            });
                            self._wheel = false;
                            self.$holder.find('#' + self.buttons[bEn]._var + 'norm').show();
                            self.$holder.find('#' + self.buttons[bEn]._var + 'over').hide();
                        }
                    }
                }

            } else {
                if (self.buttons[1].$ob.css('opacity') < .5) {
                    if (self._moveCursor && self.mouse_drag) {
                        self.$image.css('cursor', 'move');
                        self.$hitArea.css('cursor', 'move');
                    }
                    for (var bEn = 1; bEn < self.buttons_total; bEn++) {
                        self.buttons[bEn].$ob.css('opacity', .7);
                    }
                }
            }

            //When the image reaches max zoom, disable the zoom button
            if (self.rA == self.zoom_max) {
                if (self.buttons[0].$ob.css('opacity') > .5) {
                    self.buttons[0].$ob.css('opacity', .4);
                    self._wheel = false;
                    self.$holder.find('#' + self.buttons[0]._var + 'norm').show();
                    self.$holder.find('#' + self.buttons[0]._var + 'over').hide();
                }

            } else {
                if (self.buttons[0].$ob.css('opacity') < .5) {
                    self.buttons[0].$ob.css('opacity', .7);
                }
            }

            //Apply Scale and position to the image:
            if (prop_transform) {
                self.$image.css(prop_transform, 'translate(' + self._x.toFixed(14) + 'px,' + self._y.toFixed(14) + 'px) scale(' + self._sc + ')');

            } else {
                self.$image.css({
                    left: self._x,
                    top: self._y,
                    width: self._w,
                    height: self._h
                });
            }

            if (self.$loc_cont) {
                self.updateLocations(self._x, self._y, self._sc, self.locations);
            }

            //In case image Maps used, update them
            if (!prop_transform && self.map_coordinates.length > 0) {
                self.updateMap();
            }
            //If the animation completed, stop running; else continue	
            if (self.ani_end && !self._dragging && self._recent != "drag") {
                self._playing = false;
                self._recent = '';
                self.cX = (-self._x + (self.sW / 2)) / self.rA;
                self.cY = (-self._y + (self.sH / 2)) / self.rA;
                if (self.onUPDATE) {
                    self.onUPDATE(self.getZoomData(), false);
                }
                if (self.onZOOM_PAN) {
                    self.onZOOM_PAN(self.getZoomData());
                }
                clearTimeout(self.ani_timer);
            } else {
                self._playing = true;
                if (self.onUPDATE) {
                    self.onUPDATE(self.getZoomData(), true);
                }
                self.ani_timer = setTimeout(function () {
                    self.Animate();
                }, 28);
            }
        },


        /*Relocate the landmarks according to main image's position
		***********************************************************************************************************************/
        updateLocations: function (_x, _y, _sc, loc) {

            if (this.onLANDMARK !== '') {
                if (_sc >= this.show_at_zoom) {
                    if (!this._landmark) {
                        this._landmark = true
                        this.onLANDMARK(true);
                    }
                } else {
                    if (this._landmark) {
                        this._landmark = false;
                        this.onLANDMARK(false);
                    }
                }
            }

            for (var p = 0; p < loc.length; p++) {
                var wScaled,
					hScaled,
					lpx = (loc[p].x * _sc) + _x,
					lpy = (loc[p].y * _sc) + _y;

                if (_sc >= loc[p].showAt) {
                    if (loc[p].scale && prop_transform) {
                        wScaled = loc[p].w2pad * this._sc;
                        hScaled = loc[p].h2 * this._sc;
                    } else {
                        wScaled = loc[p].w2pad;
                        hScaled = loc[p].h2;
                    }
                    if (lpx > -wScaled && lpx < this.sW + wScaled && ((lpy > -hScaled && lpy < this.sH + hScaled && loc[p].lab) || (lpy > 0 && lpy < this.sH + (hScaled * 2) && !loc[p].lab))) {
                        if (!loc[p].vis) {
                            loc[p].vis = true;
                            if (prop_transform) {
                                loc[p].ob.stop()
									.css('display', 'block')
									.animate({
									    opacity: 1
									}, 300);
                            } else {
                                loc[p].ob.show();
                            }
                        }
                    } else {
                        if (loc[p].vis) {
                            loc[p].vis = false;
                            if (prop_transform) {
                                loc[p].ob.stop()
									.animate({
									    opacity: 0
									}, 200, function () {
									    $(this).hide();
									});
                            } else {
                                loc[p].ob.hide();
                            }
                        }
                    }
                } else {
                    if (loc[p].vis) {
                        loc[p].vis = false;
                        if (prop_transform) {
                            loc[p].ob.stop()
								.animate({
								    opacity: 0
								}, 200, function () {
								    $(this).hide();
								});
                        } else {
                            loc[p].ob.hide();
                        }
                    }
                }
                if (lpx !== loc[p].lpx || lpy !== loc[p].lpy && loc[p].vis) {
                    if (prop_transform) {
                        loc[p].ob.css(prop_transform, 'translate(' + lpx.toFixed(14) + 'px,' + lpy.toFixed(14) + 'px)' + (loc[p].scale ? ' scale(' + this._sc + ')' : ''));
                    } else {
                        loc[p].ob.css({
                            left: lpx,
                            top: lpy
                        });
                    }
                }
                loc[p].lpx = lpx;
                loc[p].lpy = lpy;
            }
        },


        /*If the broswer doesn't supports css border radius, we need to go with old school png image for rounded corner
		***********************************************************************************************************************/
        roundBG: function (el, _name, _w, _h, _r, _p, _c, _i, _z, _yoff) {
            var w = 50 / 2;

            el.append($(
				'<div class="bgi' + _name + '" style="background-position:' + (-(_p - _r)) + 'px ' + (-(w - _r) - _yoff) + 'px"></div>\
				<div class="bgh' + _name + '"></div>\
				<div class="bgi' + _name + '" style="background-position:' + (-_p) + 'px ' + (-(w - _r) - _yoff) + 'px; left:' + (_w - _r) + 'px"></div>\
				<div class="bgi' + _name + '" style="background-position:' + (-(_p - _r)) + 'px ' + (-w - _yoff) + 'px; top:' + (_h - _r) + 'px"></div>\
				<div class="bgh' + _name + '" style = "top:' + (_h - _r) + 'px; left:' + _r + 'px"></div>\
				<div class="bgi' + _name + '" style="background-position:' + (-_p) + 'px ' + (-w - _yoff) + 'px; top:' + (_h - _r) + 'px; left:' + (_w - _r) + 'px"></div>\
				<div class="bgc' + _name + '"></div>'
			));
            $('.bgi' + _name).css({
                position: 'absolute',
                width: _r,
                height: _r,
                'background-image': 'url(' + _i + ')',
                'background-repeat': 'no-repeat',
                '-ms-filter': 'progid:DXImageTransform.Microsoft.gradient(startColorstr=#00FFFFFF,endColorstr=#00FFFFFF)',
                'filter': 'progid:DXImageTransform.Microsoft.gradient(startColorstr=#00FFFFFF,endColorstr=#00FFFFFF)',
                'zoom': 1
            });
            $('.bgh' + _name).css({
                position: 'absolute',
                width: _w - _r * 2,
                height: _r,
                'background-color': _c,
                left: _r
            });
            $('.bgc' + _name).css({
                position: 'absolute',
                width: _w,
                height: _h - _r * 2,
                'background-color': _c,
                top: _r,
                left: 0
            });
        },


        /*To calibrate position offset when navigation events supposed to be overlapped 
		***********************************************************************************************************************/
        changeOffset: function (x, y) {
            if (x) this.oX = (this.tX - ((this.sW - this._w) / 2) - this.focusOffX) / (this._w / this.sW);
            if (y) this.oY = (this.tY - ((this.sH - this._h) / 2) - this.focusOffY) / (this._h / this.sH);
        },


        /*Transform Image Maps (hot spots) if any
		***********************************************************************************************************************/
        updateMap: function () {
            var self = this,
				mapId = 0;

            self.mapAreas.each(function () {
                var new_vals = [];
                for (var i = 0; i < self.map_coordinates[mapId].length; i++) {
                    new_vals[i] = self.map_coordinates[mapId][i] * self._sc;
                }
                new_vals = new_vals.join(",");
                $(this).attr('coords', new_vals);
                mapId++;
            });
        },


        /*To stop the timer loops immediatly
		***********************************************************************************************************************/
        haltAnimation: function () {
            clearTimeout(this.ani_timer);
            this._playing = false;
            this._recent = '';
        },


        /*Method to Remove the plugin instance
		***********************************************************************************************************************/
        destroy: function () {
            var self = this;

            if (self.assetsLoaded) {
                self.haltAnimation();
                for (prop in self.orig_style) {
                    if (self.orig_style[prop][0] !== false && self.orig_style[prop][0] !== undefined) {
                        if (self.orig_style[prop][0] === 'width' || self.orig_style[prop][0] === 'height') {
                            if (parseInt(self.orig_style[prop][1]) !== 0) {
                                self.$image.css(self.orig_style[prop][0], self.orig_style[prop][1]);
                            }
                        } else {
                            self.$image.css(self.orig_style[prop][0], self.orig_style[prop][1]);
                        }
                    }
                }
                clearTimeout(self.auto_timer);
                $(document).unbind('.sz' + self.id);
                $(window).unbind('.sz' + self.id);
                self.$holder.unbind('.sz');
                self.$controls = undefined;
            } else {
                self.$image.show();
            }

            if (self.container == '') {
                if (self.image_url == '') {
                    self.$image.insertBefore(self.$holder);
                    if (self.$holder !== undefined) {
                        self.$holder.remove();
                    }
                } else {
                    self.$elem.empty();
                    if (self.$loc_cont[0]) {
                        self.$elem.append(self.loc_clone);
                    }
                }
            } else {
                self.$image.insertBefore(self.$holder);
                self.$holder.empty();
                self.$image.wrap(self.$holder);
                if (self.$loc_cont[0]) {
                    self.$holder.append(self.loc_clone);
                }
            }
            self.$elem.removeData('smoothZoom');
            self.$holder = undefined;
            self.Buttons = undefined;
            self.op = undefined;
            self.$image = undefined;
        },


        /*Method to change focus point and level
		***********************************************************************************************************************/
        focusTo: function (params) {
            var self = this;

            if (self.assetsLoaded) {
                if (params.zoom === undefined || params.zoom === '' || params.zoom == 0) {
                    params.zoom = self.rA;
                } else {
                    params.zoom /= 100;
                }
                self._onfocus = true;
                if (params.zoom > self.rA && self.rA != self.zoom_max) {
                    self.rA = params.zoom;
                    self.rA = self.rA > self.zoom_max ? self.zoom_max : self.rA;
                } else if (params.zoom < self.rA && self.rA != self.rF) {
                    self.rA = params.zoom;
                    self.rA = self.rA < self.rF ? self.rF : self.rA;
                }
                self.transOffX = self.transOffY = 0;
                self.setDraggedPos(params.x === undefined || params.x === '' ? "" : (-params.x * self.rA) + (self.sW / 2), params.y === undefined || params.y === '' ? "" : (-params.y * self.rA) + (self.sH / 2), self.rA);
                self.reduction = params.speed ? params.speed / 10 : self.focusSpeed;
                self._recent = 'drag';
                self._dragging = true;
                if (!self._playing) {
                    self.Animate();
                }
            }
        },

        zoomIn: function (params) {
            this.buttons[0].$ob.trigger(this.event_down, {
                id: 0
            });
        },

        zoomOut: function (params) {
            this.buttons[1].$ob.trigger(this.event_down, {
                id: 1
            });
        },

        moveRight: function (params) {
            this.buttons[2].$ob.trigger(this.event_down, {
                id: 2
            });
        },

        moveLeft: function (params) {
            this.buttons[3].$ob.trigger(this.event_down, {
                id: 3
            });
        },

        moveUp: function (params) {
            this.buttons[4].$ob.trigger(this.event_down, {
                id: 4
            });
        },

        moveDown: function (params) {
            this.buttons[5].$ob.trigger(this.event_down, {
                id: 5
            });
        },

        Reset: function (params) {
            this.buttons[6].$ob.trigger(this.event_down, {
                id: 6
            });
        },

        getZoomData: function (params) {
            return {
                //x offset (without scale ratio multiplied)
                normX: (-this._x / this.rA).toFixed(14),

                //y offset (without scale ratio multiplied)
                normY: (-this._y / this.rA).toFixed(14),

                //Original image Width
                normWidth: this.iW,

                //Original image height
                normHeight: this.iH,

                //x offset (with scale ratio multiplied)
                scaledX: -this._x.toFixed(14),

                //y offset (with scale ratio multiplied)
                scaledY: -this._y.toFixed(14),

                //Scaled image width
                scaledWidth: this._w,

                //Scaled image height
                scaledHeight: this._h,

                //The X location on image which is currently on center of canvas 
                centerX: (-this._x.toFixed(14) + (this.sW / 2)) / this.rA,

                //The Y location on image which is currently on center of canvas 
                centerY: (-this._y.toFixed(14) + (this.sH / 2)) / this.rA,

                //Scale ratio
                ratio: this.rA
            };
        },

        addLandmark: function (loc) {
            if (this.$loc_cont) {
                var total = loc.length;

                for (var i = 0; i < total; i++) {
                    var $loc = $(loc[i]);
                    this.$loc_cont.append($loc);
                    this.setLocation($loc);
                }

                if (total > 0) {
                    this.updateLocations(this._x, this._y, this._sc, this.locations);
                }
            }
        },

        attachLandmark: function (loc) {
            if (this.$loc_cont) {
                var total = loc.length;
                for (var i = 0; i < total; i++) {
                    this.setLocation(loc[i] instanceof jQuery ? loc[i] : $('#' + loc[i]));
                }
                if (total > 0) {
                    this.updateLocations(this._x, this._y, this._sc, this.locations);
                }
            }
        },

        removeLandmark: function (loc) {
            if (this.$loc_cont) {
                if (loc) {
                    var total = loc.length;
                    for (var i = 0; i < total; i++) {
                        for (var j = 0; j < this.locations.length; j++) {
                            if ((loc[i] instanceof jQuery && this.locations[j].ob[0] == loc[i][0]) || (!(loc[i] instanceof jQuery) && this.locations[j].ob.attr('id') == loc[i])) {
                                this.locations[j].ob.remove();
                                this.locations.splice(j, 1);
                                j--;
                            }
                        }
                    }
                } else {
                    if (this.locations.length > 0) {
                        this.locations[this.locations.length - 1].ob.remove();
                        this.locations.pop();
                    }
                }
                if (total > 0) {
                    this.updateLocations(this._x, this._y, this._sc, this.locations);
                }
            }
        },

        refreshAllLandmarks: function () {
            var self = this;
            var locs = self.$loc_cont.children('.item');
            self.show_at_zoom = parseInt(self.$loc_cont.data('show-at-zoom'), 10) / 100;
            self.allow_scale = checkBoolean(self.$loc_cont.data('allow-scale'));
            self.allow_drag = checkBoolean(self.$loc_cont.data('allow-drag'));

            //Step 1: Remove records for which the elements no longer exist
            for (var i = 0; i < self.locations.length; i++) {
                var exists = false;
                locs.each(function () {
                    if (self.locations[i].ob[0] == $(this)[0]) {
                        exists = true;
                    }
                });
                if (!exists) {
                    self.locations.splice(i, 1);
                    i--;
                }
            }

            //Step 2: Add new elements to record
            locs.each(function () {
                var exists = false;
                for (var i = 0; i < self.locations.length; i++) {
                    if (self.locations[i].ob[0] == $(this)[0]) {
                        exists = true;
                        break;
                    }
                }
                if (!exists) {
                    self.setLocation($(this));
                }

            });
            this.updateLocations(this._x, this._y, this._sc, this.locations);
        },


        /*On windows resize, adjust some defaults
		***********************************************************************************************************************/
        resize: function (e) {
            var self;

            if (e.data) {
                e.preventDefault();
                self = e.data.self;
                var pw = self.$holder.parent().width();
                var ph = self.$holder.parent().height();

                if (self.oW) {
                    pw = Math.min(pw, self.oW);
                }
                self.sW = pw;

                if (self.oH) {
                    if (self.oW && self.maintain_ratio) {
                        self.sH = pw / (self.oW / self.oH);
                    }
                } else {
                    self.sH = ph;
                }


            } else {
                self = this;
                if (e.width) {
                    self.sW = e.width;
                }
                if (e.height) {
                    self.sH = e.height;
                }
                if (e.max_WIDTH) {
                    self.w_max = e.max_WIDTH;
                }
                if (e.max_HEIGHT) {
                    self.h_max = e.max_HEIGHT;
                }
            }
            if (self.w_max !== 0 && self.w_max !== '') {
                self.sW = Math.min(self.sW, self.w_max);
            }
            if (self.h_max !== 0 && self.h_max !== '') {
                self.sH = Math.min(self.sH, self.h_max);
            }
            self.$holder.css({
                'width': self.sW,
                'height': self.sH
            });
            if (self.bord_size > 0) {
                self.border[0].height(self.sH);
                self.border[1].css({
                    height: self.sH,
                    left: self.sW - self.bord_size
                });
                self.border[2].width(self.sW - (self.bord_size * 2));
                self.border[3].css({
                    width: self.sW - (self.bord_size * 2),
                    top: self.sH - self.bord_size
                });
            }
            if (self.bu_align[1] == 'center') {
                self.$controls.css('left', parseInt((self.sW - self.cBW) / 2));
            }
            if (self.bu_align[0] == 'center') {
                self.$controls.css('top', parseInt((self.sH - self.cBH) / 2));
            }
            self.rF = self.rR = self.checkRatio(self.sW, self.sH, self.iW, self.iH, self.zoom_fit);
            if (self.zoom_min == 0) {
                if (self.rA < self.rF) {
                    self.rA = self.rF;
                }
            }

            self.focusTo({
                x: self.cX,
                y: self.cY,
                zoom: '',
                speed: 10
            });
        }
    }


    $.fn.smoothZoom = function (params) {
        var self = this;
        var l = self.length;

        //For single or more than one plugin instance
        for (var i = 0; i < l; i++) {
            var $elem = $(self[i]);
            var instance = $elem.data('smoothZoom');

            // Case 1: Initiate the plugin if not already have an instance
            if (!instance) {

                if (typeof params === 'object' || !params) {
                    $elem.data('smoothZoom', new Zoomer($elem, params));
                }

                // Case 2: If the instance already available - Check for method call
            } else {

                // getZoomData - Returns {sourceX, sourceY, sourceWidth, sourceHeight, distX, distY, distWidth, distHeight, centerX, centerY, ratio}				
                if (params == "getZoomData") {
                    return instance[params].apply(instance, Array.prototype.slice.call(arguments, 1));

                    // destroy | focusTo | zoomIn | zoomOut | moveRight| moveLeft | moveUp | moveDown | Reset | addLandmark | removeLandmark | attachLandmark | refreshAllLandmarks
                } else if (instance[params]) {
                    instance[params].apply(instance, Array.prototype.slice.call(arguments, 1));
                }
            }
        }

        //return for chainability for possible cases
        if (params !== "getZoomData") {
            return this;
        }
    };

    function checkBoolean(_var) {
        if (_var === true) {
            return true;
        } else if (_var) {
            _var = _var.toLowerCase();
            if (_var == 'yes' || _var == 'true') {
                return true;
            }
        }
        return false;
    }


    //...................................................................................................................
    //Using Modernizr to check browser capabilities and property names prefixed

    /* Modernizr 2.5.3 (Custom Build) | MIT & BSD
	 * Build: http://www.modernizr.com/download/#-borderradius-csstransforms-csstransforms3d-touch-prefixed-teststyles-testprop-testallprops-prefixes-domprefixes
 	*/
    var Modernizr = function (a, b, c) { function y(a) { i.cssText = a } function z(a, b) { return y(l.join(a + ";") + (b || "")) } function A(a, b) { return typeof a === b } function B(a, b) { return !!~("" + a).indexOf(b) } function C(a, b) { for (var d in a) if (i[a[d]] !== c) return b == "pfx" ? a[d] : !0; return !1 } function D(a, b, d) { for (var e in a) { var f = b[a[e]]; if (f !== c) return d === !1 ? a[e] : A(f, "function") ? f.bind(d || b) : f } return !1 } function E(a, b, c) { var d = a.charAt(0).toUpperCase() + a.substr(1), e = (a + " " + n.join(d + " ") + d).split(" "); return A(b, "string") || A(b, "undefined") ? C(e, b) : (e = (a + " " + o.join(d + " ") + d).split(" "), D(e, b, c)) } var d = "2.5.3", e = {}, f = b.documentElement, g = "modernizr", h = b.createElement(g), i = h.style, j, k = {}.toString, l = " -webkit- -moz- -o- -ms- ".split(" "), m = "Webkit Moz O ms", n = m.split(" "), o = m.toLowerCase().split(" "), p = {}, q = {}, r = {}, s = [], t = s.slice, u, v = function (a, c, d, e) { var h, i, j, k = b.createElement("div"), l = b.body, m = l ? l : b.createElement("body"); if (parseInt(d, 10)) while (d--) j = b.createElement("div"), j.id = e ? e[d] : g + (d + 1), k.appendChild(j); return h = ["&#173;", "<style>", a, "</style>"].join(""), k.id = g, (l ? k : m).innerHTML += h, m.appendChild(k), l || (m.style.background = "", f.appendChild(m)), i = c(k, a), l ? k.parentNode.removeChild(k) : m.parentNode.removeChild(m), !!i }, w = {}.hasOwnProperty, x; !A(w, "undefined") && !A(w.call, "undefined") ? x = function (a, b) { return w.call(a, b) } : x = function (a, b) { return b in a && A(a.constructor.prototype[b], "undefined") }, Function.prototype.bind || (Function.prototype.bind = function (b) { var c = this; if (typeof c != "function") throw new TypeError; var d = t.call(arguments, 1), e = function () { if (this instanceof e) { var a = function () { }; a.prototype = c.prototype; var f = new a, g = c.apply(f, d.concat(t.call(arguments))); return Object(g) === g ? g : f } return c.apply(b, d.concat(t.call(arguments))) }; return e }); var F = function (c, d) { var f = c.join(""), g = d.length; v(f, function (c, d) { var f = b.styleSheets[b.styleSheets.length - 1], h = f ? f.cssRules && f.cssRules[0] ? f.cssRules[0].cssText : f.cssText || "" : "", i = c.childNodes, j = {}; while (g--) j[i[g].id] = i[g]; e.touch = "ontouchstart" in a || a.DocumentTouch && b instanceof DocumentTouch || (j.touch && j.touch.offsetTop) === 9, e.csstransforms3d = (j.csstransforms3d && j.csstransforms3d.offsetLeft) === 9 && j.csstransforms3d.offsetHeight === 3 }, g, d) }([, ["@media (", l.join("touch-enabled),("), g, ")", "{#touch{top:9px;position:absolute}}"].join(""), ["@media (", l.join("transform-3d),("), g, ")", "{#csstransforms3d{left:9px;position:absolute;height:3px;}}"].join("")], [, "touch", "csstransforms3d"]); p.touch = function () { return e.touch }, p.borderradius = function () { return E("borderRadius") }, p.csstransforms = function () { return !!E("transform") }, p.csstransforms3d = function () { var a = !!E("perspective"); return a && "webkitPerspective" in f.style && (a = e.csstransforms3d), a }; for (var G in p) x(p, G) && (u = G.toLowerCase(), e[u] = p[G](), s.push((e[u] ? "" : "no-") + u)); return y(""), h = j = null, e._version = d, e._prefixes = l, e._domPrefixes = o, e._cssomPrefixes = n, e.testProp = function (a) { return C([a]) }, e.testAllProps = E, e.testStyles = v, e.prefixed = function (a, b, c) { return b ? E(a, b, c) : E(a, "pfx") }, e }(this, this.document);

    var prop_transform = Modernizr.prefixed('transform');
    var prop_origin = Modernizr.prefixed('transformOrigin');
    var prop_radius = Modernizr.prefixed('borderRadius');
    var supportsTrans3D = Modernizr.csstransforms3d;
    var supportsTouch = Modernizr.touch;

})(jQuery, window, document);

//End - smoothZoom

//...................................................................................................................
//For mouse wheel support

/*! Copyright (c) 2011 Brandon Aaron (http://brandonaaron.net)
 * Licensed under the MIT License (LICENSE.txt).
 *
 * Thanks to: http://adomas.org/javascript-mouse-wheel/ for some pointers.
 * Thanks to: Mathias Bank(http://www.mathias-bank.de) for a scope bug fix.
 * Thanks to: Seamus Leahy for adding deltaX and deltaY
 *
 * Version: 3.0.6
 * 
 * Requires: 1.2.2+
 */
(function (a) { function d(b) { var c = b || window.event, d = [].slice.call(arguments, 1), e = 0, f = !0, g = 0, h = 0; return b = a.event.fix(c), b.type = "mousewheel", c.wheelDelta && (e = c.wheelDelta / 120), c.detail && (e = -c.detail / 3), h = e, c.axis !== undefined && c.axis === c.HORIZONTAL_AXIS && (h = 0, g = -1 * e), c.wheelDeltaY !== undefined && (h = c.wheelDeltaY / 120), c.wheelDeltaX !== undefined && (g = -1 * c.wheelDeltaX / 120), d.unshift(b, e, g, h), (a.event.dispatch || a.event.handle).apply(this, d) } var b = ["DOMMouseScroll", "mousewheel"]; if (a.event.fixHooks) for (var c = b.length; c;) a.event.fixHooks[b[--c]] = a.event.mouseHooks; a.event.special.mousewheel = { setup: function () { if (this.addEventListener) for (var a = b.length; a;) this.addEventListener(b[--a], d, !1); else this.onmousewheel = d }, teardown: function () { if (this.remove_eventListener) for (var a = b.length; a;) this.remove_eventListener(b[--a], d, !1); else this.onmousewheel = null } }, a.fn.extend({ mousewheel: function (a) { return a ? this.bind("mousewheel", a) : this.trigger("mousewheel") }, unmousewheel: function (a) { return this.unbind("mousewheel", a) } }) })(jQuery)

//...................................................................................................................