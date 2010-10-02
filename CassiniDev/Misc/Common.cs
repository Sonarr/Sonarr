//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.UI;

#endregion

namespace CassiniDev
{
    internal static class Common
    {
        public static string ConvertToHexView(this byte[] value, int numBytesPerRow)
        {
            if (value == null) return null;

            List<string> hexSplit = BitConverter.ToString(value)
                .Replace('-', ' ')
                .Trim()
                .SplitIntoChunks(numBytesPerRow*3)
                .ToList();

            int byteAddress = 0;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hexSplit.Count; i++)
            {
                sb.AppendLine(byteAddress.ToString("X4") + ":\t" + hexSplit[i]);
                byteAddress += numBytesPerRow;
            }

            return sb.ToString();
        }

        public static string GetAspVersion()
        {
            string version = null;
            try
            {
                Type type = typeof (Page);
                Assembly assembly = Assembly.GetAssembly(type);

                object[] customAttributes = assembly.GetCustomAttributes(typeof (AssemblyFileVersionAttribute), true);
                if ((customAttributes != null) && (customAttributes.GetLength(0) > 0))
                {
                    version = ((AssemblyFileVersionAttribute) customAttributes[0]).Version;
                }
                else
                {
                    version = assembly.GetName().Version.ToString();
                }
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }
            return version;
        }

