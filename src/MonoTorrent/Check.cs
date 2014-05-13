using System;

namespace MonoTorrent
{
    public static class Check
    {
        static void DoCheck(object toCheck, string name)
        {
            if (toCheck == null)
                throw new ArgumentNullException(name);
        }

        static void IsNullOrEmpty(string toCheck, string name)
        {
            DoCheck(toCheck, name);
            if (toCheck.Length == 0)
                throw new ArgumentException("Cannot be empty", name);
        }

        public static void Address(object address)
        {
            DoCheck(address, "address");
        }

        public static void AddressRange(object addressRange)
        {
            DoCheck(addressRange, "addressRange");
        }

        public static void AddressRanges(object addressRanges)
        {
            DoCheck(addressRanges, "addressRanges");
        }

        public static void Announces(object announces)
        {
            DoCheck(announces, "announces");
        }

        public static void BaseDirectory(object baseDirectory)
        {
            DoCheck(baseDirectory, "baseDirectory");
        }

        internal static void BaseType(Type baseType)
        {
            DoCheck(baseType, "baseType");
        }

        internal static void Buffer(object buffer)
        {
            DoCheck(buffer, "buffer");
        }

        internal static void Cache(object cache)
        {
            DoCheck(cache, "cache");
        }

        public static void Data(object data)
        {
            DoCheck(data, "data");
        }

        public static void Destination (object destination)
        {
            DoCheck (destination, "destination");
        }

        public static void Endpoint(object endpoint)
        {
            DoCheck(endpoint, "endpoint");
        }

        public static void File(object file)
        {
            DoCheck(file, "file");
        }

        public static void Files(object files)
        {
            DoCheck(files, "files");
        }

        public static void FileSource(object fileSource)
        {
            DoCheck(fileSource, "fileSource");
        }

        public static void InfoHash(object infoHash)
        {
            DoCheck(infoHash, "infoHash");
        }

        public static void Key (object key)
        {
            DoCheck (key, "key");
        }

        public static void Limiter(object limiter)
        {
            DoCheck(limiter, "limiter");
        }

        public static void Listener(object listener)
        {
            DoCheck(listener, "listener");
        }

        public static void Location(object location)
        {
            DoCheck(location, "location");
        }

        public static void MagnetLink(object magnetLink)
        {
            DoCheck(magnetLink, "magnetLink");
        }

        public static void Manager(object manager)
        {
            DoCheck(manager, "manager");
        }

        public static void Mappings (object mappings)
        {
            DoCheck (mappings, "mappings");
        }

        public static void Metadata(object metadata)
        {
            DoCheck(metadata, "metadata");
        }

        public static void Name (object name)
        {
            DoCheck (name, "name");
        }

        public static void Path(object path)
        {
            DoCheck(path, "path");
        }

        public static void Paths (object paths)
        {
            DoCheck (paths, "paths");
        }

        public static void PathNotEmpty(string path)
        {
            IsNullOrEmpty(path, "path");
        }

        public static void Peer (object peer)
        {
            DoCheck (peer, "peer");
        }

        public static void Peers (object peers)
        {
            DoCheck (peers, "peers");
        }

        public static void Picker(object picker)
        {
            DoCheck(picker, "picker");
        }

        public static void Result(object result)
        {
            DoCheck(result, "result");
        }

        public static void SavePath(object savePath)
        {
            DoCheck(savePath, "savePath");
        }

        public static void Settings(object settings)
        {
            DoCheck(settings, "settings");
        }

        internal static void SpecificType(Type specificType)
        {
            DoCheck(specificType, "specificType");
        }

        public static void Stream(object stream)
        {
            DoCheck(stream, "stream");
        }

        public static void Torrent(object torrent)
        {
            DoCheck(torrent, "torrent");
        }

        public static void TorrentInformation(object torrentInformation)
        {
            DoCheck(torrentInformation, "torrentInformation");
        }

        public static void TorrentSave(object torrentSave)
        {
            DoCheck(torrentSave, "torrentSave");
        }

        public static void Tracker(object tracker)
        {
            DoCheck(tracker, "tracker");
        }

        public static void Url(object url)
        {
            DoCheck(url, "url");
        }

        public static void Uri(Uri uri)
        {
            DoCheck(uri, "uri");
        }

        public static void Value(object value)
        {
            DoCheck(value, "value");
        }

        public static void Writer(object writer)
        {
            DoCheck(writer, "writer");
        }
    }
}