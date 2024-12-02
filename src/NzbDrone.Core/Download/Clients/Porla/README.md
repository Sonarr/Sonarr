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

Preset ussage are being set on the torrent adds, through a client setting.

So I needed to do something with the `save_path` on the `torrent.add` request.
Theis field is optional if you set a defualt path on porla BUT required if you didn't.  
I wanted this to work "by default", so if you have NO presets set, you **NEED** to set the `save_path` _BUT_ if you want to use the presets values you should NOT set the `save_path` in the RPC request. That is why you see that intersting branch on the `AddTorrent` functions.

Interesting handling, the chosen preset is **overlayed** over the values from the _default_ preset if it exists.
We need to deal with that, to determine the "effective" settings loaded inside porla

### Tags

Would be great to use to filter BIG lists of torrents even more! But Sonarr expects everything. Something to look at in the future if Sonarr ever does granular torrent requests.

The idea is to use the tags on torrents to make listing more efficient, in our case tagging them with series/show/season so we can find what we are looking for.

What I did set tags with the series attributes. I want to be able to come around later and filter torrents outside the context of Sonarr.

You can set a client setting to disable this behavior.

### Torrent Metadata

I **DID NOT USE** this, maybe add something?

It's basically a any-field that we can fill with anything we want.

### LibTorrent [Torrent Flags](https://libtorrent.org/single-page-ref.html#torrent_flags_t)

#### share_mode

Should be very useful for private trackers. Sadly unclear how to implement it without doing some spagetti.

# Data / Response Examples

Config Definition can be found at [`config.hpp`](https://github.com/porla/porla/blob/v0.37.0/src/config.hpp)

## Presets.list

Return should be defined in [`presets.hpp`](https://github.com/porla/porla/blob/v0.37.0/src/lua/packages/presets.cpp)

### Config.toml
```toml
[presets.default]
save_path = "/tmp/"

[presets.other]
tags = ["other"]
```

### Response
```json
{
"default":{
    "category":null,"download_limit":null,"max_connections":null,"max_uploads":null,"save_path":"/tmp/","session":null,"tags":null,"upload_limit":null
    },
"other":{
    "category":null,"download_limit":null,"max_connections":null,"max_uploads":null,"save_path":null,"session":null,"tags":["other"],"upload_limit":null
    }
}
```