        /// <summary>
        /// CassiniDev FIX: #12506
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetContentType(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            string contentType;

            FileInfo info = new FileInfo(fileName);
            string extension = info.Extension.ToLowerInvariant();

            switch (extension)
            {
                    //NOTE: these are fallbacks - and should be refined as needed
                    // Only if the request does not already know
                    // the content-type will this switch be hit - meaning that 
                    // served content-types for extensions listed here may not match
                    // as this method may not be polled.

                case ".svgz":
                    contentType = "image/svg+xml";
                    break;

                    // from registry - last resort - verified mappings follow

                case ".3g2":
                    contentType = "video/3gpp2";
                    break;
                case ".3gp":
                    contentType = "video/3gpp";
                    break;
                case ".3gp2":
                    contentType = "video/3gpp2";
                    break;
                case ".3gpp":
                    contentType = "video/3gpp";
                    break;
                case ".adt":
                    contentType = "audio/vnd.dlna.adts";
                    break;
                case ".amr":
                    contentType = "audio/AMR";
                    break;
                case ".addin":
                    contentType = "text/xml";
                    break;
                case ".evr":
                    contentType = "audio/evrc-qcp";
                    break;
                case ".evrc":
                    contentType = "audio/evrc-qcp";
                    break;
                case ".ssisdeploymentmanifest":
                    contentType = "text/xml";
                    break;
                case ".xoml":
                    contentType = "text/plain";
                    break;
                case ".aac":
                    contentType = "audio/aac";
                    break;
                case ".ac3":
                    contentType = "audio/ac3";
                    break;
                case ".accda":
                    contentType = "application/msaccess";
                    break;
                case ".accdb":
                    contentType = "application/msaccess";
                    break;
                case ".accdc":
                    contentType = "application/msaccess";
                    break;
                case ".accde":
                    contentType = "application/msaccess";
                    break;
                case ".accdr":
                    contentType = "application/msaccess";
                    break;
                case ".accdt":
                    contentType = "application/msaccess";
                    break;
                case ".acrobatsecuritysettings":
                    contentType = "application/vnd.adobe.acrobat-security-settings";
                    break;
                case ".ad":
                    contentType = "text/plain";
                    break;
                case ".ade":
                    contentType = "application/msaccess";
                    break;
                case ".adobebridge":
                    contentType = "application/x-bridge-url";
                    break;
                case ".adp":
                    contentType = "application/msaccess";
                    break;
                case ".adts":
                    contentType = "audio/vnd.dlna.adts";
                    break;
                case ".amc":
                    contentType = "application/x-mpeg";
                    break;
                case ".application":
                    contentType = "application/x-ms-application";
                    break;
                case ".asa":
                    contentType = "application/xml";
                    break;
                case ".asax":
                    contentType = "application/xml";
                    break;
                case ".ascx":
                    contentType = "application/xml";
                    break;

                case ".ashx":
                    contentType = "application/xml";
                    break;
                case ".asm":
                    contentType = "text/plain";
                    break;
                case ".asmx":
                    contentType = "application/xml";
                    break;
                case ".aspx":
                    contentType = "application/xml";
                    break;
                case ".awf":
                    contentType = "application/vnd.adobe.workflow";
                    break;
                case ".biz":
                    contentType = "text/xml";
                    break;

                case ".c2r":
                    contentType = "text/vnd-ms.click2record+xml";
                    break;
                case ".caf":
                    contentType = "audio/x-caf";
                    break;

                case ".cc":
                    contentType = "text/plain";
                    break;
                case ".cd":
                    contentType = "text/plain";
                    break;
                case ".cdda":
                    contentType = "audio/aiff";
                    break;

                case ".config":
                    contentType = "application/xml";
                    break;
                case ".contact":
                    contentType = "text/x-ms-contact";
                    break;
                case ".coverage":
                    contentType = "application/xml";
                    break;
                case ".cpp":
                    contentType = "text/plain";
                    break;
                case ".cs":
                    contentType = "text/plain";
                    break;
                case ".csdproj":
                    contentType = "text/plain";
                    break;
                case ".csproj":
                    contentType = "text/plain";
                    break;

                case ".csv":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".cur":
                    contentType = "text/plain";
                    break;
                case ".cxx":
                    contentType = "text/plain";
                    break;
                case ".datasource":
                    contentType = "application/xml";
                    break;
                case ".dbproj":
                    contentType = "text/plain";
                    break;
                case ".dcd":
                    contentType = "text/xml";
                    break;
                case ".dd":
                    contentType = "text/plain";
                    break;
                case ".def":
                    contentType = "text/plain";
                    break;

                case ".design":
                    contentType = "image/design";
                    break;
                case ".dgml":
                    contentType = "application/xml";
                    break;
                case ".dib":
                    contentType = "image/bmp";
                    break;
                case ".dif":
                    contentType = "video/x-dv";
                    break;
                case ".docm":
                    contentType = "application/vnd.ms-word.document.macroEnabled.12";
                    break;
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".dotm":
                    contentType = "application/vnd.ms-word.template.macroEnabled.12";
                    break;
                case ".dotx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.template";
                    break;
                case ".dsp":
                    contentType = "text/plain";
                    break;
                case ".dsprototype":
                    contentType = "text/plain";
                    break;
                case ".dsw":
                    contentType = "text/plain";
                    break;
                case ".dtd":
                    contentType = "application/xml-dtd";
                    break;
                case ".dtsconfig":
                    contentType = "text/xml";
                    break;
                case ".dv":
                    contentType = "video/x-dv";
                    break;
                case ".dwfx":
                    contentType = "model/vnd.dwfx+xps";
                    break;
                case ".easmx":
                    contentType = "model/vnd.easmx+xps";
                    break;
                case ".edrwx":
                    contentType = "model/vnd.edrwx+xps";
                    break;
                case ".eprtx":
                    contentType = "model/vnd.eprtx+xps";
                    break;
                case ".fdf":
                    contentType = "application/vnd.fdf";
                    break;
                case ".filters":
                    contentType = "Application/xml";
                    break;
                case ".flc":
                    contentType = "video/flc";
                    break;
                case ".fo":
                    contentType = "text/xml";
                    break;
                case ".fsscript":
                    contentType = "application/fsharp-script";
                    break;
                case ".fsx":
                    contentType = "application/fsharp-script";
                    break;
                case ".generictest":
                    contentType = "application/xml";
                    break;
                case ".group":
                    contentType = "text/x-ms-group";
                    break;
                case ".gsm":
                    contentType = "audio/x-gsm";
                    break;
                case ".hpp":
                    contentType = "text/plain";
                    break;
                case ".hxa":
                    contentType = "application/xml";
                    break;
                case ".hxc":
                    contentType = "application/xml";
                    break;
                case ".hxd":
                    contentType = "application/octet-stream";
                    break;
                case ".hxe":
                    contentType = "application/xml";
                    break;
                case ".hxf":
                    contentType = "application/xml";
                    break;
                case ".hxh":
                    contentType = "application/octet-stream";
                    break;
                case ".hxi":
                    contentType = "application/octet-stream";
                    break;
                case ".hxk":
                    contentType = "application/xml";
                    break;
                case ".hxq":
                    contentType = "application/octet-stream";
                    break;
                case ".hxr":
                    contentType = "application/octet-stream";
                    break;
                case ".hxs":
                    contentType = "application/octet-stream";
                    break;
                case ".hxt":
                    contentType = "application/xml";
                    break;
                case ".hxv":
                    contentType = "application/xml";
                    break;
                case ".hxw":
                    contentType = "application/octet-stream";
                    break;
                case ".hxx":
                    contentType = "text/plain";
                    break;
                case ".i":
                    contentType = "text/plain";
                    break;
                case ".idl":
                    contentType = "text/plain";
                    break;
                case ".inc":
                    contentType = "text/plain";
                    break;
                case ".inl":
                    contentType = "text/plain";
                    break;
                case ".ipproj":
                    contentType = "text/plain";
                    break;
                case ".iqy":
                    contentType = "text/x-ms-iqy";
                    break;
                case ".ismv":
                    contentType = "video/ismv";
                    break;
                case ".jsx":
                    contentType = "text/plain";
                    break;
                case ".jsxbin":
                    contentType = "text/plain";
                    break;
                case ".jtx":
                    contentType = "application/x-jtx+xps";
                    break;
                case ".ldd":
                    contentType = "text/plain";
                    break;
                case ".library-ms":
                    contentType = "application/windows-library+xml";
                    break;
                case ".loadtest":
                    contentType = "application/xml";
                    break;
                case ".lsaprototype":
                    contentType = "text/plain";
                    break;
                case ".lst":
                    contentType = "text/plain";
                    break;
                case ".m1v":
                    contentType = "video/mpeg";
                    break;
                case ".m2t":
                    contentType = "video/vnd.dlna.mpeg-tts";
                    break;
                case ".m2ts":
                    contentType = "video/vnd.dlna.mpeg-tts";
                    break;
                case ".m2v":
                    contentType = "video/mpeg";
                    break;
                case ".m4a":
                    contentType = "audio/mp4";
                    break;
                case ".m4b":
                    contentType = "audio/x-m4b";
                    break;
                case ".m4p":
                    contentType = "audio/x-m4p";
                    break;
                case ".m4v":
                    contentType = "video/x-m4v";
                    break;
                case ".mac":
                    contentType = "image/x-macpaint";
                    break;
                case ".mak":
                    contentType = "text/plain";
                    break;
                case ".map":
                    contentType = "text/plain";
                    break;
                case ".master":
                    contentType = "application/xml";
                    break;
                case ".mda":
                    contentType = "application/msaccess";
                    break;
                case ".mde":
                    contentType = "application/msaccess";
                    break;
                case ".mdp":
                    contentType = "text/plain";
                    break;
                case ".mfp":
                    contentType = "application/x-shockwave-flash";
                    break;
                case ".mk":
                    contentType = "text/plain";
                    break;
                case ".mod":
                    contentType = "video/mpeg";
                    break;
                case ".mp2v":
                    contentType = "video/mpeg";
                    break;
                case ".mp4":
                    contentType = "video/mp4";
                    break;
                case ".mp4v":
                    contentType = "video/mp4";
                    break;
                case ".mpf":
                    contentType = "application/vnd.ms-mediapackage";
                    break;
                case ".mqv":
                    contentType = "video/quicktime";
                    break;
                case ".mts":
                    contentType = "video/vnd.dlna.mpeg-tts";
                    break;
                case ".mtx":
                    contentType = "application/xml";
                    break;
                case ".mxp":
                    contentType = "application/x-mmxp";
                    break;
                case ".nix":
                    contentType = "application/x-mix-transfer";
                    break;
                case ".odc":
                    contentType = "text/x-ms-odc";
                    break;
                case ".odh":
                    contentType = "text/plain";
                    break;
                case ".odl":
                    contentType = "text/plain";
                    break;
                case ".odp":
                    contentType = "application/vnd.oasis.opendocument.presentation";
                    break;
                case ".ods":
                    contentType = "application/vnd.oasis.opendocument.spreadsheet";
                    break;
                case ".odt":
                    contentType = "application/vnd.oasis.opendocument.text";
                    break;
                case ".orderedtest":
                    contentType = "application/xml";
                    break;
                case ".osdx":
                    contentType = "application/opensearchdescription+xml";
                    break;
                case ".pct":
                    contentType = "image/pict";
                    break;
                case ".pcx":
                    contentType = "image/x-pcx";
                    break;

                case ".pdfxml":
                    contentType = "application/vnd.adobe.pdfxml";
                    break;
                case ".pdx":
                    contentType = "application/vnd.adobe.pdx";
                    break;
                case ".pic":
                    contentType = "image/pict";
                    break;
                case ".pict":
                    contentType = "image/pict";
                    break;
                case ".pkgdef":
                    contentType = "text/plain";
                    break;
                case ".pkgundef":
                    contentType = "text/plain";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                case ".pnt":
                    contentType = "image/x-macpaint";
                    break;
                case ".pntg":
                    contentType = "image/x-macpaint";
                    break;
                case ".potm":
                    contentType = "application/vnd.ms-powerpoint.template.macroEnabled.12";
                    break;
                case ".potx":
                    contentType = "application/vnd.openxmlformats-officedocument.presentationml.template";
                    break;
                case ".ppa":
                    contentType = "application/vnd.ms-powerpoint";
                    break;
                case ".ppam":
                    contentType = "application/vnd.ms-powerpoint.addin.macroEnabled.12";
                    break;
                case ".ppsm":
                    contentType = "application/vnd.ms-powerpoint.slideshow.macroEnabled.12";
                    break;
                case ".ppsx":
                    contentType = "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                    break;
                case ".pptm":
                    contentType = "application/vnd.ms-powerpoint.presentation.macroEnabled.12";
                    break;
                case ".pptx":
                    contentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
                case ".psc1":
                    contentType = "application/PowerShell";
                    break;
                case ".psess":
                    contentType = "application/xml";
                    break;
                case ".pwz":
                    contentType = "application/vnd.ms-powerpoint";
                    break;
                case ".pxr":
                    contentType = "image/pxr";
                    break;
                case ".qcp":
                    contentType = "audio/vnd.qcelp";
                    break;
                case ".qht":
                    contentType = "text/x-html-insertion";
                    break;
                case ".qhtm":
                    contentType = "text/x-html-insertion";
                    break;
                case ".qti":
                    contentType = "image/x-quicktime";
                    break;
                case ".qtif":
                    contentType = "image/x-quicktime";
                    break;
                case ".qtl":
                    contentType = "application/x-quicktimeplayer";
                    break;
                case ".rat":
                    contentType = "application/rat-file";
                    break;
                case ".raw":
                    contentType = "application/octet-stream";
                    break;

                case ".rc":
                    contentType = "text/plain";
                    break;
                case ".rc2":
                    contentType = "text/plain";
                    break;
                case ".rct":
                    contentType = "text/plain";
                    break;
                case ".rdf":
                    contentType = "text/xml";
                    break;
                case ".rdlc":
                    contentType = "application/xml";
                    break;
                case ".rels":
                    contentType = "application/vnd.ms-package.relationships+xml";
                    break;
                case ".resx":
                    contentType = "application/xml";
                    break;
                case ".rgs":
                    contentType = "text/plain";
                    break;
                case ".rjt":
                    contentType = "application/vnd.rn-realsystem-rjt";
                    break;
                case ".rm":
                    contentType = "application/vnd.rn-realmedia";
                    break;
                case ".rmf":
                    contentType = "application/vnd.adobe.rmf";
                    break;
                case ".rmj":
                    contentType = "application/vnd.rn-realsystem-rmj";
                    break;
                case ".rmm":
                    contentType = "audio/x-pn-realaudio";
                    break;
                case ".rmp":
                    contentType = "application/vnd.rn-rn_music_package";
                    break;
                case ".rms":
                    contentType = "application/vnd.rn-realaudio-secure";
                    break;
                case ".rmvb":
                    contentType = "application/vnd.rn-realmedia-vbr";
                    break;
                case ".rmx":
                    contentType = "application/vnd.rn-realsystem-rmx";
                    break;
                case ".rnx":
                    contentType = "application/vnd.rn-realplayer";
                    break;
                case ".rp":
                    contentType = "image/vnd.rn-realpix";
                    break;
                case ".rpm":
                    contentType = "audio/x-pn-realaudio-plugin";
                    break;
                case ".rqy":
                    contentType = "text/x-ms-rqy";
                    break;
                case ".rsml":
                    contentType = "application/vnd.rn-rsml";
                    break;
                case ".rt":
                    contentType = "text/vnd.rn-realtext";
                    break;
                case ".rtsp":
                    contentType = "application/x-rtsp";
                    break;
                case ".ruleset":
                    contentType = "application/xml";
                    break;
                case ".rv":
                    contentType = "video/vnd.rn-realvideo";
                    break;
                case ".s":
                    contentType = "text/plain";
                    break;
                case ".sd":
                    contentType = "text/plain";
                    break;
                case ".sd2":
                    contentType = "audio/x-sd2";
                    break;
                case ".sdm":
                    contentType = "text/plain";
                    break;
                case ".sdmdocument":
                    contentType = "text/plain";
                    break;
                case ".sdp":
                    contentType = "application/sdp";
                    break;
                case ".sdv":
                    contentType = "video/sd-video";
                    break;
                case ".searchConnector-ms":
                    contentType = "application/windows-search-connector+xml";
                    break;
                case ".settings":
                    contentType = "application/xml";
                    break;
                case ".sgi":
                    contentType = "image/x-sgi";
                    break;
                case ".shtml":
                    contentType = "text/html";
                    break;
                case ".sitemap":
                    contentType = "application/xml";
                    break;
                case ".skin":
                    contentType = "application/xml";
                    break;
                case ".sldm":
                    contentType = "application/vnd.ms-powerpoint.slide.macroEnabled.12";
                    break;
                case ".sldx":
                    contentType = "application/vnd.openxmlformats-officedocument.presentationml.slide";
                    break;
                case ".slk":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".sln":
                    contentType = "text/plain";
                    break;
                case ".slupkg-ms":
                    contentType = "application/x-ms-license";
                    break;
                case ".smi":
                    contentType = "application/smil";
                    break;
                case ".smil":
                    contentType = "application/smil";
                    break;
                case ".snippet":
                    contentType = "application/xml";
                    break;
                case ".sol":
                    contentType = "text/plain";
                    break;
                case ".sor":
                    contentType = "text/plain";
                    break;
                case ".srf":
                    contentType = "text/plain";
                    break;
                case ".svc":
                    contentType = "application/xml";
                    break;
                case ".tga":
                    contentType = "image/x-targa";
                    break;
                case ".targa":
                    contentType = "image/x-targa";
                    break;
                case ".testrunconfig":
                    contentType = "application/xml";
                    break;
                case ".testsettings":
                    contentType = "application/xml";
                    break;
                case ".thmx":
                    contentType = "application/vnd.ms-officetheme";
                    break;
                case ".tlh":
                    contentType = "text/plain";
                    break;
                case ".tli":
                    contentType = "text/plain";
                    break;
                case ".trx":
                    contentType = "application/xml";
                    break;
                case ".ts":
                    contentType = "video/vnd.dlna.mpeg-tts";
                    break;
                case ".tts":
                    contentType = "video/vnd.dlna.mpeg-tts";
                    break;
                case ".user":
                    contentType = "text/plain";
                    break;
                case ".vb":
                    contentType = "text/plain";
                    break;
                case ".vbdproj":
                    contentType = "text/plain";
                    break;
                case ".vbproj":
                    contentType = "text/plain";
                    break;
                case ".vcproj":
                    contentType = "Application/xml";
                    break;
                case ".vcxproj":
                    contentType = "Application/xml";
                    break;
                case ".vddproj":
                    contentType = "text/plain";
                    break;
                case ".vdp":
                    contentType = "text/plain";
                    break;
                case ".vdproj":
                    contentType = "text/plain";
                    break;
                case ".vdx":
                    contentType = "application/vnd.visio";
                    break;
                case ".vscontent":
                    contentType = "application/xml";
                    break;
                case ".vsct":
                    contentType = "text/xml";
                    break;
                case ".vsd":
                    contentType = "application/vnd.visio";
                    break;
                case ".vsi":
                    contentType = "application/ms-vsi";
                    break;
                case ".vsix":
                    contentType = "application/vsix";
                    break;
                case ".vsixlangpack":
                    contentType = "text/xml";
                    break;
                case ".vsixmanifest":
                    contentType = "text/xml";
                    break;
                case ".vsl":
                    contentType = "application/vnd.visio";
                    break;
                case ".vsmdi":
                    contentType = "application/xml";
                    break;
                case ".vspscc":
                    contentType = "text/plain";
                    break;
                case ".vss":
                    contentType = "application/vnd.visio";
                    break;
                case ".vsscc":
                    contentType = "text/plain";
                    break;
                case ".vssettings":
                    contentType = "text/xml";
                    break;
                case ".vssscc":
                    contentType = "text/plain";
                    break;
                case ".vst":
                    contentType = "application/vnd.visio";
                    break;
                case ".vstemplate":
                    contentType = "text/xml";
                    break;
                case ".vsto":
                    contentType = "application/x-ms-vsto";
                    break;
                case ".vsu":
                    contentType = "application/vnd.visio";
                    break;
                case ".vsw":
                    contentType = "application/vnd.visio";
                    break;
                case ".vsx":
                    contentType = "application/vnd.visio";
                    break;
                case ".vtx":
                    contentType = "application/vnd.visio";
                    break;
                case ".wax":
                    contentType = "audio/x-ms-wax";
                    break;
                case ".wbk":
                    contentType = "application/msword";
                    break;
                case ".wdp":
                    contentType = "image/vnd.ms-photo";
                    break;
                case ".webtest":
                    contentType = "application/xml";
                    break;
                case ".wiq":
                    contentType = "application/xml";
                    break;
                case ".wiz":
                    contentType = "application/msword";
                    break;
                case ".wm":
                    contentType = "video/x-ms-wm";
                    break;
                case ".wma":
                    contentType = "audio/x-ms-wma";
                    break;
                case ".wmd":
                    contentType = "application/x-ms-wmd";
                    break;
                case ".wmv":
                    contentType = "video/x-ms-wmv";
                    break;
                case ".wmx":
                    contentType = "video/x-ms-wmx";
                    break;
                case ".wmz":
                    contentType = "application/x-ms-wmz";
                    break;
                case ".wpl":
                    contentType = "application/vnd.ms-wpl";
                    break;
                case ".wsc":
                    contentType = "text/scriptlet";
                    break;
                case ".wsdl":
                    contentType = "application/xml";
                    break;
                case ".wvx":
                    contentType = "video/x-ms-wvx";
                    break;
                case ".xaml":
                    contentType = "application/xaml+xml";
                    break;
                case ".xbap":
                    contentType = "application/x-ms-xbap";
                    break;
                case ".xbrl":
                    contentType = "text/xml";
                    break;
                case ".xdp":
                    contentType = "application/vnd.adobe.xdp+xml";
                    break;
                case ".xdr":
                    contentType = "application/xml";
                    break;
                case ".xej":
                    contentType = "application/xej+xml";
                    break;
                case ".xel":
                    contentType = "application/xel+xml";
                    break;
                case ".xesc":
                    contentType = "application/x-ms-wmv";
                    break;
                case ".xfd":
                    contentType = "application/vnd.adobe.xfd+xml";
                    break;
                case ".xfdf":
                    contentType = "application/vnd.adobe.xfdf";
                    break;
                case ".xht":
                    contentType = "application/xhtml+xml";
                    break;
                case ".xhtml":
                    contentType = "application/xhtml+xml";
                    break;
                case ".xlam":
                    contentType = "application/vnd.ms-excel.addin.macroEnabled.12";
                    break;
                case ".xlk":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xll":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xlsb":
                    contentType = "application/vnd.ms-excel.sheet.binary.macroEnabled.12";
                    break;
                case ".xlsm":
                    contentType = "application/vnd.ms-excel.sheet.macroEnabled.12";
                    break;
                case ".xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".xltm":
                    contentType = "application/vnd.ms-excel.template.macroEnabled.12";
                    break;
                case ".xltx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.template";
                    break;
                case ".xml":
                    contentType = "application/xml";
                    break;
                case ".xmta":
                    contentType = "application/xml";
                    break;
                case ".xpr":
                    contentType = "image/xpr";
                    break;
                case ".xps":
                    contentType = "application/vnd.ms-xpsdocument";
                    break;
                case ".xrm-ms":
                    contentType = "text/xml";
                    break;
                case ".xsc":
                    contentType = "application/xml";
                    break;
                case ".xsd":
                    contentType = "application/xml";
                    break;
                case ".xsl":
                    contentType = "text/xml";
                    break;
                case ".xslt":
                    contentType = "application/xml";
                    break;
                case ".xss":
                    contentType = "application/xml";
                    break;

                    // standard mappings from http://www.w3schools.com/media/media_mimeref.asp

                case ".323":
                    contentType = "text/h323";
                    break;
                case ".acx":
                    contentType = "application/internet-property-stream";
                    break;
                case ".ai":
                    contentType = "application/postscript";
                    break;
                case ".aif":
                    contentType = "audio/x-aiff";
                    break;
                case ".aifc":
                    contentType = "audio/x-aiff";
                    break;
                case ".aiff":
                    contentType = "audio/x-aiff";
                    break;
                case ".asf":
                    contentType = "video/x-ms-asf";
                    break;
                case ".asr":
                    contentType = "video/x-ms-asf";
                    break;
                case ".asx":
                    contentType = "video/x-ms-asf";
                    break;
                case ".au":
                    contentType = "audio/basic";
                    break;
                case ".avi":
                    contentType = "video/x-msvideo";
                    break;
                case ".axs":
                    contentType = "application/olescript";
                    break;
                case ".bas":
                    contentType = "text/plain";
                    break;
                case ".bcpio":
                    contentType = "application/x-bcpio";
                    break;
                case ".bin":
                    contentType = "application/octet-stream";
                    break;
                case ".bmp":
                    contentType = "image/bmp";
                    break;
                case ".c":
                    contentType = "text/plain";
                    break;
                case ".cat":
                    contentType = "application/vnd.ms-pkiseccat";
                    break;
                case ".cdf":
                    contentType = "application/x-cdf";
                    break;
                case ".cer":
                    contentType = "application/x-x509-ca-cert";
                    break;
                case ".class":
                    contentType = "application/octet-stream";
                    break;
                case ".clp":
                    contentType = "application/x-msclip";
                    break;
                case ".cmx":
                    contentType = "image/x-cmx";
                    break;
                case ".cod":
                    contentType = "image/cis-cod";
                    break;
                case ".cpio":
                    contentType = "application/x-cpio";
                    break;
                case ".crd":
                    contentType = "application/x-mscardfile";
                    break;
                case ".crl":
                    contentType = "application/pkix-crl";
                    break;
                case ".crt":
                    contentType = "application/x-x509-ca-cert";
                    break;
                case ".csh":
                    contentType = "application/x-csh";
                    break;
                case ".css":
                    contentType = "text/css";
                    break;
                case ".dcr":
                    contentType = "application/x-director";
                    break;
                case ".der":
                    contentType = "application/x-x509-ca-cert";
                    break;
                case ".dir":
                    contentType = "application/x-director";
                    break;
                case ".dll":
                    contentType = "application/x-msdownload";
                    break;
                case ".dms":
                    contentType = "application/octet-stream";
                    break;
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".dot":
                    contentType = "application/msword";
                    break;
                case ".dvi":
                    contentType = "application/x-dvi";
                    break;
                case ".dxr":
                    contentType = "application/x-director";
                    break;
                case ".eps":
                    contentType = "application/postscript";
                    break;
                case ".etx":
                    contentType = "text/x-setext";
                    break;
                case ".evy":
                    contentType = "application/envoy";
                    break;
                case ".exe":
                    contentType = "application/octet-stream";
                    break;
                case ".fif":
                    contentType = "application/fractals";
                    break;
                case ".flr":
                    contentType = "x-world/x-vrml";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".gtar":
                    contentType = "application/x-gtar";
                    break;
                case ".gz":
                    contentType = "application/x-gzip";
                    break;
                case ".h":
                    contentType = "text/plain";
                    break;
                case ".hdf":
                    contentType = "application/x-hdf";
                    break;
                case ".hlp":
                    contentType = "application/winhlp";
                    break;
                case ".hqx":
                    contentType = "application/mac-binhex40";
                    break;
                case ".hta":
                    contentType = "application/hta";
                    break;
                case ".htc":
                    contentType = "text/x-component";
                    break;
                case ".htm":
                    contentType = "text/html";
                    break;
                case ".html":
                    contentType = "text/html";
                    break;
                case ".htt":
                    contentType = "text/webviewhtml";
                    break;
                case ".ico":
                    contentType = "image/x-icon";
                    break;
                case ".ief":
                    contentType = "image/ief";
                    break;
                case ".iii":
                    contentType = "application/x-iphone";
                    break;
                case ".ins":
                    contentType = "application/x-internet-signup";
                    break;
                case ".isp":
                    contentType = "application/x-internet-signup";
                    break;
                case ".jfif":
                    contentType = "image/pipeg";
                    break;
                case ".jpe":
                    contentType = "image/jpeg";
                    break;
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".jpg":
                    contentType = "image/jpeg";
                    break;
                case ".js":
                    contentType = "application/x-javascript";
                    break;
                case ".latex":
                    contentType = "application/x-latex";
                    break;
                case ".lha":
                    contentType = "application/octet-stream";
                    break;
                case ".lsf":
                    contentType = "video/x-la-asf";
                    break;
                case ".lsx":
                    contentType = "video/x-la-asf";
                    break;
                case ".lzh":
                    contentType = "application/octet-stream";
                    break;
                case ".m13":
                    contentType = "application/x-msmediaview";
                    break;
                case ".m14":
                    contentType = "application/x-msmediaview";
                    break;
                case ".m3u":
                    contentType = "audio/x-mpegurl";
                    break;
                case ".man":
                    contentType = "application/x-troff-man";
                    break;
                case ".mdb":
                    contentType = "application/x-msaccess";
                    break;
                case ".me":
                    contentType = "application/x-troff-me";
                    break;
                case ".mht":
                    contentType = "message/rfc822";
                    break;
                case ".mhtml":
                    contentType = "message/rfc822";
                    break;
                case ".mid":
                    contentType = "audio/mid";
                    break;
                case ".mny":
                    contentType = "application/x-msmoney";
                    break;
                case ".mov":
                    contentType = "video/quicktime";
                    break;
                case ".movie":
                    contentType = "video/x-sgi-movie";
                    break;
                case ".mp2":
                    contentType = "video/mpeg";
                    break;
                case ".mp3":
                    contentType = "audio/mpeg";
                    break;
                case ".mpa":
                    contentType = "video/mpeg";
                    break;
                case ".mpe":
                    contentType = "video/mpeg";
                    break;
                case ".mpeg":
                    contentType = "video/mpeg";
                    break;
                case ".mpg":
                    contentType = "video/mpeg";
                    break;
                case ".mpp":
                    contentType = "application/vnd.ms-project";
                    break;
                case ".mpv2":
                    contentType = "video/mpeg";
                    break;
                case ".ms":
                    contentType = "application/x-troff-ms";
                    break;
                case ".msg":
                    contentType = "application/vnd.ms-outlook";
                    break;
                case ".mvb":
                    contentType = "application/x-msmediaview";
                    break;
                case ".nc":
                    contentType = "application/x-netcdf";
                    break;
                case ".nws":
                    contentType = "message/rfc822";
                    break;
                case ".oda":
                    contentType = "application/oda";
                    break;
                case ".p10":
                    contentType = "application/pkcs10";
                    break;
                case ".p12":
                    contentType = "application/x-pkcs12";
                    break;
                case ".p7b":
                    contentType = "application/x-pkcs7-certificates";
                    break;
                case ".p7c":
                    contentType = "application/x-pkcs7-mime";
                    break;
                case ".p7m":
                    contentType = "application/x-pkcs7-mime";
                    break;
                case ".p7r":
                    contentType = "application/x-pkcs7-certreqresp";
                    break;
                case ".p7s":
                    contentType = "application/x-pkcs7-signature";
                    break;
                case ".pbm":
                    contentType = "image/x-portable-bitmap";
                    break;
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".pfx":
                    contentType = "application/x-pkcs12";
                    break;
                case ".pgm":
                    contentType = "image/x-portable-graymap";
                    break;
                case ".pko":
                    contentType = "application/ynd.ms-pkipko";
                    break;
                case ".pma":
                    contentType = "application/x-perfmon";
                    break;
                case ".pmc":
                    contentType = "application/x-perfmon";
                    break;
                case ".pml":
                    contentType = "application/x-perfmon";
                    break;
                case ".pmr":
                    contentType = "application/x-perfmon";
                    break;
                case ".pmw":
                    contentType = "application/x-perfmon";
                    break;
                case ".pnm":
                    contentType = "image/x-portable-anymap";
                    break;
                case ".pot":
                    contentType = "application/vnd.ms-powerpoint";
                    break;
                case ".pot,":
                    contentType = "application/vnd.ms-powerpoint";
                    break;
                case ".ppm":
                    contentType = "image/x-portable-pixmap";
                    break;
                case ".pps":
                    contentType = "application/vnd.ms-powerpoint";
                    break;
                case ".ppt":
                    contentType = "application/vnd.ms-powerpoint";
                    break;
                case ".prf":
                    contentType = "application/pics-rules";
                    break;
                case ".ps":
                    contentType = "application/postscript";
                    break;
                case ".pub":
                    contentType = "application/x-mspublisher";
                    break;
                case ".qt":
                    contentType = "video/quicktime";
                    break;
                case ".ra":
                    contentType = "audio/x-pn-realaudio";
                    break;
                case ".ram":
                    contentType = "audio/x-pn-realaudio";
                    break;
                case ".ras":
                    contentType = "image/x-cmu-raster";
                    break;
                case ".rgb":
                    contentType = "image/x-rgb";
                    break;
                case ".rmi":
                    contentType = "audio/mid";
                    break;
                case ".roff":
                    contentType = "application/x-troff";
                    break;
                case ".rtf":
                    contentType = "application/rtf";
                    break;
                case ".rtx":
                    contentType = "text/richtext";
                    break;
                case ".scd":
                    contentType = "application/x-msschedule";
                    break;
                case ".sct":
                    contentType = "text/scriptlet";
                    break;
                case ".setpay":
                    contentType = "application/set-payment-initiation";
                    break;
                case ".setreg":
                    contentType = "application/set-registration-initiation";
                    break;
                case ".sh":
                    contentType = "application/x-sh";
                    break;
                case ".shar":
                    contentType = "application/x-shar";
                    break;
                case ".sit":
                    contentType = "application/x-stuffit";
                    break;
                case ".snd":
                    contentType = "audio/basic";
                    break;
                case ".spc":
                    contentType = "application/x-pkcs7-certificates";
                    break;
                case ".spl":
                    contentType = "application/futuresplash";
                    break;
                case ".src":
                    contentType = "application/x-wais-source";
                    break;
                case ".sst":
                    contentType = "application/vnd.ms-pkicertstore";
                    break;
                case ".stl":
                    contentType = "application/vnd.ms-pkistl";
                    break;
                case ".stm":
                    contentType = "text/html";
                    break;
                case ".sv4cpio":
                    contentType = "application/x-sv4cpio";
                    break;
                case ".sv4crc":
                    contentType = "application/x-sv4crc";
                    break;
                case ".svg":
                    contentType = "image/svg+xml";
                    break;
                case ".swf":
                    contentType = "application/x-shockwave-flash";
                    break;
                case ".t":
                    contentType = "application/x-troff";
                    break;
                case ".tar":
                    contentType = "application/x-tar";
                    break;
                case ".tcl":
                    contentType = "application/x-tcl";
                    break;
                case ".tex":
                    contentType = "application/x-tex";
                    break;
                case ".texi":
                    contentType = "application/x-texinfo";
                    break;
                case ".texinfo":
                    contentType = "application/x-texinfo";
                    break;
                case ".tgz":
                    contentType = "application/x-compressed";
                    break;
                case ".tif":
                    contentType = "image/tiff";
                    break;
                case ".tiff":
                    contentType = "image/tiff";
                    break;
                case ".tr":
                    contentType = "application/x-troff";
                    break;
                case ".trm":
                    contentType = "application/x-msterminal";
                    break;
                case ".tsv":
                    contentType = "text/tab-separated-values";
                    break;
                case ".txt":
                    contentType = "text/plain";
                    break;
                case ".uls":
                    contentType = "text/iuls";
                    break;
                case ".ustar":
                    contentType = "application/x-ustar";
                    break;
                case ".vcf":
                    contentType = "text/x-vcard";
                    break;
                case ".vrml":
                    contentType = "x-world/x-vrml";
                    break;
                case ".wav":
                    contentType = "audio/x-wav";
                    break;
                case ".wcm":
                    contentType = "application/vnd.ms-works";
                    break;
                case ".wdb":
                    contentType = "application/vnd.ms-works";
                    break;
                case ".wks":
                    contentType = "application/vnd.ms-works";
                    break;
                case ".wmf":
                    contentType = "application/x-msmetafile";
                    break;
                case ".wps":
                    contentType = "application/vnd.ms-works";
                    break;
                case ".wri":
                    contentType = "application/x-mswrite";
                    break;
                case ".wrl":
                    contentType = "x-world/x-vrml";
                    break;
                case ".wrz":
                    contentType = "x-world/x-vrml";
                    break;
                case ".xaf":
                    contentType = "x-world/x-vrml";
                    break;
                case ".xbm":
                    contentType = "image/x-xbitmap";
                    break;
                case ".xla":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xlc":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xlm":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xls":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xlt":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xlw":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xof":
                    contentType = "x-world/x-vrml";
                    break;
                case ".xpm":
                    contentType = "image/x-xpixmap";
                    break;
                case ".xwd":
                    contentType = "image/x-xwindowdump";
                    break;
                case ".z":
                    contentType = "application/x-compress";
                    break;
                case ".zip":
                    contentType = "application/zip";
                    break;

                default:
                    // this should be used as a last resort only. i.e. svg files return text/xml
                    contentType = GetMimeFromFile(fileName);
                    break;
            }
            return contentType;
        }

