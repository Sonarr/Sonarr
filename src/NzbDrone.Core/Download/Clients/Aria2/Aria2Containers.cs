using CookComputing.XmlRpc;

namespace NzbDrone.Core.Download.Clients.Aria2
{
    public class Aria2Version
    {
        [XmlRpcMember("version")]
        public string Version;

        [XmlRpcMember("enabledFeatures")]
        public string[] EnabledFeatures;
    }

    public class Aria2Uri
    {
        [XmlRpcMember("status")]
        public string Status;

        [XmlRpcMember("uri")]
        public string Uri;
    }

    public class Aria2File
    {
        [XmlRpcMember("index")]
        public string Index;

        [XmlRpcMember("length")]
        public string Length;

        [XmlRpcMember("completedLength")]
        public string CompletedLength;

        [XmlRpcMember("path")]
        public string Path;

        [XmlRpcMember("selected")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string Selected;

        [XmlRpcMember("uris")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public Aria2Uri[] Uris;
    }

    public class Aria2Status
    {
        [XmlRpcMember("bittorrent")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public XmlRpcStruct Bittorrent;

        [XmlRpcMember("bitfield")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string Bitfield;

        [XmlRpcMember("infoHash")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string InfoHash;

        [XmlRpcMember("completedLength")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string CompletedLength;

        [XmlRpcMember("connections")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string Connections;

        [XmlRpcMember("dir")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string Dir;

        [XmlRpcMember("downloadSpeed")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string DownloadSpeed;

        [XmlRpcMember("files")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public Aria2File[] Files;

        [XmlRpcMember("gid")]
        public string Gid;

        [XmlRpcMember("numPieces")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string NumPieces;

        [XmlRpcMember("pieceLength")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string PieceLength;

        [XmlRpcMember("status")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string Status;

        [XmlRpcMember("totalLength")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string TotalLength;

        [XmlRpcMember("uploadLength")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string UploadLength;

        [XmlRpcMember("uploadSpeed")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string UploadSpeed;

        [XmlRpcMember("errorMessage")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string ErrorMessage;
    }
}
