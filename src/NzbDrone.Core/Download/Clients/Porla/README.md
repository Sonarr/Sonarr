# Porla Client

[Porla](https://porla.org/)! The (Unofficial) Web frontend for [LibTorrent](https://libtorrent.org/)!

## Details

* Uses JSONRPC
* Written in C++
* Has LUA Plugin Capability
* Session Capable
* Has Proxy Support (Looking at you Transmission!)
* Presets!

> Currently (2024-02) still in bleeding-edge active development.

## Deving

I coppied large parts of this code from the Hadouken Client, as it was the other JSONRPC client in the collection.

Docs are iffy currently, check out the methods at the [source (v0.37.0)](https://github.com/porla/porla/tree/v0.37.0/src/methods). They are not too hard to figure out in conjunction with the [method docs](https://porla.org/api/methods/)

### `GetStatus()`

Unsure what this needs to do exactly.  
I _think_ it's supposed to introspect the configuration.  
_IF_ that is the case. TODO: Figure out extracting the `save_path` from the prefix 

## Feature Implementation

### Sessions

I **DID NOT** do anything with sessions.

They could be VERY powerfull to work with Private and Public trackers, but the front end currently doesn't have strong support for it.

Could also be a way to have multiple Sonarrs pointed at the same Porla, or a diffrent way to deal with the other Starr ecosystem.

### Presets

Presets are being set on the torrent adds

So I needed to do something with the `save_path`.  
I wanted this to work "by default", so if you have NO presets set, you **NEED** to set the `save_path` _BUT_ if you want to use the presets values you shoule NOT set the `save_path` in the RPC request. That is why you see that intersting branch on the `AddTorrent` functions.

### Tags

Would be great to use to filter BIG lists of torrents even more! But it doesn't look like Sonarr expects that. looks like it expects everything

The idea is to use the tags to find a spesific series/show/season so listing is effiecient.

So what I wanted to do instead is set tags for the series. I want to be able to come around later and filter torrents seperated from Sonarr.

TODO: Should probably create a flag to turn this off.

### Torrent Metadata

I **DID NOT USE** this, maybe add something?

### LibTorrent [Torrent Flags](https://libtorrent.org/single-page-ref.html#torrent_flags_t)

#### share_mode

Should be very useful for private trackers. Sadly unclear how to implement it without stepping on toes.