        public static T GetValueOrDefault<T>(this IDataRecord row, string fieldName)
        {
            int ordinal = row.GetOrdinal(fieldName);
            return row.GetValueOrDefault<T>(ordinal);
        }

        public static T GetValueOrDefault<T>(this IDataRecord row, int ordinal)
        {
            return (T) (row.IsDBNull(ordinal) ? default(T) : row.GetValue(ordinal));
        }

        public static byte[] StreamToBytes(this Stream input)
        {
            int capacity = input.CanSeek ? (int) input.Length : 0;
            using (MemoryStream output = new MemoryStream(capacity))
            {
                int readLength;
                byte[] buffer = new byte[4096];

                do
                {
                    readLength = input.Read(buffer, 0, buffer.Length);
                    output.Write(buffer, 0, readLength);
                } while (readLength != 0);

                return output.ToArray();
            }
        }

        /// <summary>
        /// CassiniDev FIX: #12506
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string GetMimeFromFile(string file)
        {
            IntPtr mimeout;
            if (!File.Exists(file))
                return null;
            //throw new FileNotFoundException(file + " not found");

            int maxContent = (int) new FileInfo(file).Length;
            if (maxContent > 4096) maxContent = 4096;
            FileStream fs = File.OpenRead(file);

            byte[] buf = new byte[maxContent];
            fs.Read(buf, 0, maxContent);
            fs.Close();
            int result = Interop.FindMimeFromData(IntPtr.Zero, file, buf, maxContent, null, 0, out mimeout, 0);

            if (result != 0)
                throw Marshal.GetExceptionForHR(result);
            string mime = Marshal.PtrToStringUni(mimeout);
            Marshal.FreeCoTaskMem(mimeout);
            return mime;
        }

        private static IList<string> SplitIntoChunks(this string text, int chunkSize)
        {
            List<string> chunks = new List<string>();
            int offset = 0;
            while (offset < text.Length)
            {
                int size = Math.Min(chunkSize, text.Length - offset);
                chunks.Add(text.Substring(offset, size));
                offset += size;
            }
            return chunks;
        }
    }

    public enum RunState
    {
        Idle = 0,
        Running
    }

    public enum PortMode
    {
        FirstAvailable = 0,
        Specific
    }

    public enum ErrorField
    {
        None,
        ApplicationPath,
        VirtualPath,
        HostName,
        IsAddHost,
        IPAddress,
        IPAddressAny,
        IPAddressLoopBack,
        Port,
        PortRangeStart,
        PortRangeEnd,
        PortRange
    }

    public enum IPMode
    {
        Loopback = 0,
        Any,
        Specific
    }

    public enum RunMode
    {
        Server,
        Hostsfile
    }

    internal class CassiniException : Exception
    {
        public CassiniException(string message, ErrorField field, Exception innerException)
            : base(message, innerException)
        {
            Field = field;
        }

        public CassiniException(string message, ErrorField field)
            : this(message, field, null)
        {
        }

        public ErrorField Field { get; set; }
    }
}