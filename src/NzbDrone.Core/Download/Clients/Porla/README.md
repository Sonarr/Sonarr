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


## Feature Implementation

### Sessions

I **DID NOT** do anything with sessions.

They could be VERY powerfull to work with Private and Public trackers, but the front end currently doesn't have strong support for it.

Could also be a way to have multiple Sonarrs pointed at the same Porla, or a diffrent way to deal with the other Starr ecosystem.

### Presets

I MIGHT have done something with them...

### Tags

Would be great to use to filter BIG lists of torrents even more! But it doesn't look like Sonarr expects that. looks like it expects everything

The idea is to use the tags to find a spesific series/show/season so listing is effiecient.

### Torrent Metadata

I **DID NOT USE** this, maybe add something?

### LibTorrent [Torrent Flags](https://libtorrent.org/single-page-ref.html#torrent_flags_t)

#### share_mode

Should be very useful for private trackers. Sadly unclear how to implement it without stepping on toes.
