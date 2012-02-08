/*
 * File:        Scroller.min.js
 * Version:     1.0.1
 * Author:      Allan Jardine (www.sprymedia.co.uk)
 * 
 * Copyright 2011 Allan Jardine, all rights reserved.
 *
 * This source file is free software, under either the GPL v2 license or a
 * BSD (3 point) style license, as supplied with this software.
 * 
 * This source file is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
 * or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.
 */
/*
     GPL v2 or BSD 3 point style
 @contact     www.sprymedia.co.uk/contact

 @copyright Copyright 2011 Allan Jardine, all rights reserved.

 This source file is free software, under either the GPL v2 license or a
 BSD style license, available at:
 http://datatables.net/license_gpl2
 http://datatables.net/license_bsd
*/
(function(d,i,h){var g=function(a,c){!this instanceof g?alert("Scroller warning: Scroller must be initialised with the 'new' keyword."):("undefined"==typeof c&&(c={}),this.s=d.extend({dt:a,tableTop:0,tableBottom:0,redrawTop:0,redrawBottom:0,rowHeight:null,autoHeight:!0,viewportHeight:0,viewportRows:0,stateTO:null,drawTO:null},g.oDefaults,c),this.dom={force:h.createElement("div"),scroller:null,table:null},this.s.dt.oScroller=this,this._fnConstruct())};g.prototype={fnRowToPixels:function(a){return a*
this.s.rowHeight},fnPixelsToRow:function(a){return parseInt(a/this.s.rowHeight,10)},fnScrollToRow:function(a,c){var b=this.fnRowToPixels(a);"undefined"==typeof c||c?d(this.dom.scroller).animate({scrollTop:b}):d(this.dom.scroller).scrollTop(b)},fnMeasure:function(a){this.s.autoHeight&&this._fnCalcRowHeight();this.s.viewportHeight=d(this.dom.scroller).height();this.s.viewportRows=parseInt(this.s.viewportHeight/this.s.rowHeight,10)+1;this.s.dt._iDisplayLength=3*this.s.viewportRows;this.s.trace&&console.log("Row height: "+
this.s.rowHeight+" Viewport height: "+this.s.viewportHeight+" Viewport rows: "+this.s.viewportRows+" Display rows: "+this.s.dt._iDisplayLength);("undefined"==typeof a||a)&&this.s.dt.oInstance.fnDraw()},_fnConstruct:function(){var a=this;this.dom.force.style.position="absolute";this.dom.force.style.top="0px";this.dom.force.style.left="0px";this.dom.force.style.width="1px";this.dom.scroller=d("div.dataTables_scrollBody",this.s.dt.nTableWrapper)[0];this.dom.scroller.appendChild(this.dom.force);this.dom.scroller.style.position=
"relative";this.dom.table=d(">table",this.dom.scroller)[0];this.dom.table.style.position="absolute";this.dom.table.style.top="0px";this.dom.table.style.left="0px";if("auto"!=this.s.rowHeight)this.s.rowHeight=!1;this.fnMeasure();d(this.dom.scroller).scroll(function(){a._fnScroll.call(a)});this.s.dt.aoDrawCallback.push({fn:function(){a._fnDrawCallback.call(a)},sName:"Scroller"});this.s.dt.aoStateSave.push({fn:function(c,b){return b+',"iScroller":'+a.dom.scroller.scrollTop},sName:"Scroller_State"})},
_fnScroll:function(){var a=this,c=this.dom.scroller.scrollTop,b;this.s.trace&&console.log("Scroll: "+c+"px - boundaries: "+this.s.redrawTop+" / "+this.s.redrawBottom+".  Showing rows "+this.fnPixelsToRow(c)+" to "+this.fnPixelsToRow(c+d(this.dom.scroller).height())+" in the viewport, with rows "+this.s.dt._iDisplayStart+" to "+this.s.dt._iDisplayEnd+" rendered by the DataTable");this._fnInfo();clearTimeout(this.s.stateTO);this.s.stateTO=setTimeout(function(){a.s.dt.oApi._fnSaveState(a.s.dt)},250);
if(c<this.s.redrawTop||c>this.s.redrawBottom)if(b=parseInt(c/this.s.rowHeight,10)-this.s.viewportRows,0>b?b=0:b+this.s.dt._iDisplayLength>this.s.dt.fnRecordsDisplay()?(b=this.s.dt.fnRecordsDisplay()-this.s.dt._iDisplayLength,0>b&&(b=0)):0!==b%2&&b++,b!=this.s.dt._iDisplayStart)this.s.tableTop=d(this.s.dt.nTable).offset().top,this.s.tableBottom=d(this.s.dt.nTable).height()+this.s.tableTop,this.s.dt.oFeatures.bServerSide?(clearTimeout(this.s.drawTO),this.s.drawTO=setTimeout(function(){a.s.dt._iDisplayStart=
b;a.s.dt.oApi._fnCalculateEnd(a.s.dt);a.s.dt.oApi._fnDraw(a.s.dt)},this.s.serverWait)):(this.s.dt._iDisplayStart=b,this.s.dt.oApi._fnCalculateEnd(this.s.dt),this.s.dt.oApi._fnDraw(this.s.dt)),this.s.trace&&console.log("Scroll forcing redraw - top DT render row: "+b)},_fnDrawCallback:function(){var a=this,c=this.dom.scroller.scrollTop;this.dom.force.style.height=this.s.rowHeight*this.s.dt.fnRecordsDisplay()+"px";var b=this.s.rowHeight*this.s.dt._iDisplayStart;0===this.s.dt._iDisplayStart?b=0:this.s.dt._iDisplayStart===
this.s.dt.fnRecordsDisplay()-this.s.dt._iDisplayLength&&(b=this.s.rowHeight*this.s.dt._iDisplayStart);this.dom.table.style.top=b+"px";this.s.tableTop=b;this.s.tableBottom=d(this.s.dt.nTable).height()+this.s.tableTop;this.s.redrawTop=c-this.s.viewportHeight/2;this.s.redrawBottom=this.s.tableBottom-1.5*this.s.viewportHeight;this.s.trace&&console.log("Table redraw. Table top: "+b+"px Table bottom: "+this.s.tableBottom+" Scroll boundary top: "+this.s.redrawTop+" Scroll boundary bottom: "+this.s.redrawBottom+
" Rows drawn: "+this.s.dt._iDisplayLength);setTimeout(function(){a._fnInfo.call(a)},0);this.s.dt.oFeatures.bStateSave&&null!==this.s.dt.oLoadedState&&"undefined"!=typeof this.s.dt.oLoadedState.iScroller&&(null!==this.s.dt.sAjaxSource&&2==this.s.dt.iDraw||null===this.s.dt.sAjaxSource&&1==this.s.dt.iDraw)&&setTimeout(function(){d(a.dom.scroller).scrollTop(a.s.dt.oLoadedState.iScroller);a.s.redrawTop=a.s.dt.oLoadedState.iScroller-a.s.viewportHeight/2},0)},_fnCalcRowHeight:function(){var a=h.createElement("div"),
c=this.s.dt.nTable.cloneNode(!1),b=h.createElement("tbody"),e=h.createElement("tr"),f=h.createElement("td");f.innerHTML="&nbsp;";e.appendChild(f);b.appendChild(e);c.appendChild(b);a.className=this.s.dt.oClasses.sScrollBody;a.appendChild(c);h.body.appendChild(a);this.s.rowHeight=d(e).height();h.body.removeChild(a)},_fnInfo:function(){if(this.s.dt.oFeatures.bInfo){var a=this.s.dt,c=this.dom.scroller.scrollTop,b=this.fnPixelsToRow(c)+1,e=a.fnRecordsTotal(),f=a.fnRecordsDisplay(),c=this.fnPixelsToRow(c+
d(this.dom.scroller).height()),c=f<c?f:c,b=a.fnFormatNumber(b),c=a.fnFormatNumber(c),e=a.fnFormatNumber(e),f=a.fnFormatNumber(f),f=0===a.fnRecordsDisplay()&&a.fnRecordsDisplay()==a.fnRecordsTotal()?a.oLanguage.sInfoEmpty+a.oLanguage.sInfoPostFix:0===a.fnRecordsDisplay()?a.oLanguage.sInfoEmpty+" "+a.oLanguage.sInfoFiltered.replace("_MAX_",e)+a.oLanguage.sInfoPostFix:a.fnRecordsDisplay()==a.fnRecordsTotal()?a.oLanguage.sInfo.replace("_START_",b).replace("_END_",c).replace("_TOTAL_",f)+a.oLanguage.sInfoPostFix:
a.oLanguage.sInfo.replace("_START_",b).replace("_END_",c).replace("_TOTAL_",f)+" "+a.oLanguage.sInfoFiltered.replace("_MAX_",a.fnFormatNumber(a.fnRecordsTotal()))+a.oLanguage.sInfoPostFix,a=a.aanFeatures.i;if("undefined"!=typeof a){e=0;for(b=a.length;e<b;e++)d(a[e]).html(f)}}}};g.oDefaults={trace:!1,rowHeight:"auto",serverWait:200};g.prototype.CLASS="Scroller";g.VERSION="1.0.1";g.prototype.CLASS=g.VERSION;"function"==typeof d.fn.dataTable&&"function"==typeof d.fn.dataTableExt.fnVersionCheck&&d.fn.dataTableExt.fnVersionCheck("1.8.0")?
d.fn.dataTableExt.aoFeatures.push({fnInit:function(a){return(new g(a,"undefined"==typeof a.oInit.oScroller?{}:a.oInit.oScroller)).dom.wrapper},cFeature:"S",sFeature:"Scroller"}):alert("Warning: Scroller requires DataTables 1.8.0 or greater - www.datatables.net/download")})(jQuery,window,document);
