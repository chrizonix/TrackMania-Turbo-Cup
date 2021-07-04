# TrackMania Turbo Scoreboard Recorder and Upload Tool
[![GitHub Release](https://img.shields.io/github/release/chrizonix/TrackMania-Turbo-Recorder.svg)](https://github.com/chrizonix/TrackMania-Turbo-Recorder/releases/tag/v1.0.0)
[![Github Releases (by Release)](https://img.shields.io/github/downloads/chrizonix/TrackMania-Turbo-Recorder/v1.0.0/total.svg)](https://github.com/chrizonix/TrackMania-Turbo-Recorder/releases/tag/v1.0.0)
[![Github Commits (since latest release)](https://img.shields.io/github/commits-since/chrizonix/TrackMania-Turbo-Recorder/latest.svg)](https://github.com/chrizonix/TrackMania-Turbo-Recorder/compare/v1.0.0...master)
[![GitHub Repo Size in Bytes](https://img.shields.io/github/repo-size/chrizonix/TrackMania-Turbo-Recorder.svg)](https://github.com/chrizonix/TrackMania-Turbo-Recorder)
[![Github License](https://img.shields.io/github/license/chrizonix/TrackMania-Turbo-Recorder.svg)](LICENSE.md)

## Install / Usage
* Extract the `TurboCup.zip` Package to any Location (e.g. Desktop)
* Install OpenPlanet for TrackMania Turbo ([openplanet.nl/download](https://openplanet.nl/download))
* Launch the TurboCup Recorder App (`TurboCup.exe`)
* Click the `Start` Button
  * Launch the Game with the In-Game Scoreboard Recorder Plugin ([chrizonix/OpenPlanet-Plugins](https://github.com/chrizonix/OpenPlanet-Plugins))
  * Launch the Plugin via `F3 -> Scripts -> Turbo Cup -> Control Panel -> Start`
  * Host the Game and Play with your Friends
* Click the `Stop` Button
* Upload the Player Scores (`Upload-YYYY-MM-DD.csv`) to your Personal Server (Optional)
  * **Important:** Change the Upload URL and Login Credentials in [Uploader.cs#L27](https://github.com/chrizonix/TrackMania-Turbo-Recorder/blob/fe151c5a9f0585e2ebe47be83205fb018f692f10/TurboCup/Uploader.cs#L27)

## Why?
TrackMania Turbo offers no Dedicated Server, so Scripting for a Racing Event has to be done on client-side.
This Tool in combination with the In-Game Recorder Plugin, enables a Race Organizer to Record the Scoreboard of every Player in the Current Session.

## Additional Credits
* [Terminal Application Icon by CB2K, Wikimedia Commons](https://commons.wikimedia.org/wiki/Category:Black_Mac_Style_Icons#/media/File:Dosemu_Mac.png)
