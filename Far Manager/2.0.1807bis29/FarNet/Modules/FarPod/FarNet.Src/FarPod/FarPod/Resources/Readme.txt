
Plugin   : FarPod
Version  : 0.0.1
Release  : 2011-06-21
Category : File Panel
Author   : Sergii Tkachenko
E-mail   : joye.ramone@gmail.com
Source   : <not available yet>


= DESCRIPTION =


FarPod allows manage IPod devices.

 - Add, remove music from your iPod
 - Add, remove and edit playlists
 - Copy, move music beetwen playlists
 - Copy music, playlists from your iPod to PC
 - Tag editing
 - etc...

= PREREQUISITES =


 - .NET Framework 3.5
 - Plugin FarNet 4.4.18
 - Far Manager 2.0.1807
 - SharePodLib 3.9.7 (27 Nov 2010) http://www.getsharepod.com/fordevelopers/
 - taglib-sharp 2.0.3.7 


= INSTALLATION =


1. Unpack archive to %FARHOME%

2. Download http://www.getsharepod.com/download/SharePodLib.zip, 
   unpack SharePodLib.dll file to %FARHOME%\FarNet\Modules\FarPod\

3. Download http://download.banshee.fm/taglib-sharp/2.0.3.7/taglib-sharp-2.0.3.7-windows.zip, 
   unpack taglib-sharp.dll file to %FARHOME%\FarNet\Modules\FarPod\


= SETTINGS =


Open the module settings panel from the main .NET menu:
F11 | .NET | Settings | FarPod

Setting description:

	PlayListNameFormat - playlist display format. 
		Default={0}

	NewPlayListName - default name for new playlist dialog. 
		Default=NewPlayList

	SupportFileMask - support file mask. 
		Default=*.mp3;*.m4a;*.m4v;*.wav;*.mp4;*.aac;*.m4b;*.aif;*.afc;*.m4r

	SupportArtWorkMask - support artwork mask. 
		Default=folder.jpg;cover.jpg;front.*jpg

	TrackNameFormatForPanel - track display format. 
		Default=[Artist]-[Album] - [TrackNumber] [Title]

	TrackNameFormatForCopy - track name format for copy/move tracks dialog.
		Default=[Artist]-[Year]-[Album]\\[TrackNumber]-[Title]

Support tags:
	[Title]
	[Artist]
	[Album]
	[TrackNumber]
	[Genre]
	[Composer]
	[AlbumArtist]
	[DiscNumber]
	[Year]


= HISTORY =

	0.0.2 2011-06-22
		- Fix issue with move files to device
		- Fix issue in dialog display

	0.0.1 2011-06-21
		- Initial release

