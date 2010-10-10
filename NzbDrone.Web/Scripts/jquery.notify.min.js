/*
 * jQuery Notify UI Widget 1.4
 * Copyright (c) 2010 Eric Hynds
 *
 * http://www.erichynds.com/jquery/a-jquery-ui-growl-ubuntu-notification-widget/
 *
 * Depends:
 *   - jQuery 1.4
 *   - jQuery UI 1.8 widget factory
 *
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 *
*/
(function(d){d.widget("ech.notify",{options:{speed:500,expires:5E3,stack:"below",custom:false},_create:function(){var a=this;this.templates={};this.keys=[];this.element.addClass("ui-notify").children().addClass("ui-notify-message ui-notify-message-style").each(function(b){b=this.id||b;a.keys.push(b);a.templates[b]=d(this).removeAttr("id").wrap("<div></div>").parent().html()}).end().empty().show()},create:function(a,b,c){if(typeof a==="object"){c=b;b=a;a=null}a=this.templates[a||this.keys[0]];if(c&&c.custom)a=d(a).removeClass("ui-notify-message-style").wrap("<div></div>").parent().html();return(new d.ech.notify.instance(this))._create(b,d.extend({},this.options,c),a)}});d.extend(d.ech.notify,{instance:function(a){this.parent=a;this.isOpen=false}});d.extend(d.ech.notify.instance.prototype,{_create:function(a,b,c){this.options=b;var e=this;c=c.replace(/#(?:\{|%7B)(.*?)(?:\}|%7D)/g,function(f,g){return g in a?a[g]:""});c=this.element=d(c);var h=c.find(".ui-notify-close");typeof this.options.click==="function"&&c.addClass("ui-notify-click").bind("click",function(f){e._trigger("click",f,e)});h.length&&h.bind("click",function(){e.close();return false});this.open();typeof b.expires==="number"&&window.setTimeout(function(){e.close()},b.expires);return this},close:function(){var a=this,b=this.options.speed;this.isOpen=false;this.element.fadeTo(b,0).slideUp(b,function(){a._trigger("close")});return this},open:function(){if(this.isOpen||this._trigger("beforeopen")===false)return this;var a=this;this.isOpen=true;this.element[this.options.stack==="above"?"prependTo":"appendTo"](this.parent.element).css({display:"none",opacity:""}).fadeIn(this.options.speed,function(){a._trigger("open")});return this},widget:function(){return this.element},_trigger:function(a,b,c){return this.parent._trigger.call(this,a,b,c)}})})(jQuery);
