#### Changelog:

`v0.4.0` -> `v0.5.0`

```diff
+ Spectator mode, You can now watch your trombuddies play live with ultra low latency!
+ In game SongDownloads. Download your favorite charts from the toottally setting page MoreSongs
+ We will add more search filters in the future as well as making this accessible directly in the Level Selection Screen.
+ Added Rated Icon next to the leaderboard for rated charts
+ Fixed TootTally login page requiring the user to press enter after each input
+ Added minimal password restrictions to login page (must be more than 5 characters and not contain your username)
+ Improved scrolling on leaderboard and setting pages
+ Fixed several edge case bugs with the leaderboard and trombuddies panel
+ Added option to remove 'Cool S'
```

-----

`v0.3.4` -> `v0.4.0`

```diff
+ TootTallySettings: a replacement for TrombSettings for TootTally modules
+ TromBuddies: TootTally's new friend and status system
+ Profile session popup on the bottom right of the song select screen to show your current session's TT gains
+ Point Screen now shows you your current score's TT and position
+ Fixed replay bugs for non-16:9 resolutions. v0.4.0 replays cannot be viewed correctly on v0.3.4 or under.
+ (Actually) Fixed crashing issue related to TootTallyLogger and Themes
+ Added button to reload current themes in the Themes folder
+ New FC and SSS graphic for the point screen
+ Minor optimizations and reworks
```

-----

`v0.3.3` -> `v0.3.4`

```diff
+ (Hopefully) fixed crashing issue related to TootTallyLogger and Themes
+ Fixed Replay Timestamp slider
+ Fixed text position for replay speed slider
+ Updated replay system to work with smooth scrolling
+ More very minor bug fixes
```


`v0.3.2` -> `v0.3.3`

```diff
+ Updated all text elements to TextMeshPro
+ Implemented TootTally's own logging system. The new log files can be found in Bepinex/Logs.
+ Added a profile popup in the LevelSelectScreen with the current session's tt gains.
+ Fixed a rare bug where GameSpeed wouldn't apply to some songs.
+ Reworked the way Replays handles speed.
+ Partially fixed themes for the random button and the SongTitle's outline
+ Updated Profile Popup visual
+ Added a new submitted play Popup in the PointScene
+ Added Pride Theme Preset
+ Fixed compatibility with latest update
```

`v0.3.1` -> `v0.3.2`

```diff
+ Replaced Turbo and Practice buttons for a Game Speed Slider
+ Added Game Speed next to the score in leaderboards
+ Fixed Notification not scaling properly on 16:10 and 4:3
+ Added a new Debug Mode option that enables extra logging info
+ Fixed compatibility with latest update
```

`v0.3.0` -> `v0.3.1`

```diff
+ Fixed issues with April Fools' 2023 update
```

`v0.2.9` -> `v0.3.0`

```diff
+ Added a new Login Screen to TootTally account creation and first time login.
+ Added modules:
-- Modules are plugins you can installed that TootTally will manage. Modules can be enabled / Disabled without having to restart your game and can be updated individually via ThunderStore without having to update the entire core mod. This is an extremely flexible feature and we are excited to reveal some useful modules we've been working on.
+ Migrated to TrombLoaderV2 and BaboonAPI compatiblity
+ Circular Breathing compatibility, disabling the mod using TrombSettings is good enough to let it submit scores.
+ Improved Replay frame interpolation
+ Added Replay Timestamp slider
+ Global notification system: TootTally admins can now send messages to user or broadcast messages to active users.

- Fixed notification scaling incorrectly
- Fixed pathing for mac users
- Several Other minor fixes
```

-----

`v0.2.8` -> `v0.2.9`

```diff
+ Hotfixed official charts not submitting properly
```

`v0.2.7` -> `v0.2.8`

```diff
+ Access any point in the replay! A new scroll bar is in the bottom of the play area that allows you to scrub through a replay.
+ Fixed displayed star rating being off by 1 when above 6 stars.
+ Fixed star rating crash when map is above 10 stars
+ Fixed replays lagging sometimes
+ Fixed pathing for MacOS
+ Fixed tt being displayed on unrated charts
- Scores made with the Circular Breathing mod are now no longer sent over to the server. This may change in the future, so please tell us now if you want us to revert this change.
```

`v0.2.4` -> `v0.2.7`

```diff
+ Fixed issue with r2modman not respecting the folder structure in the package
```

`v0.2.1` -> `v0.2.4`

```diff
+ TootTally Themes! These themes allow you to change the look of your song select screen in myriads of ways! The mod will come with custom themes made by our beta testers.
+ TootTally Rating and Leaderboards System! Some charts are now "rated" and will give you a rating for plays set on them. You can view the global leaderboards at https://toottally.com/leaderboards! Expect these values to fluctuate for a bit as we continually fix and finetune the algorithm to be much closer to expected values.
+ Discord Rich Presence! Flex your rankings on the global leaderboards on your Discord server or friends! Show what songs you've been playing! (Wait that might actually not be a good idea oh god)
+ Slightly revamped Replay UI! Speed of replays can now be adjusted via a slider!
+ Upped the sample rate for replays to 120Hz. This should make replays more accurate compared to previous versions.
+ Fixed off by one error on replay summaries.
+ Fixed freezing at the end of a very long track due to replay saving.
- We now have a Privacy Policy (https://toottally.com/privacy). Please take the time to read through it if you can.
```

`v0.2.1` -> `v0.2.3`

There are no changes to the mod itself. We only updated the dependencies and metadata on Thunderstore.

`v0.2.0` -> `v0.2.1`

```diff
+ Notification toasts!
+ Fixed some bugs related to replays and charts not being sent over properly.
+ Fixed rare cases where replays think they're in Australia (rare case where a replay is recorded upside down).
+ Fixed crash caused by leaving the Home Screen too quickly after booting the game
+ Steam Leaderboards can now be accessed from the point screen. This is merely a hotfix, and we'll be pushing out a more proper fix in the future.
+ Additional replay optimization, they should be slightly smaller now compared to previous replays
```
