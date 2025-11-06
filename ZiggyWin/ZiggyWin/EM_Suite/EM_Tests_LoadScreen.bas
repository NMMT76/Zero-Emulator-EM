'Not so much LoadScreen, but more so FREAD (file read) into video memory
'Only works with .scr files for obvious reasons. An unroll of size 32 works
'well, but you can go as high as you're willing to "sacrifice" RAM

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

Sub Test_LoadScreen()
	dim offset,filelength as ULong
	EMRegisterAction(10,"FREAD")
	WriteStringToEM("E:\\Dev\\Data\\tzerra_TheStarryNight_2015.scr")
	EMRunCommand(10)
	'We must read the length even though we dont use it
	filelength=ReadULongFromEM()
	for offset=0 TO 6911 step 256
		EM_To_VRAM_32(offset)
		EM_To_VRAM_32(offset+32)
		EM_To_VRAM_32(offset+64)
		EM_To_VRAM_32(offset+96)
		EM_To_VRAM_32(offset+128)
		EM_To_VRAM_32(offset+160)
		EM_To_VRAM_32(offset+192)
		EM_To_VRAM_32(offset+224)
	next
	Pause(500)
	Print "FileLength : ";str(filelength);
end sub

CLS
Test_LoadScreen()


