# Tales-of-Symphonia-NGC-Undub
Hello all! This is the new home for the Tales of Symphonia GCN Undub patch! Many years ago I created the initial Tales of Symphonia undub patch. There were various issues with it (documented below) with the most glaring one being the omission of restoring skit voices. Alas, I was but a young college student at the time, and I did not yet have the tech know-how to complete this. (There were also a lot less resources available back then!)

Earlier this year, I took reverse engineering on as a hobby, and a few months ago I decided that I now knew enough to come back and finish this project. I'm happy to say that I've finally accomplished what I originally set out to do all those years ago. All voices are now properly restored, and all known bugs have been fixed. Enjoy!

# Releases
For releases, please see the release page.

Current Version: 2.0 BETA

# Bug Reporting
If you run into any bugs please open an issue on github. Include as many details as you can, and if possible, please include a save file. You may also try to reach me in various other places where you may find me. Thanks!!

# Version History, Information, and Documentation

## Version 1.0 - OLD - Informational Only
I've lost a lot of the WIP intermediary files so this is mostly going on memory. At the time of this patch's creation, editing GCN games was under the limitation that files could not be replaced with larger files. All the afs files were extracted, ahx's were converted to wav's, and to get the JP files to fit within the NA file limit, they were downsampled about 20% and repacked. I honestly do not remember how or what I did with the playable character .snd files (includes random grunts + sometimes actual dialog), it's possible they weren't actually replaced?

Bugs/Issues:
- No Voiced Skits (despite nulled out audio being replaced with original audio files)
- "Grunts" + dialog for human-type enemies not undubbed
- Issue where one of Presea's lines (the "tutururu" sound) would play on top of one of those grunts - likely others as well
- Possibly/likely other stuff

## Version 2.0 BETA
Quite a few changes, most of them tackling bugs or issues with the old version. All known bugs have been resolved.

- No more downsampled files. GCRebuilder v1.1 by BSV is now used to repackage files, and the file size constraint is no longer there. Audiophiles rejoice!

- Wrong voice bug. The ID of voice data was shifted down by 1 in the NA release. I'm not 100% sure on the exact cause of this one. I think they are caused by the playable characters having their ID's restored to their JP ID's, shifting it up by one, and enemies were still using the NA ID's. The footsoldier enemy (the two soldiers before the first mini boss at the start of the game,) for example, has ID's that begin right after Presea's. When he tries to play his first voice (basic attack), since it shared an ID with the JP up-shifted last of Presea's voices, they both played. Like I said, not 100% sure, but I believe this to be the case. Not sure if there are other PC's that share a bound with a voiced NPC.

This has been fixed by adjusting the enemy voice ID's to the up-shifted JP ID's. Which brings us to...

- Enemy dialog restored! This was probably the hardest thing to accomplish. If you've ever explored the file contents of this game, you may have noticed that there are a lot of individual .snd files in the S/ folder, from v_10_bh04.snd to v_67_hum25.snd, and so on. This is actually the enemy voice data. But these files are unused. I think whoever built the game left them in the filesystem by mistake. Through a lot of debugging and exploration, I finally discovered that the actual sound data, without headers, is actually inside btlvbank.dat. But the header is where the ID's are. Through even more painful debugging, I finally found the headers were inside of the btlenemy.dat entries, but those entries were compressed. I found the decompression routine in the game code, and directly copied the decompiled code from Ghidra into a C# project. I managed to successfully decompress one of the entries, and lo and behold, the header data! The only problem now is having to recompress the data after modifying it. I started analyzing the compression algorithm some more, attempted to piece together what it was doing, add comments + adjust variables, etc., when I noticed the compression looked similar to LZSS. (I haven't worked with compression before, so this was new!) Speaking with colleagues, I was pointed in the direction of compto. Went to take a look at it, and thankfully, that was indeed the compression being used! 

With this new piece of knowledge in hand, I had everything needed to accomplish this. Since the Japanese btlvbank voices were also different sizes, I had to rebuild that as well. I opted to use the vanilla Japanese btlvbank file, and instead just replaced the entire voice portion of the btlenemy entries with the Japanese one. There is one last file, btlusual, which has two relevant pointer tables at the end - one for btlenemy entries, and one for btlvbank. The pointer table for vbank was straight up replaced w/ the Japanese one, while the btlenemy one was rebuilt from scratch based on how my script rebuilt the file. Check github repo for code used to rebuild this.

Doing all of these changes fixes the overlapping ID issue, as everything should be using the Japanese ID now, and also restores the enemy voices that were not undubbed in the previous version.

- And lastly... Skit voices have been restored! This took two days of debugging and documenting code, and after going through the entire routine that loads the voice files and picks and loads the correct one, not seeing any differences between the JP and NA versions of the game, I finally stepped out to the function that called it, and right there, was an extra function call that was not present in the JP version. A quick peek inside it shows that it looks like a function that edits the volume. So after all this time, the problem was that they decided that nulling out the audio file wasn't enough, they had to go and mute it, too! NOPing out the bl instruction effectively fixes this.

![image](https://user-images.githubusercontent.com/6155506/134822118-a568d578-94e1-4b00-a0c8-5a40b4e53fc2.png)

I also added a little logo during boot. I hope it's not too intrusive!

# To-do for future releases:

1. Sub the ending video
2. Look into the viability of merging both discs into one and removing the disc swap
3. Restore sound test menu option that was removed from the NA release
4. Add subtitles to battle voices

I've spent a lot of time on this and I'm tired of it. :) The only item on the list I'll probably do is the first one. I'll see if anyone unearths any bugs or issues in the next few months. Hopefully I can get that done and just include it in the next update, either just removing the beta tag if no issues, or with bug fixes for any issues identified.

Thanks all. <3
Julian
