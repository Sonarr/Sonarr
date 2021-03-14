using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NzbDrone.Core.Download.Clients.Aria2
{
    public class Aria2Version
    {
        public string version;
        public string[] enabledFeatures;
    }

    public class Aria2Uri
    {
        public string status;
        public string uri;
    }

    public class Aria2File
    {
        public string index;
        public string length;
        public string completedLength;
        public string path;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string selected;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public Aria2Uri[] uris;
    }

    public class Aria2Status
    {
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public XmlRpcStruct bittorrent;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string bitfield;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string infoHash;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string completedLength;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string connections;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string dir;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string downloadSpeed;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public Aria2File[] files;
        public string gid;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string numPieces;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string pieceLength;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string status;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string totalLength;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string uploadLength;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string uploadSpeed;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string errorMessage;
    }
}
