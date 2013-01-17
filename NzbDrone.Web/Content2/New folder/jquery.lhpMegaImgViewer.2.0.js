// Mega Image Viewer v2.0 - jQuery image viewer plugin - converting <div> element to an animated image viewer
// (c) 2012 lhp - http://codecanyon.net/user/lhp
(function (f) {
    var c, h, e;
    h = {
        dragSmooth: 8
    };
    e = {
        viewportWidth: "100%",
        viewportHeight: "100%",
        fitToViewportShortSide: false,
        contentSizeOver100: false,
        startScale: 1,
        startX: 0,
        startY: 0,
        animTime: 500,
        draggInertia: 10,
        contentUrl: "",
        intNavEnable: true,
        intNavPos: "T",
        intNavAutoHide: false,
        intNavMoveDownBtt: true,
        intNavMoveUpBtt: true,
        intNavMoveRightBtt: true,
        intNavMoveLeftBtt: true,
        intNavZoomBtt: true,
        intNavUnzoomBtt: true,
        intNavFitToViewportBtt: true,
        intNavFullSizeBtt: true,
        mapEnable: true,
        mapThumb: null,
        mapPos: "BL",
        popupShowAction: "rollover",
        testMode: false
    };
    c = {
        init: function (m, l) {
            return this.each(function () {
                var q = f(this),
                    p = q.data("lhpMIV"),
                    o = q.find("img"),
                    n = {};
                f.extend(n, e, m);
                f.extend(n, h);
                if (!p) {
                    if (n.draggInertia < 0) {
                        n.draggInertia = 0
                    }
                    n.animTime = parseInt(n.animTime);
                    if (n.animTime < 0) {
                        n.animTime = 0
                    }
                    if (o.length > 0) {
                        n.contentUrl = o[0].src;
                        o.remove()
                    }
                    q.data("lhpMIV", {});
                    q.data("lhpMIV").interImgsTmp = o;
                    q.data("lhpMIV").lc = new d(n, q, l)
                }
            })
        },
        setPosition: function (l, o, n, m) {
            return this.each(function () {
                var q = f(this),
                    p = q.data("lhpMIV");
                if (p) {
                    q.data("lhpMIV").lc.setProperties(l, o, n, m)
                }
            })
        },
        moveUp: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.beginDirectMove("U")
                }
            })
        },
        moveDown: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.beginDirectMove("D")
                }
            })
        },
        moveLeft: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.beginDirectMove("L")
                }
            })
        },
        moveRight: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.beginDirectMove("R")
                }
            })
        },
        moveStop: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.stopDirectMoving()
                }
            })
        },
        zoom: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.beginZooming("Z")
                }
            })
        },
        unzoom: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.beginZooming("U")
                }
            })
        },
        zoomStop: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.stopZooming()
                }
            })
        },
        fitToViewport: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.setProperties(null, null, 0)
                }
            })
        },
        fullSize: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.setProperties(null, null, 1)
                }
            })
        },
        adaptsToContainer: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.adaptsToContainer()
                }
            })
        },
        getCurrentState: function () {
            var n = f(this),
                m = n.data("lhpMIV"),
                l = {};
            if (m) {
                l = n.data("lhpMIV").lc.getCurrentState()
            }
            return l
        },
        destroy: function () {
            return this.each(function () {
                var m = f(this),
                    l = m.data("lhpMIV");
                if (l) {
                    m.data("lhpMIV").lc.destroy();
                    m.prepend(m.data("lhpMIV").interImgsTmp);
                    m.removeData("lhpMIV")
                }
            })
        }
    };
    f.fn.lhpMegaImgViewer = function (l) {
        if (c[l]) {
            return c[l].apply(this, Array.prototype.slice.call(arguments, 1))
        } else {
            if (typeof l === "object" || !l) {
                return c.init.apply(this, arguments)
            } else {
                f.error("Method " + l + " does not exist on jQuery.lhpMegaImgViewer")
            }
        }
    };
    var d = function (m, n, l) {
        this.isTouchDev = (typeof (window.ontouchstart) != "undefined") ? true : false;
        this.sett = m;
        this.$mainHolder = n;
        this.lastMousePageCoor = null;
        this.lastDrag = null;
        this.contentFullSize = {};
        this.$mivHol = null;
        this.$contentHol = null;
        this.$content = null;
        this.$preloadHol = null;
        this.$blackScreen = null;
        this.$infoBox = null;
        this.$navHol = null;
        this.movingIntreval = null;
        this.movingDirectIntreval = null;
        this.navAutohideInterval = null;
        this.speedX = this.speedY = null;
        this.targetX = this.targetY = null;
        this.allow = {
            allowDown: false,
            allowUp: false,
            allowLeft: false,
            allowRight: false,
            allowZoom: false,
            allowUnzoom: false
        };
        this.isScaled = false;
        this.sm = new k();
        this.map = null;
        this.markersContainer = l;
        this.markers = null;
        this.createHolders();
        this.contentLoader = new g(this.sett.contentUrl, this.$contentHol, function (o) {
            return function (p) {
                o.imgContentStart(p)
            }
        }(this));
        this.contentLoader.loadStart()
    };
    d.prototype.createHolders = function () {
        this.$mivHol = f("<div />").addClass("lhp_miv_holder").css({
            position: "relative",
            overflow: "hidden",
            width: this.sett.viewportWidth,
            height: this.sett.viewportHeight
        });
        this.$preloadHol = f("<div />").addClass("lhp_miv_preload_holder");
        this.$contentHol = f("<div />").addClass("lhp_miv_content_holder").css({
            position: "absolute"
        });
        this.$blackScreen = f("<div />").addClass("lhp_miv_blackScreen").css({
            position: "absolute",
            "z-index": "9999",
            width: "100%",
            height: "100%",
            background: "#ffffff"
        });
        this.$mivHol.append(this.$preloadHol);
        this.$mivHol.append(this.$blackScreen);
        this.$mivHol.append(this.$contentHol);
        this.$mainHolder.append(this.$mivHol);
        if (this.sett.testMode) {
            this.$infoBox = f("<div />").addClass("lhp_miv_infoBox_holder");
            this.$mivHol.append(this.$infoBox)
        }
    };
    d.prototype.iniNav = function () {
        var m = f("<ul />").addClass("ui-widget ui-helper-clearfix"),
            s = this.$mainHolder,
            n = this.$navHol,
            p, q, o = this,
            t = 27,
            l = 0,
            r = [
                ["moveDown", "moveStop", "ui-icon-carat-1-n", "intNavMoveDownBtt"],
                ["moveUp", "moveStop", "ui-icon-carat-1-s", "intNavMoveUpBtt"],
                ["moveRight", "moveStop", "ui-icon-carat-1-w", "intNavMoveRightBtt"],
                ["moveLeft", "moveStop", "ui-icon-carat-1-e", "intNavMoveLeftBtt"],
                ["zoom", "zoomStop", "ui-icon-zoomin", "intNavZoomBtt"],
                ["unzoom", "zoomStop", "ui-icon-zoomout", "intNavUnzoomBtt"],
                ["fitToViewport", null, "ui-icon-stop", "intNavFitToViewportBtt"],
                ["fullSize", null, "ui-icon-arrow-4-diag", "intNavFullSizeBtt"]
            ];
        f.each(r, function (v) {
            var w = r[v][0],
                x = r[v][1],
                z = r[v][3],
                y, u;
            if (o.sett[z]) {
                l += t;
                y = f("<li />").addClass("ui-state-default ui-corner-all " + w), u = f("<span />").addClass("ui-icon " + r[v][2]);
                y.append(u);
                m.append(y);
                y.bind("mouseenter.lhpMIV touchstart.lhpMIV", function () {
                    if (!f(this).hasClass("lhp_miv_nav_btt_disab")) {
                        f(this).addClass("ui-state-hover")
                    }
                });
                y.bind("mouseleave.lhpMIV touchend.lhpMIV", function () {
                    f(this).removeClass("ui-state-hover")
                });
                y.bind(((o.isTouchDev) ? "touchstart.lhpMIV" : "mousedown.lhpMIV"), function (A) {
                    return function (B) {
                        if (!f(this).hasClass("lhp_miv_nav_btt_disab")) {
                            s.lhpMegaImgViewer(A)
                        }
                        B.preventDefault()
                    }
                }(w));
                if (x) {
                    y.bind(((o.isTouchDev) ? "touchend.lhpMIV" : "mouseup.lhpMIV"), function (A) {
                        return function (B) {
                            if (!f(this).hasClass("lhp_miv_nav_btt_disab")) {
                                s.lhpMegaImgViewer(A)
                            }
                            B.preventDefault()
                        }
                    }(x))
                }
            }
        });
        if (this.$navHol.hasClass("lhp_miv_nav_pos_L") || this.$navHol.hasClass("lhp_miv_nav_pos_R")) {
            this.$navHol.css("width", t);
            this.$navHol.css("margin-top", - l / 2)
        }
        if (this.$navHol.hasClass("lhp_miv_nav_pos_T") || this.$navHol.hasClass("lhp_miv_nav_pos_B")) {
            this.$navHol.css("margin-left", - l / 2)
        }
        s.bind("mivChange.lhpMIV", function (w) {
            var v = "lhp_miv_nav_btt_disab",
                u = "ui-state-hover";
            if (w.allowDown) {
                n.find(".moveDown").removeClass(v)
            } else {
                n.find(".moveDown").removeClass(u).addClass(v)
            }
            if (w.allowUp) {
                n.find(".moveUp").removeClass(v)
            } else {
                n.find(".moveUp").removeClass(u).addClass(v)
            }
            if (w.allowLeft) {
                n.find(".moveLeft").removeClass(v)
            } else {
                n.find(".moveLeft").removeClass(u).addClass(v)
            }
            if (w.allowRight) {
                n.find(".moveRight").removeClass(v)
            } else {
                n.find(".moveRight").removeClass(u).addClass(v)
            }
            if (w.allowZoom) {
                n.find(".zoom").removeClass(v);
                n.find(".fullSize").removeClass(v)
            } else {
                n.find(".zoom").removeClass(u).addClass(v);
                n.find(".fullSize").removeClass(u).addClass(v)
            }
            if (w.allowUnzoom) {
                n.find(".unzoom").removeClass(v);
                n.find(".fitToViewport").removeClass(v)
            } else {
                n.find(".unzoom").removeClass(u).addClass(v);
                n.find(".fitToViewport").removeClass(u).addClass(v)
            }
        });
        if (this.sett.intNavAutoHide) {
            n.css("display", "none");
            s.bind("mouseenter.lhpMIV touchstart.lhpMIV", function () {
                clearInterval(o.navAutohideInterval);
                n.fadeIn("fast")
            });
            s.bind("mouseleave.lhpMIV touchend.lhpMIV", function () {
                clearInterval(o.navAutohideInterval);
                o.navAutohideInterval = setInterval(function (u) {
                    return function () {
                        u.stop().clearQueue().fadeOut("fast")
                    }
                }(n), 1000)
            })
        }
        n.append(m)
    };
    d.prototype.imgContentStart = function (l) {
        this.$content = l;
        l.addClass("lhp_miv_content").css({
            "float": "left"
        });
        this.contentFullSize = {
            w: l.width(),
            h: l.height()
        };
        this.sett.mainImgWidth = this.contentFullSize.w;
        this.sett.mainImgHeight = this.contentFullSize.h;
        this.start();
        this.$preloadHol.remove();
        this.$blackScreen.animate({
            opacity: 0
        }, {
            duration: 500,
            complete: function () {
                f(this).remove()
            }
        })
    };
    d.prototype.start = function () {
        if (this.sett.mapEnable && this.sett.mapThumb) {
            this.map = new b(this.sett, this.$mainHolder, this.$content, this.isTouchDev);
            this.map.ini(this.$mivHol)
        }
        if (this.sett.intNavEnable) {
            this.$navHol = f('<div class="lhp_miv_nav"/>').addClass("lhp_miv_nav_pos_" + this.sett.intNavPos);
            this.iniNav();
            this.$mivHol.prepend(this.$navHol)
        }
        this.markers = new a(this.$mainHolder, this.$contentHol, this.markersContainer, this.isTouchDev, this.sett.popupShowAction, this.sett.startScale);
        this.markers.ini();
        this.$contentHol.bind("mouseenter.lhpMIV", {
            _this: this
        }, this.mouseenterHandler);
        if (this.isTouchDev) {
            this.$contentHol.bind("touchstart.lhpMIV", {
                _this: this
            }, this.mousedownHandler)
        } else {
            this.$contentHol.bind("mousedown.lhpMIV", {
                _this: this
            }, this.mousedownHandler);
            this.$contentHol.bind("mouseup.lhpMIV", {
                _this: this
            }, this.mouseupHandler);
            this.$contentHol.bind("mouseleave.lhpMIV", {
                _this: this
            }, this.mouseupHandler)
        }
        this.$contentHol.bind("mousewheel.lhpMIV", {
            _this: this
        }, this.mousewheelHandler);
        if (this.sett.testMode) {
            this.$contentHol.bind("mousemove.lhpMIV", {
                _this: this
            }, this.showCurrentCoor)
        }
        this.setProperties(this.sett.startX, this.sett.startY, this.sett.startScale, true)
    };
    d.prototype.destroy = function () {
        this.contentLoader.dispose();
        this.contentLoader = null;
        this.animStop();
        this.stopMoving();
        this.stopDirectMoving();
        if (this.markers) {
            this.markers.destroy()
        }
        if (this.$navHol) {
            this.$navHol.find("li").each(function (l) {
                f(this).unbind()
            })
        }
        if (this.map) {
            this.map.destroy()
        }
        this.$mainHolder.unbind(".lhpMIV");
        this.$contentHol.unbind();
        this.$mivHol.remove();
        f.each(this, function (m, l) {
            if (!f.isFunction(l)) {
                m = null
            }
        })
    };
    d.prototype.mousePageCoor = function (m) {
        var l = {
            x: m.pageX,
            y: m.pageY
        };
        m = m.originalEvent;
        if (this.isTouchDev && m) {
            l.x = m.changedTouches[0].pageX;
            l.y = m.changedTouches[0].pageY
        }
        return l
    };
    d.prototype.mouseenterHandler = function (l) {
        if (!l.data._this.sett.testMode) {
            l.data._this.$contentHol.css("cursor", "url(css/lhp_miv/cursorHand.png),default")
        }
    };
    d.prototype.mousedownHandler = function (l) {
        var m = l.data._this;
        m.animStop(true);
        m.stopMoving();
        m.stopDirectMoving();
        if (m.isTouchDev) {
            m.$contentHol.unbind("touchmove.lhpMIV", m.mousemoveHandler).bind("touchmove.lhpMIV", {
                _this: m
            }, m.mousemoveHandler);
            m.$contentHol.unbind({
                "touchend.lhpMIV": m.positioning
            }).bind("touchend.lhpMIV", {
                _this: m
            }, m.positioning)
        } else {
            m.$contentHol.unbind("mousemove.lhpMIV", m.mousemoveHandler).bind("mousemove.lhpMIV", {
                _this: m
            }, m.mousemoveHandler);
            m.$contentHol.unbind({
                "mouseup.lhpMIV": m.positioning
            }).bind("mouseup.lhpMIV", {
                _this: m
            }, m.positioning)
        }
        m.lastMousePageCoor = m.mousePageCoor(l);
        m.$contentHol.css("cursor", "url(css/lhp_miv/cursorDrag.png), move");
        l.preventDefault()
    };
    d.prototype.mousemoveHandler = function (l) {
        var m = l.data._this;
        if (m.isTouchDev) {
            m.$contentHol.unbind({
                "touchend.lhpMIV": m.positioning
            });
            m.$contentHol.unbind({
                "touchend.lhpMIV": m.stopDraggingHandler
            }).bind("touchend.lhpMIV", {
                _this: m
            }, m.stopDraggingHandler)
        } else {
            m.$contentHol.unbind("mouseup.lhpMIV", m.positioning);
            m.$contentHol.unbind({
                "mouseup.lhpMIV": m.stopDraggingHandler
            }).bind("mouseup.lhpMIV", {
                _this: m
            }, m.stopDraggingHandler);
            m.$contentHol.unbind({
                "mouseleave.lhpMIV": m.stopDraggingHandler
            }).bind("mouseleave.lhpMIV", {
                _this: m
            }, m.stopDraggingHandler)
        }
        m.dragging(l, "hard");
        l.preventDefault()
    };
    d.prototype.mouseupHandler = function (l) {
        var m = l.data._this;
        m.$contentHol.unbind("mousemove.lhpMIV", m.mousemoveHandler);
        m.$contentHol.unbind("mouseup.lhpMIV", m.positioning);
        if (!m.sett.testMode) {
            m.$contentHol.css("cursor", "url(css/lhp_miv/cursorHand.png),default")
        } else {
            m.$contentHol.css("cursor", "default")
        }
    };
    d.prototype.stopDraggingHandler = function (l) {
        var m = l.data._this;
        m.$contentHol.unbind({
            "mouseup.lhpMIV": m.stopDraggingHandler
        });
        m.$contentHol.unbind({
            "mouseleave.lhpMIV": m.stopDraggingHandler
        });
        m.dragging(l, "inertia")
    };
    d.prototype.mousewheelHandler = function (m, p) {
        var o = m.data._this,
            n = (p > 0) ? o.sm.nextScale() : o.sm.prevScale(),
            l = o.calculateScale(m, n);
        o.animStop();
        o.stopMoving();
        o.stopDirectMoving();
        o.animSizeAndPos(l.x, l.y, l.w, l.h);
        m.preventDefault();
        return false
    };
    d.prototype.showCurrentCoor = function (o) {
        var q = o.data._this,
            l = q.mousePageCoor(o),
            p = q.$contentHol.position(),
            m = q.$mivHol.offset(),
            n = q.$content.width() / q.contentFullSize.w;
        l.x = Math.round((l.x - p.left - m.left) / n);
        l.y = Math.round((l.y - p.top - m.top) / n);
        q.$infoBox.css("display", "block");
        q.$infoBox.html("x:" + l.x + " y:" + l.y)
    };
    d.prototype.adaptsToContainer = function () {
        if (this.$content) {
            var l = this.$content.width() / this.contentFullSize.w;
            l = (l > 1) ? 1 : l;
            this.animStop();
            this.stopMoving();
            this.stopDirectMoving();
            this.setProperties(null, null, l, true)
        }
    };
    d.prototype.beginZooming = function (n) {
        if (this.$content) {
            var r = (n == "Z") ? 1 : -1,
                o = {
                    _this: this
                }, q = {
                    x: (this.$mivHol.width() / 2),
                    y: (this.$mivHol.height() / 2)
                }, m = this.$mivHol.offset(),
                l = {
                    x: (q.x + m.left),
                    y: (q.y + m.top)
                }, p = {
                    data: o,
                    pageX: l.x,
                    pageY: l.y
                };
            this.animStop(true);
            this.stopMoving();
            this.stopDirectMoving();
            if (!this.movingIntreval) {
                this.movingIntreval = setInterval(function (u, s, t) {
                    return function () {
                        u.zooming(s, t)
                    }
                }(this, p, r), this.sett.animTime / 5)
            }
            this.zooming(p, r)
        }
    };
    d.prototype.zooming = function (m, o) {
        var n = (o > 0) ? this.sm.nextScale() : this.sm.prevScale(),
            l = this.calculateScale(m, n);
        this.animStop();
        this.animSizeAndPos(l.x, l.y, l.w, l.h);
        if (this.sett.fitToViewportShortSide) {
            if (n >= 1 || l.w <= this.$mivHol.width() || l.h <= this.$mivHol.height()) {
                this.stopZooming()
            }
        } else {
            if (n >= 1 || (l.w <= this.$mivHol.width() && l.h <= this.$mivHol.height())) {
                this.stopZooming()
            }
        }
    };
    d.prototype.stopZooming = function () {
        this.stopMoving()
    };
    d.prototype.beginDirectMove = function (l) {
        if (this.$content) {
            this.animStop(true);
            this.stopMoving();
            this.sm.setScale(this.$content.width() / this.contentFullSize.w);
            this.speedX = this.speedY = 0;
            switch (l) {
            case "U":
                this.speedY = -50000 / this.sett.animTime;
                break;
            case "D":
                this.speedY = 50000 / this.sett.animTime;
                break;
            case "L":
                this.speedX = -50000 / this.sett.animTime;
                break;
            case "R":
                this.speedX = 50000 / this.sett.animTime;
                break
            }
            if (!this.movingDirectIntreval && (this.speedX || this.speedY)) {
                this.movingDirectIntreval = setInterval(function (m) {
                    return function () {
                        m.directMoveWithInertia()
                    }
                }(this), 10)
            }
        }
    };
    d.prototype.directMoveWithInertia = function () {
        var m = this.$contentHol.position().left,
            l = this.$contentHol.position().top,
            p = Math.ceil(m + this.speedX),
            n = Math.ceil(l + this.speedY),
            o;
        if (!this.movingIntreval) {
            this.movingIntreval = setInterval(function (q) {
                return function () {
                    q.moveWithInertia()
                }
            }(this), 10)
        }
        o = this.getSafeTarget(p, n, this.speedX, this.speedY);
        this.targetX = Math.round(o.x);
        this.targetY = Math.round(o.y)
    };
    d.prototype.stopDirectMoving = function () {
        clearInterval(this.movingDirectIntreval);
        this.movingDirectIntreval = null
    };
    d.prototype.dragging = function (q, o) {
        var p = this.sett.draggInertia,
            m = this.mousePageCoor(q),
            n = m.x - this.lastMousePageCoor.x,
            l = m.y - this.lastMousePageCoor.y;
        if (o == "inertia" && this.lastDragg) {
            this.draggingWithInertia(this.lastDragg.x * p, this.lastDragg.y * p)
        } else {
            this.draggingHard(n, l)
        }
        this.lastDragg = {
            x: (Math.abs(n) < 5) ? 0 : n,
            y: (Math.abs(l) < 5) ? 0 : l
        };
        this.lastMousePageCoor = m
    };
    d.prototype.draggingHard = function (m, l) {
        var q = this.$contentHol.position(),
            p = q.left + m,
            n = q.top + l,
            o = this.getSafeTarget(p, n, m, l);
        this.animStop();
        this.$contentHol.css({
            left: o.x,
            top: o.y
        })
    };
    d.prototype.draggingWithInertia = function (m, l) {
        var p = this.targetX + m,
            n = this.targetY + l,
            o;
        if (!this.movingIntreval) {
            this.movingIntreval = setInterval(function (q) {
                return function () {
                    q.moveWithInertia()
                }
            }(this), 10);
            p = this.$contentHol.position().left + m;
            n = this.$contentHol.position().top + l
        }
        o = this.getSafeTarget(p, n, m, l);
        this.targetX = Math.round(o.x);
        this.targetY = Math.round(o.y)
    };
    d.prototype.getSafeTarget = function (y, w, o, m) {
        var l = this.getLimit(this.sm.getScale()),
            p = l.xMin,
            s = l.xMax,
            z = l.yMin,
            r = l.yMax,
            n = this.$mivHol.width(),
            x = this.$mivHol.height(),
            v = n / 2,
            u = x / 2,
            t = this.contentFullSize.w * this.sm.getScale(),
            q = this.contentFullSize.h * this.sm.getScale();
        if ((m < 0) && (w < z)) {
            w = z
        } else {
            if ((m > 0) && (w > r)) {
                w = r
            }
        }
        if (q < x) {
            w = u - q / 2
        }
        if ((o < 0) && (y < p)) {
            y = p
        } else {
            if ((o > 0) && (y > s)) {
                y = s
            }
        }
        if (t < n) {
            y = v - t / 2
        }
        return {
            x: y,
            y: w
        }
    };
    d.prototype.moveWithInertia = function () {
        var o = this.$contentHol.position(),
            n = this.sett.dragSmooth,
            m, l;
        o.left = Math.ceil(o.left);
        o.top = Math.ceil(o.top);
        m = (this.targetX - o.left) / n;
        l = (this.targetY - o.top) / n;
        if (Math.abs(m) < 1) {
            m = (m > 0) ? 1 : -1
        }
        if (Math.abs(l) < 1) {
            l = (l > 0) ? 1 : -1
        }
        if (o.left == this.targetX) {
            m = 0
        }
        if (o.top == this.targetY) {
            l = 0
        }
        this.$contentHol.css({
            left: o.left + m,
            top: o.top + l
        });
        this.dispatchEventChange();
        if (o.left == this.targetX && o.top == this.targetY) {
            this.stopDirectMoving();
            this.stopMoving()
        }
    };
    d.prototype.stopMoving = function () {
        clearInterval(this.movingIntreval);
        this.movingIntreval = null
    };
    d.prototype.positioning = function (m) {
        var n = m.data._this,
            l = n.calculatePosInCenter(m);
        n.animStop();
        n.stopMoving();
        n.stopDirectMoving();
        n.animSizeAndPos(l.x, l.y)
    };
    d.prototype.setProperties = function (A, u, o, z) {
        if (this.$content) {
            var p = {
                _this: this
            }, q = {
                x: (this.$mivHol.width() / 2),
                y: (this.$mivHol.height() / 2)
            }, B = this.$mivHol.offset(),
                n = {
                    x: (q.x + B.left),
                    y: (q.y + B.top)
                }, r = {
                    data: p,
                    pageX: n.x,
                    pageY: n.y
                }, w = this.$contentHol.position(),
                s, v, l = w.left,
                C = w.top,
                m = this.$content.width(),
                t = this.$content.height();
            A = parseFloat(A);
            u = parseFloat(u);
            o = parseFloat(o);
            if (!isNaN(o)) {
                if (o > 1) {
                    o = 1
                }
                s = this.calculateScale(r, o);
                l = s.x;
                C = s.y;
                m = s.w;
                t = s.h
            }
            v = m / this.contentFullSize.w;
            if (!isNaN(A)) {
                l = -(A * v) + q.x
            }
            if (!isNaN(u)) {
                C = -(u * v) + q.y
            }
            this.animStop();
            this.stopMoving();
            this.stopDirectMoving();
            this.animSizeAndPos(l, C, m, t, z)
        }
    };
    d.prototype.calculatePosInCenter = function (n) {
        var o = this.$contentHol.position(),
            r = this.$mivHol.offset(),
            m = {
                x: (this.$mivHol.width() / 2),
                y: (this.$mivHol.height() / 2)
            }, s = this.mousePageCoor(n),
            t = {
                x: (s.x - r.left),
                y: (s.y - r.top)
            }, q, p, l, u;
        q = m.x - t.x;
        p = m.y - t.y;
        l = o.left + q;
        u = o.top + p;
        return {
            x: l,
            y: u,
            shftX: q,
            shftY: p
        }
    };
    d.prototype.calculateScale = function (q, p) {
        var t = this.$mivHol.offset(),
            o = this.$content.offset(),
            u = this.mousePageCoor(q),
            s, n, l, v, m, r;
        p = this.getSafeScale(p);
        this.sm.setScale(p);
        s = this.$content.width() / this.contentFullSize.w;
        n = {
            x: (u.x - o.left) / s,
            y: (u.y - o.top) / s
        };
        m = Math.round(this.contentFullSize.w * p);
        r = Math.round(this.contentFullSize.h * p);
        l = Math.round(o.left - t.left + n.x * (s - p));
        v = Math.round(o.top - t.top + n.y * (s - p));
        return {
            x: l,
            y: v,
            w: m,
            h: r
        }
    };
    d.prototype.getSafeScale = function (s) {
        var u = (s <= 0) ? 0.00001 : s,
            l = this.$mivHol.width(),
            v = this.$mivHol.height(),
            t = this.contentFullSize.w,
            p = this.contentFullSize.h,
            r = t * u,
            n = p * u,
            q = l / t,
            w = v / p,
            m = l / v,
            o = r / n;
        if (this.sett.fitToViewportShortSide) {
            if (r < l || n < v) {
                q = l / this.contentFullSize.w;
                w = v / this.contentFullSize.h;
                u = Math.max(q, w);
                if (!this.sett.contentSizeOver100 && (t <= l || p <= v)) {
                    u = 1
                }
            }
        } else {
            if (r < l && n < v) {
                if (o <= m) {
                    u = w
                } else {
                    u = q
                }
            }
            if (!this.sett.contentSizeOver100 && t <= l && p <= v) {
                u = 1
            }
        }
        return u
    };
    d.prototype.getLimit = function (n) {
        var m = -(Math.round(this.contentFullSize.w * n) - this.$mivHol.width()),
            l = -(Math.round(this.contentFullSize.h * n) - this.$mivHol.height());
        return {
            xMin: m,
            xMax: 0,
            yMin: l,
            yMax: 0
        }
    };
    d.prototype.getSafeXY = function (w, u, t) {
        var l = this.getLimit(t),
            m = this.$mivHol.width(),
            v = this.$mivHol.height(),
            s = m / 2,
            q = v / 2,
            r = this.contentFullSize.w,
            o = this.contentFullSize.h,
            p = r * t,
            n = o * t,
            A = w,
            z = u;
        if (p < m) {
            if (w < l.xMin || w > l.xMax) {
                A = s - p / 2
            }
        } else {
            if (w < l.xMin) {
                A = l.xMin
            } else {
                if (w > l.xMax) {
                    A = l.xMax
                }
            }
        }
        if (n < v) {
            if (u < l.yMin || u > l.yMax) {
                z = q - n / 2
            }
        } else {
            if (u < l.yMin) {
                z = l.yMin
            } else {
                if (u > l.yMax) {
                    z = l.yMax
                }
            }
        }
        return {
            x: A,
            y: z
        }
    };
    d.prototype.animSizeAndPos = function (t, q, u, n, s) {
        var p, r, v = function (w) {
            return function () {
                w.dispatchEventChange()
            }
        }(this),
            l = function (w) {
                return function () {
                    w.dispatchEventChange()
                }
            }(this),
            m = function (w) {
                return function () {
                    w.dispatchEventChange()
                }
            }(this),
            o = function (w) {
                return function () {
                    w.isScaled = false;
                    w.dispatchEventChange()
                }
            }(this);
        if (u != undefined) {
            r = u / this.contentFullSize.w
        } else {
            r = this.$content.width() / this.contentFullSize.w
        }
        if (t != undefined && q != undefined) {
            p = this.getSafeXY(t, q, r);
            if (s) {
                this.$contentHol.css({
                    left: p.x,
                    top: p.y
                });
                l()
            } else {
                this.$contentHol.animate({
                    left: p.x,
                    top: p.y
                }, {
                    duration: this.sett.animTime,
                    easing: "easeOutCubic",
                    step: v,
                    complete: l
                })
            }
        }
        if (u != undefined && n != undefined && (u != this.$content.width() || n != this.$content.height())) {
            this.isScaled = true;
            if (s) {
                this.$content.css({
                    width: u,
                    height: n
                });
                m();
                o()
            } else {
                this.$content.animate({
                    width: u,
                    height: n
                }, {
                    duration: this.sett.animTime,
                    easing: "easeOutCubic",
                    step: m,
                    complete: o
                })
            }
        }
    };
    d.prototype.animStop = function (l) {
        if (this.$contentHol && this.$content) {
            this.$contentHol.stop().clearQueue();
            this.$content.stop().clearQueue();
            if (l) {
                this.sm.setScale(this.$content.width() / this.contentFullSize.w)
            }
            this.dispatchEventChange()
        }
    };
    d.prototype.dispatchEventChange = function () {
        var l = this.getCurrentState(),
            m = f.Event("mivChange", l);
        this.allow = l;
        this.$mainHolder.trigger(m)
    };
    d.prototype.getCurrentState = function () {
        var m = {};
        if (this.$content) {
            var r = this.$contentHol.position(),
                n = this.getLimit(this.sm.getScale()),
                l = this.$content.width(),
                o = this.$content.height(),
                q = {
                    x: (this.$mivHol.width() / 2),
                    y: (this.$mivHol.height() / 2)
                }, p = l / this.contentFullSize.w;
            m.allowDown = (Math.ceil(r.top) < Math.ceil(n.yMax));
            m.allowUp = (Math.ceil(r.top) > Math.ceil(n.yMin));
            m.allowRight = (Math.ceil(r.left) < Math.ceil(n.xMax));
            m.allowLeft = (Math.ceil(r.left) > Math.ceil(n.xMin));
            m.allowZoom = (l / this.contentFullSize.w < 1);
            if (this.sett.fitToViewportShortSide) {
                m.allowUnzoom = (l > this.$mivHol.width() && o > this.$mivHol.height())
            } else {
                m.allowUnzoom = (l > this.$mivHol.width() || o > this.$mivHol.height())
            }
            m.wPropViewpContent = this.$mivHol.width() / l;
            m.hPropViewpContent = this.$mivHol.height() / o;
            m.xPosInCenter = Math.round((-r.left + q.x) / p);
            m.yPosInCenter = Math.round((-r.top + q.y) / p);
            m.scale = p;
            m.isScaled = this.isScaled
        }
        return m
    };
    d.prototype.allowCompare = function (n, l) {
        var m = true;
        f.each(n, function (o) {
            if (n[o] != l[o]) {
                m = false;
                return
            }
        });
        return m
    };
    var k = function () {
        this.step = 0.1;
        this.curr = 1
    };
    k.prototype.getScale = function () {
        return this.curr
    };
    k.prototype.setScale = function (l) {
        this.curr = l
    };
    k.prototype.nextScale = function () {
        var l = this.curr + this.step;
        if (l > 1) {
            this.curr = 1
        } else {
            this.curr = l
        }
        return this.getScale()
    };
    k.prototype.prevScale = function () {
        var l = this.curr - this.step;
        if (l < this.step) {
            this.curr = 0
        } else {
            this.curr = l
        }
        return this.getScale()
    };
    var g = function (m, l, n) {
        this.url = m;
        this.$imgHolder = l;
        this.callback = n
    };
    g.prototype.loadStart = function () {
        var l = f("<img/>");
        l.one("load", function (m) {
            return function (n) {
                m.loadComplete(n)
            }
        }(this));
        this.$imgHolder.prepend(l);
        l.attr("src", this.url)
    };
    g.prototype.loadComplete = function (l) {
        if (this.callback) {
            this.callback(f(l.currentTarget))
        }
    };
    g.prototype.dispose = function () {
        this.callback = null
    };
    var b = function (n, o, l, m) {
        this.contentLoader = null;
        this.isTouchDev = m;
        this.sett = n;
        this.$mainHolder = o;
        this.$previewImg = l;
        this.$img = null;
        this.$mapHol = null;
        this.$mapWrappHol = null;
        this.$vr = null;
        this.lastMousePageCoor = {};
        this.contentLoadStartTimeout
    };
    b.prototype.ini = function (l) {
        this.$mapHol = f('<div class="lhp_miv_map"/>');
        this.$mapWrappHol = f('<div class="lhp_miv_map_wrapp_hol"/>');
        this.$mapHol.append(this.$mapWrappHol);
        l.prepend(this.$mapHol);
        this.contentLoader = new g(this.sett.mapThumb, this.$mapWrappHol, function (n) {
            return function (o) {
                n.start(o)
            }
        }(this));
        var m = this;
        this.contentLoadStartTimeout = setTimeout(function () {
            return function () {
                m.contentLoader.loadStart()
            }
        }(), 10)
    };
    b.prototype.start = function (m) {
        var l = m.width(),
            n = m.height(),
            o;
        this.$img = m;
        this.$img.css({
            cursor: "pointer"
        });
        this.$mapHol.addClass("lhp_miv_map_pos_" + this.sett.mapPos).css({
            width: l,
            height: n
        });
        this.$mapWrappHol.addClass("lhp_miv_map_wrapp_hol_" + this.sett.mapPos).css({
            width: l,
            height: n
        });
        switch (this.sett.mapPos) {
        case "T":
        case "B":
            this.$mapHol.css("margin-left", - l / 2);
            break;
        case "L":
        case "R":
            this.$mapHol.css("margin-top", - n / 2);
            break
        }
        this.$mapWrappHol.append(this.$img);
        this.$vr = f('<div class="lhp_miv_map_vr"/>').css({
            position: "absolute",
            "z-index": 2
        }).appendTo(this.$mapWrappHol);
        this.vrAddInteractions();
        this.$mainHolder.bind("mivChange.lhpMIV", {
            _this: this
        }, this.mivChangeHandler);
        o = this.$mainHolder.lhpMegaImgViewer("getCurrentState");
        o.data = {};
        o.data._this = this;
        this.mivChangeHandler(o)
    };
    b.prototype.destroy = function () {
        clearTimeout(this.contentLoadStartTimeout);
        this.$vr.unbind(".lhpMIV");
        this.$mapHol.unbind(".lhpMIV");
        this.$img.unbind(".lhpMIV");
        this.contentLoader.dispose();
        this.contentLoader = null
    };
    b.prototype.vrAddInteractions = function () {
        this.$vr.bind("mouseenter.lhpMIV", {
            _this: this
        }, this.mouseenterHandler);
        if (this.isTouchDev) {
            this.$vr.bind("touchstart.lhpMIV", {
                _this: this
            }, this.mousedownHandler);
            this.$vr.bind("touchend.lhpMIV", {
                _this: this
            }, this.mouseupHandler);
            this.$img.bind("touchstart.lhpMIV", {
                _this: this
            }, this.mouseclickHandler)
        } else {
            this.$vr.bind("mousedown.lhpMIV", {
                _this: this
            }, this.mousedownHandler);
            this.$mapHol.bind("mouseup.lhpMIV", {
                _this: this
            }, this.mouseupHandler);
            this.$mapHol.bind("mouseleave.lhpMIV", {
                _this: this
            }, this.mouseupHandler);
            this.$img.bind("click.lhpMIV", {
                _this: this
            }, this.mouseclickHandler)
        }
    };
    b.prototype.mouseenterHandler = function (l) {
        l.data._this.$vr.css("cursor", "url(css/lhp_miv/cursorHand.png),default")
    };
    b.prototype.mousedownHandler = function (l) {
        var m = l.data._this;
        m.$mainHolder.unbind("mivChange.lhpMIV", m.mivChangeHandler);
        if (m.isTouchDev) {
            m.$mapHol.unbind("touchmove.lhpMIV", m.mousemoveHandler).bind("touchmove.lhpMIV", {
                _this: m
            }, m.mousemoveHandler)
        } else {
            m.$mapHol.unbind("mousemove.lhpMIV", m.mousemoveHandler).bind("mousemove.lhpMIV", {
                _this: m
            }, m.mousemoveHandler)
        }
        m.$vr.unbind("mouseenter.lhpMIV", m.mouseenterHandler);
        m.lastMousePageCoor = m.mousePageCoor(l);
        m.$vr.css("cursor", "url(css/lhp_miv/cursorDrag.png), move").addClass("lhp_miv_map_vr_over");
        l.preventDefault()
    };
    b.prototype.mousemoveHandler = function (l) {
        var m = l.data._this;
        if (m.isTouchDev) {
            m.$mapHol.unbind({
                "touchend.lhpMIV": m.stopDraggingHandler
            }).bind("touchend.lhpMIV", {
                _this: m
            }, m.stopDraggingHandler)
        } else {
            m.$mapHol.unbind({
                "mouseup.lhpMIV": m.stopDraggingHandler
            }).bind("mouseup.lhpMIV", {
                _this: m
            }, m.stopDraggingHandler);
            m.$mapHol.unbind({
                "mouseleave.lhpMIV": m.stopDraggingHandler
            }).bind("mouseleave.lhpMIV", {
                _this: m
            }, m.stopDraggingHandler)
        }
        m.dragging(l);
        l.preventDefault()
    };
    b.prototype.mouseupHandler = function (l) {
        var m = l.data._this;
        m.$mapHol.unbind("touchmove.lhpMIV", m.mousemoveHandler);
        m.$mapHol.unbind("mousemove.lhpMIV", m.mousemoveHandler);
        m.$vr.unbind("mouseenter.lhpMIV", m.mouseenterHandler).bind("mouseenter.lhpMIV", {
            _this: m
        }, m.mouseenterHandler);
        m.$mainHolder.unbind("mivChange.lhpMIV", m.mivChangeHandler).bind("mivChange.lhpMIV", {
            _this: m
        }, m.mivChangeHandler);
        m.$vr.css("cursor", "url(css/lhp_miv/cursorHand.png),default").removeClass("lhp_miv_map_vr_over")
    };
    b.prototype.mouseclickHandler = function (o) {
        var q = o.data._this,
            m = q.mousePageCoor(o),
            n = q.$mapHol.offset(),
            l = (m.x - n.left) * q.sett.mainImgWidth / q.$mapWrappHol.width(),
            p = (m.y - n.top) * q.sett.mainImgHeight / q.$mapWrappHol.height();
        q.$mainHolder.lhpMegaImgViewer("setPosition", l, p)
    };
    b.prototype.dragging = function (r) {
        var m = this.mousePageCoor(r),
            n = m.x - this.lastMousePageCoor.x,
            l = m.y - this.lastMousePageCoor.y,
            s = this.$vr.position(),
            q = s.left + n,
            o = s.top + l,
            p = this.getSafeTarget(q, o, n, l);
        this.$vr.css({
            left: p.x,
            top: p.y
        });
        this.lastMousePageCoor = m;
        this.mainHolderSetPosition(p.x, p.y)
    };
    b.prototype.stopDraggingHandler = function (l) {
        var m = l.data._this;
        m.$mapHol.unbind({
            "touchend.lhpMIV": m.stopDraggingHandler
        });
        m.$mapHol.unbind({
            "mouseup.lhpMIV": m.stopDraggingHandler
        });
        m.$mapHol.unbind({
            "mouseleave.lhpMIV": m.stopDraggingHandler
        })
    };
    b.prototype.mousePageCoor = function (m) {
        var l = {
            x: m.pageX,
            y: m.pageY
        };
        m = m.originalEvent;
        if (this.isTouchDev && m) {
            l.x = m.changedTouches[0].pageX;
            l.y = m.changedTouches[0].pageY
        }
        return l
    };
    b.prototype.getSafeTarget = function (q, p, n, m) {
        var o = 0,
            l = 0,
            s = this.$mapWrappHol.width() - this.$vr.width(),
            r = this.$mapWrappHol.height() - this.$vr.height();
        if ((m < 0) && (p < l)) {
            p = l
        } else {
            if ((m > 0) && (p > r)) {
                p = r
            }
        }
        if ((n < 0) && (q < o)) {
            q = o
        } else {
            if ((n > 0) && (q > s)) {
                q = s
            }
        }
        return {
            x: q,
            y: p
        }
    };
    b.prototype.mainHolderSetPosition = function (n, m) {
        var l = (n + this.$vr.width() / 2) * this.sett.mainImgWidth / this.$mapWrappHol.width(),
            o = (m + this.$vr.height() / 2) * this.sett.mainImgHeight / this.$mapWrappHol.height();
        this.$mainHolder.lhpMegaImgViewer("setPosition", l, o, null, true)
    };
    b.prototype.mivChangeHandler = function (q) {
        var s = q.data._this,
            m = s.$mapWrappHol.width(),
            p = s.$mapWrappHol.height(),
            r = Math.round(m * ((q.wPropViewpContent > 1) ? 1 : q.wPropViewpContent)),
            l = Math.round(p * ((q.hPropViewpContent > 1) ? 1 : q.hPropViewpContent)),
            o = Math.round((m / s.sett.mainImgWidth) * q.xPosInCenter - (r / 2)),
            n = Math.round((p / s.sett.mainImgHeight) * q.yPosInCenter - (l / 2));
        s.$vr.css({
            width: r,
            height: l,
            left: o,
            top: n
        })
    };
    var a = function (q, p, l, n, o, m) {
        this.$mainHolder = q;
        this.$contentHol = p;
        this.containerId = l;
        this.mClass = "lhp_miv_hotspot";
        this.mInnClass = "lhp_miv_marker";
        this.pClass = "lhp_miv_popup";
        this.isTouchDev = n;
        this.markers = [];
        this.popups = [];
        this.currShowPopup = null;
        this.popupShowAction = o;
        this.startScale = m
    };
    a.prototype.ini = function () {
        var m = this,
            l;
        f("#" + this.containerId).find("." + this.mClass).each(function () {
            m.addMarker(f(this).clone(true, true))
        });
        this.$mainHolder.bind("mivChange.lhpMIV", {
            _this: this
        }, this.mivChangeHandler);
        if (this.startScale == 1) {
            this.positionsMarkers(1)
        }
    };
    a.prototype.destroy = function () {
        var l;
        for (l in this.markers) {
            this.markers[l].destroy()
        }
        for (l in this.popups) {
            this.popups[l].destroy()
        }
        this.$mainHolder = null;
        this.$contentHol = null;
        this.markers = null;
        this.popups = null
    };
    a.prototype.addMarker = function (t) {
        var o = 0,
            u = 0,
            s = 0,
            r = 0,
            l, q, m, n;
        if (t.attr("data-id")) {
            o = t.attr("data-id")
        }
        if (t.attr("data-x")) {
            u = parseInt(t.attr("data-x"))
        }
        if (t.attr("data-y")) {
            s = parseInt(t.attr("data-y"))
        }
        if (t.attr("data-visible-scale")) {
            r = parseFloat(t.attr("data-visible-scale"))
        }
        if (t.attr("data-url")) {
            l = t.attr("data-url")
        }
        n = t.find("." + this.pClass).remove()[0];
        this.$contentHol.append(t);
        q = new i(this, o, u, s, r, l, t);
        this.markers.push(q);
        if (n) {
            this.$contentHol.append(n);
            m = new j(o, f(n), q);
            m.ini();
            this.popups.push(m);
            q.popup = m
        }
        q.ini()
    };
    a.prototype.mivChangeHandler = function (l) {
        var m = l.data._this;
        if (l.isScaled) {
            m.positionsMarkers(l.scale);
            m.positionsPopup()
        } else {
            m.positionsPopup()
        }
    };
    a.prototype.positionsMarkers = function (n) {
        var m, l;
        for (m in this.markers) {
            l = this.markers[m];
            l.positions(n);
            l.visibility(n)
        }
    };
    a.prototype.positionsPopup = function () {
        if (this.currShowPopup) {
            this.currShowPopup.positions()
        }
    };
    a.prototype.getLimit = function () {
        var p = this.$contentHol.position(),
            m = -p.left,
            o = m + this.$mainHolder.width(),
            l = -p.top,
            n = l + this.$mainHolder.height();
        return {
            xMin: m,
            xMax: o,
            yMin: l,
            yMax: n
        }
    };
    a.prototype.showPopup = function (l) {
        if (!this.currShowPopup) {
            this.currShowPopup = l;
            this.currShowPopup.show();
            this.currShowPopup.positions();
            return
        }
        if (this.currShowPopup && this.currShowPopup != l) {
            this.hidePopup(this.currShowPopup);
            this.currShowPopup = l;
            this.currShowPopup.show();
            this.currShowPopup.positions()
        }
    };
    a.prototype.hidePopup = function (l) {
        if (this.currShowPopup && this.currShowPopup == l) {
            this.currShowPopup.hide();
            this.currShowPopup = null
        }
    };
    var i = function (p, r, l, q, o, m, n) {
        this.markers = p;
        this.id = r;
        this.x = l;
        this.y = q;
        this.visibleScale = o;
        this.url = m;
        this.$m = n;
        this.visible = false;
        this.popup = null;
        this.popupClose = null
    };
    i.prototype.ini = function () {
        this.style();
        this.positions(1);
        if (this.url) {
            this.addInteractivityUrl()
        }
        if (this.popup) {
            this.popupClose = this.popup.addClose();
            this.addPopupAction()
        } else {
            if (this.markers.popupShowAction == "rollover") {
                this.addPopupActionNull()
            }
        }
    };
    i.prototype.destroy = function () {
        this.getInn().unbind(".lhpMIV");
        if (this.popup) {
            this.popupClose.unbind(".lhpMIV");
            this.popupClose = null;
            this.popup = null
        }
        this.$m = null;
        this.markers = null
    };
    i.prototype.getInn = function () {
        return this.$m.find("." + this.markers.mInnClass)
    };
    i.prototype.getSize = function () {
        return {
            w: this.getInn().width(),
            h: this.getInn().height()
        }
    };
    i.prototype.getEdges = function () {
        return this.findEdges()
    };
    i.prototype.findEdges = function () {
        var q = this.getInn().offset(),
            n = this.markers.$mainHolder.offset(),
            u = this.markers.$contentHol.position(),
            x = u.left,
            p = u.top,
            v = this.getSize(),
            o = q.left - x - n.left,
            m = o + v.w,
            w = q.top - p - n.top,
            s = w + v.h;
        return ({
            L: o,
            R: m,
            T: w,
            B: s
        })
    };
    i.prototype.getLimit = function () {
        return this.markers.getLimit()
    };
    i.prototype.style = function () {
        var l = {
            position: "absolute",
            "z-index": "2",
            display: "none"
        };
        this.$m.css(l);
        this.$m.css("height", this.$m.height())
    };
    i.prototype.positions = function (m) {
        var l = Math.round(this.x * m),
            n = Math.round(this.y * m);
        this.$m.css({
            left: l,
            top: n
        })
    };
    i.prototype.visibility = function (l) {
        if (l >= this.visibleScale) {
            if (!this.visible) {
                this.$m.stop(true, true).fadeIn(300)
            }
            this.visible = true
        } else {
            if (this.visible) {
                this.$m.fadeOut(300)
            }
            this.visible = false;
            this.markers.hidePopup(this.popup)
        }
    };
    i.prototype.addInteractivityUrl = function () {
        this.getInn().css("cursor", "pointer");
        this.getInn().bind(((this.markers.isTouchDev) ? "touchstart.lhpMIV" : "mousedown.lhpMIV"), {
            _this: this
        }, this.clickHandlerUrl)
    };
    i.prototype.clickHandlerUrl = function (l) {
        var m = l.data._this;
        if (m.url) {
            window.location = m.url
        }
        l.stopPropagation()
    };
    i.prototype.addPopupAction = function () {
        if (this.markers.popupShowAction == "click") {
            this.getInn().bind(((this.markers.isTouchDev) ? "touchstart.lhpMIV" : "mousedown.lhpMIV"), {
                _this: this
            }, this.showPopup);
            this.getInn().css("cursor", "pointer")
        } else {
            this.getInn().bind(((this.markers.isTouchDev) ? "touchstart.lhpMIV" : "mouseenter.lhpMIV"), {
                _this: this
            }, this.showPopup)
        }
        this.popupClose.bind(((this.markers.isTouchDev) ? "touchstart.lhpMIV" : "mousedown.lhpMIV"), {
            _this: this
        }, this.hidePopup)
    };
    i.prototype.addPopupActionNull = function () {
        this.getInn().bind(((this.markers.isTouchDev) ? "touchstart.lhpMIV" : "mouseenter.lhpMIV"), {
            _this: this
        }, this.showPopup)
    };
    i.prototype.showPopup = function (l) {
        var m = l.data._this;
        m.markers.showPopup(m.popup);
        l.stopPropagation()
    };
    i.prototype.hidePopup = function (l) {
        var m = l.data._this;
        m.markers.hidePopup(m.popup);
        l.stopPropagation()
    };
    var j = function (n, m, l) {
        this.id = n;
        this.$p = m;
        this.marker = l;
        this.posHor = this.posHC;
        this.posVer = this.posVT;
        this.$closeHolder = null
    };
    j.prototype.ini = function () {
        if (this.$p.hasClass("pos-TL")) {
            this.posHor = this.posHL;
            this.posVer = this.posVT
        } else {
            if (this.$p.hasClass("pos-T")) {
                this.posHor = this.posHC;
                this.posVer = this.posVT
            } else {
                if (this.$p.hasClass("pos-TR")) {
                    this.posHor = this.posHR;
                    this.posVer = this.posVT
                } else {
                    if (this.$p.hasClass("pos-L")) {
                        this.posHor = this.posHL;
                        this.posVer = this.posVC
                    } else {
                        if (this.$p.hasClass("pos-R")) {
                            this.posHor = this.posHR;
                            this.posVer = this.posVC
                        } else {
                            if (this.$p.hasClass("pos-BL")) {
                                this.posHor = this.posHL;
                                this.posVer = this.posVB
                            } else {
                                if (this.$p.hasClass("pos-B")) {
                                    this.posHor = this.posHC;
                                    this.posVer = this.posVB
                                } else {
                                    if (this.$p.hasClass("pos-BR")) {
                                        this.posHor = this.posHR;
                                        this.posVer = this.posVB
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        this.style();
        this.positions(1)
    };
    j.prototype.destroy = function () {
        this.$p = null;
        this.marker = null
    };
    j.prototype.style = function () {
        var l = {
            display: "none",
            position: "absolute",
            "z-index": "3"
        };
        this.$p.css(l);
        this.$p.css("height", this.$p.height())
    };
    j.prototype.addClose = function () {
        this.$closeHolder = f('<div class="lhp_miv_popup_close"></div>');
        this.$closeHolder.hover(function () {
            f(this).css("opacity", 0.7)
        }, function () {
            f(this).css("opacity", 1)
        });
        this.$p.append(this.$closeHolder);
        return this.$closeHolder
    };
    j.prototype.getSize = function () {
        return {
            w: this.$p.width(),
            h: this.$p.height()
        }
    };
    j.prototype.positions = function () {
        var m = this.marker.getEdges(),
            l = this.posHor(m),
            q = this.posVer(m),
            o = this.marker.getLimit(),
            n = this.$p.width(),
            p = this.$p.height();
        if (l < o.xMin) {
            l = o.xMin
        } else {
            if (l + n > o.xMax) {
                l = o.xMax - n
            }
        }
        if (q < o.yMin) {
            q = o.yMin
        } else {
            if (q + p > o.yMax) {
                q = o.yMax - p
            }
        }
        this.$p.css({
            left: l,
            top: q
        })
    };
    j.prototype.posVT = function (l) {
        return Math.round(l.T) - this.$p.height()
    };
    j.prototype.posVC = function (l) {
        return Math.round(l.T + (l.B - l.T) / 2) - this.$p.height() / 2
    };
    j.prototype.posVB = function (l) {
        return Math.round(l.B)
    };
    j.prototype.posHL = function (l) {
        return Math.round(l.L) - this.$p.width()
    };
    j.prototype.posHC = function (l) {
        return Math.round(l.L + (l.R - l.L) / 2) - this.$p.width() / 2
    };
    j.prototype.posHR = function (l) {
        return Math.round(l.R)
    };
    j.prototype.show = function () {
        this.$p.fadeIn(300)
    };
    j.prototype.hide = function () {
        this.$p.stop().clearQueue().fadeOut(100)
    }
})(jQuery);