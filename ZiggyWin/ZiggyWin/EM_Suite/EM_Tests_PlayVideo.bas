'Demo of the PLAYVIDEOFILEBW method, change file path to reflect your actual
'file/path
'WARNING: do NOT use large video files as the method creates ALL the video data
'in RAM, not on a "per frame requested" fashion
'Obviously has many issues, and even with a large frameskip 'value it simply
'can't keep up. This would be the poster child for when PIO will fail miserably
'compared to DMA, its just too much data to transfer consistently per unit of
'time
'One could mitigate it by not transfering a full screen worth of data, just a
'some smaller area. Full screen is 6K@50fps = 300KB/s which is far beyond what
'PIO can do, quarter screen is 1.5k@50fps = 75KB/s which is just about doable
'with a massive unroll (256-512). If you go quarter screen AND 25fps, you are
'down to 37.5KB/s which you can comfortably do with a minimal unroll (32-64)
'Feel free to write a method that takes an arbitrary target size and fps ;)


#include <C:\Users\NT\Desktop\zxbasic\EM_Base.bas>

sub EM_To_VRAM_32(offset as ULong)
	POKE 16384+offset,IN 63
	POKE 16385+offset,IN 63
	POKE 16386+offset,IN 63
	POKE 16387+offset,IN 63
	POKE 16388+offset,IN 63
	POKE 16389+offset,IN 63
	POKE 16390+offset,IN 63
	POKE 16391+offset,IN 63
	POKE 16392+offset,IN 63
	POKE 16393+offset,IN 63
	POKE 16394+offset,IN 63
	POKE 16395+offset,IN 63
	POKE 16396+offset,IN 63
	POKE 16397+offset,IN 63
	POKE 16398+offset,IN 63
	POKE 16399+offset,IN 63
	POKE 16400+offset,IN 63
	POKE 16401+offset,IN 63
	POKE 16402+offset,IN 63
	POKE 16403+offset,IN 63
	POKE 16404+offset,IN 63
	POKE 16405+offset,IN 63
	POKE 16406+offset,IN 63
	POKE 16407+offset,IN 63
	POKE 16408+offset,IN 63
	POKE 16409+offset,IN 63
	POKE 16410+offset,IN 63
	POKE 16411+offset,IN 63
	POKE 16412+offset,IN 63
	POKE 16413+offset,IN 63
	POKE 16414+offset,IN 63
	POKE 16415+offset,IN 63
end sub

Sub Test_PlayVideo()
	dim framecount,framecounter,offset as ULong
	EMRegisterAction(10,"PLAYVIDEOFILEBW")
	WriteStringToEM("E:\\Dev\\Data\\Touhou-Bad-Apple.mp4")
	OUT 63,0 'Quant mode 0, Bayer2x2
	OUT 63,4 'Frameskip 2
	EMRunCommand(10)
	framecount=ReadULongFromEM()
	print "Video frame count : ";STR(framecount)
	pause(250)
	for framecounter=0 TO framecount-1
		for offset=0 TO 6143 step 512
			EM_To_VRAM_32(offset)
			EM_To_VRAM_32(offset+32)
			EM_To_VRAM_32(offset+64)
			EM_To_VRAM_32(offset+96)
			EM_To_VRAM_32(offset+128)
			EM_To_VRAM_32(offset+160)
			EM_To_VRAM_32(offset+192)
			EM_To_VRAM_32(offset+224)
			
			EM_To_VRAM_32(offset+256)
			EM_To_VRAM_32(offset+256+32)
			EM_To_VRAM_32(offset+256+64)
			EM_To_VRAM_32(offset+256+96)
			EM_To_VRAM_32(offset+256+128)
			EM_To_VRAM_32(offset+256+160)
			EM_To_VRAM_32(offset+256+192)
			EM_To_VRAM_32(offset+256+224)
		next
	next
end sub

CLS
Test_PlayVideo()

