
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoTorrent
{
    public class MagnetLink
    {
        public RawTrackerTier AnnounceUrls {
            get; private set;
        }

        public InfoHash InfoHash {
            get; private set;
        }

        public string Name {
            get; private set;
        }

        public List<string> Webseeds {
            get; private set;
        }

        public MagnetLink (string url)
        {
            Check.Url (url);
            AnnounceUrls = new RawTrackerTier ();
            Webseeds  = new List<string> ();

            ParseMagnetLink (url);
        }

        void ParseMagnetLink (string url)
        {
            string[] splitStr = url.Split ('?');
            if (splitStr.Length == 0 || splitStr[0] != "magnet:")
                throw new FormatException ("The magnet link must start with 'magnet:?'.");

            if (splitStr.Length == 1)
                return;//no parametter

            string[] parameters = splitStr[1].Split ('&', ';');

            for (int i = 0; i < parameters.Length ; i++)
            {
                string[] keyval = parameters[i].Split ('=');
                if (keyval.Length != 2)
                    throw new FormatException ("A field-value pair of the magnet link contain more than one equal'.");
                switch (keyval[0].Substring(0, 2))
                {
                    case "xt"://exact topic
                        if (InfoHash != null)
                            throw new FormatException ("More than one infohash in magnet link is not allowed.");

                        string val = keyval[1].Substring(9);
                        switch (keyval[1].Substring(0, 9))
                        {
                            case "urn:sha1:"://base32 hash
                            case "urn:btih:":
                            if (val.Length == 32)
                                InfoHash = InfoHash.FromBase32 (val);
                            else if (val.Length == 40)
                                InfoHash = InfoHash.FromHex (val);
                            else
                                throw new FormatException("Infohash must be base32 or hex encoded.");
                            break;
                        }
                    break;
                    case "tr" ://address tracker
                        var bytes = UriHelper.UrlDecode(keyval[1]);
                        AnnounceUrls.Add(Encoding.UTF8.GetString(bytes));
                    break;
                    case "as"://Acceptable Source
                        Webseeds.Add (keyval[1]);
                    break;
                    case "dn"://display name
                        var name = UriHelper.UrlDecode(keyval[1]);
                        Name = Encoding.UTF8.GetString(name);
                    break;
                    case "xl"://exact length
                    case "xs":// eXact Source - P2P link.
                    case "kt"://keyword topic
                    case "mt"://manifest topic
                        //not supported for moment
                    break;
                    default:
                        //not supported
                    break;
                }
            }
        }
    }
}
