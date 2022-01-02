# WMC-JsonConsoleWrapper
A (very) janky wrapper over the WMC Api.

All this does is spit out json over stdout, giving you the ability to parse and utilize the WMC api in any project with the smallest amount of effort possible.  
![What I'm talking about](https://i.imgur.com/HIxqS3G.png)

Example (song change):
```json
{
  "Type":"PROPERTY_CHANGE",
  "SessionId":"chrome.exe",
  "Data":{
    "AlbumArtist":"",
    "AlbumTitle":"",
    "AlbumTrackCount":0,
    "Artist":"Eminem",
    "Genres":[],
    "PlaybackType":"Music",
    "Subtitle":"",
    "Thumbnail":" ... base64 string ...",
    "ThumbnailType":"image/jpeg",
    "Title":"Infinite",
    "TrackNumber":0
  }
}
```
