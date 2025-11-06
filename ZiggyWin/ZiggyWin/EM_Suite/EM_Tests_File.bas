#include <C:\Users\NT\Desktop\zxbasic\EM_Base.bas>

sub EM_To_VRAM_32(offset as UInteger)
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

Sub Test_IMAGELOADBWDMA()
	EMRegisterAction(10,"IMAGELOADBWDMA")
	'WriteStringToEM("https://www.thermofisher.com/blog/food/wp-content/uploads/sites/5/2022/06/iStock-1369307158_candy.jpg")
	WriteStringToEM("E:\\Dev\\Data\\Images\\ZXSpectrum48k.jpg")
	EMRunCommand(10)
	'For offset=0 TO 6144-1
	'	POKE offset+16384,IN 63
	'NEXT offset
end sub

Sub Test_IMAGELOADBWPIO()
	DIM offset as ULong
	EMRegisterAction(10,"IMAGELOADBWPIO")
	'WriteStringToEM("https://www.thermofisher.com/blog/food/wp-content/uploads/sites/5/2022/06/iStock-1369307158_candy.jpg")
	WriteStringToEM("E:\\Dev\\Data\\Images\\ZXSpectrum48k.jpg")
	EMRunCommand(10)
	For offset=0 TO 6144-1
		POKE offset+16384,IN 63
	NEXT offset
end sub

Sub Test_PLAYAUDIOSYNC()
	EMRegisterAction(10,"PLAYAUDIOSYNC")
	WriteStringToEM("E:\\Dev\\Data\\MP3\\80sw.mp3")
	EMRunCommand(10)
end sub

Sub Test_PLAYAUDIOSYNCNOWAIT()
	EMRegisterAction(10,"PLAYAUDIOSYNC")
	WriteStringToEM("E:\\Dev\\Data\\MP3\\80sw.mp3")
	EMRunCommandNoWait(10)
end sub

Sub Test_CAPTUREIMAGEBWPIO(ditherer as Ubyte)
	DIM offset as UInteger
	EMRegisterAction(10,"CAPTUREIMAGEBWPIO")
	'Push desired ditherer to EM
	OUT 63,ditherer
	EMRunCommand(10)
	For offset=0 TO 6144-1 STEP 96
		EM_To_VRAM_32(offset)
		offset=offset+32
		EM_To_VRAM_32(offset)
		offset=offset+32
		EM_To_VRAM_32(offset)
		offset=offset+32
	NEXT offset
end sub

Sub Test_CAPTUREIMAGEBWDMA(ditherer as Ubyte)
	EMRegisterAction(10,"CAPTUREIMAGEBWDMA")
	'Push desired ditherer to EM
	OUT 63,ditherer
	EMRunCommand(10)
end sub

'DIM dith as Ubyte
'DO
'	Test_CAPTUREIMAGEBW(0)
'LOOP

'Test_IMAGELOADBWPIO()
'Pause(200)
'CLS
'Test_IMAGELOADBWDMA()

'Test_PLAYAUDIOSYNC()

'DO
'	Test_CAPTUREIMAGEBWPIO(0)
'	PRINT AT 0,0;"ZX CAM"
'	PRINT AT 0,31;"X"
'	PAUSE(200)
'LOOP

CLS
Test_PLAYAUDIOSYNCNOWAIT()
DIM counter as ULong
counter=0
DO
	Print AT 0,0;"Playing (";Str(counter);")"
	counter=counter+1
LOOP WHILE IN 191>0
Print AT 0,0;"Playing ended (";Str(counter);")"

