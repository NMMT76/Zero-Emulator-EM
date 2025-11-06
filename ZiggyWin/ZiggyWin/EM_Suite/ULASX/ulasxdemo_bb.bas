'ULASX DATAPORT
DIM outdata as UInteger
outdata=48955
'ULASX COMMAND PORT
DIM outcommand as UInteger
outcommand=65339

Sub Solid0Fill()
	'100% INK
	POKE USR "a" + 0, 0
	POKE USR "a" + 1, 0
	POKE USR "a" + 2, 0
	POKE USR "a" + 3, 0
	POKE USR "a" + 4, 0
	POKE USR "a" + 5, 0
	POKE USR "a" + 6, 0
	POKE USR "a" + 7, 0
end sub
Sub Solid100Fill()
	'100% INK
	POKE USR "a" + 0, 255
	POKE USR "a" + 1, 255
	POKE USR "a" + 2, 255
	POKE USR "a" + 3, 255
	POKE USR "a" + 4, 255
	POKE USR "a" + 5, 255
	POKE USR "a" + 6, 255
	POKE USR "a" + 7, 255
end sub
Sub Solid50LTFill()
	'50% Lower Triangle INK
	POKE USR "a" + 0, 1
	POKE USR "a" + 1, 3
	POKE USR "a" + 2, 7
	POKE USR "a" + 3, 15
	POKE USR "a" + 4, 31
	POKE USR "a" + 5, 63
	POKE USR "a" + 6, 127
	POKE USR "a" + 7, 255
end sub
Sub Solid50UTFill()
	'50% Upper Triangle INK
	POKE USR "a" + 7, 255
	POKE USR "a" + 6, 127
	POKE USR "a" + 5, 63
	POKE USR "a" + 4, 31
	POKE USR "a" + 3, 15
	POKE USR "a" + 2, 7
	POKE USR "a" + 1, 3
	POKE USR "a" + 0, 1
end sub
Sub OrderedDither25Fill()
	'50% OrderedDither INK
	POKE USR "a" + 0, 136
	POKE USR "a" + 1, 34
	POKE USR "a" + 2, 136
	POKE USR "a" + 3, 34
	POKE USR "a" + 4, 136
	POKE USR "a" + 5, 34
	POKE USR "a" + 6, 136
	POKE USR "a" + 7, 34
end sub
Sub OrderedDither50Fill()
	'50% OrderedDither INK
	POKE USR "a" + 0, 170
	POKE USR "a" + 1, 85
	POKE USR "a" + 2, 170
	POKE USR "a" + 3, 85
	POKE USR "a" + 4, 170
	POKE USR "a" + 5, 85
	POKE USR "a" + 6, 170
	POKE USR "a" + 7, 85
end sub
Sub OrderedDither75Fill()
	'75% OrderedDither INK
	POKE USR "a" + 0, 119
	POKE USR "a" + 1, 255
	POKE USR "a" + 2, 221
	POKE USR "a" + 3, 255
	POKE USR "a" + 4, 119
	POKE USR "a" + 5, 255
	POKE USR "a" + 6, 221
	POKE USR "a" + 7, 255
end sub

sub ULASXSetPalleteGrey32()
	'Set a ULASX Greyscale Pallete FOR INK
	DIM c as UByte
	FOR c=0 TO 30
		OUT outdata,c 'INDEX
		OUT outdata,c*8.2 'R
		OUT outdata,c*8.2 'G
		OUT outdata,c*8.2 'B
		OUT outcommand,2 'SET INK COLOR
	NEXT c
	'SET last color (white) manually
	OUT outdata,31 'INDEX
	OUT outdata,255 'R
	OUT outdata,255 'G
	OUT outdata,255 'B
	OUT outcommand,2 'SET INK COLOR
end sub

Sub ULASXSetInk(index AS Ubyte)
	'SET INK COLOR
	FLASH 0 : BRIGHT 0
	'PRINT AT 15,0; STR$(inkc)
	IF index>=16 THEN FLASH 1 : index=index-16
	IF index>=8 THEN BRIGHT 1 : LET index=index-8
	INK index
	'PRINT AT 16,0; STR$(inkc)
end sub

Sub PrintPatterns01()
	CLS
	DIM x,y,yoffset as UByte
	yoffset=0
	DO
		if yoffset=0
			OrderedDither25Fill
		else if yoffset=1
			OrderedDither50Fill
		else if yoffset=2
			OrderedDither75Fill
		endif 
		FOR y=0 TO 7
			PAPER y
			FOR x=0 TO 31
				ULASXSetInk(x)
				PRINT AT y+(yoffset*8),x; "\a";
			NEXT x
		NEXT y
		yoffset=yoffset+1
	LOOP UNTIL yoffset>2
	Pause(500)
end sub
Sub PrintPatterns02()
	CLS
	DIM x,y,yoffset as UByte
	yoffset=0
	DO
		if yoffset=0
			Solid0Fill()
		else if yoffset=1
			Solid50LTFill()
		else if yoffset=2
			Solid100Fill()
		endif 
		FOR y=0 TO 7
			PAPER y
			FOR x=0 TO 31
				ULASXSetInk(x)
				PRINT AT y+(yoffset*8),x; "\a";
			NEXT x
		NEXT y
		yoffset=yoffset+1
	LOOP UNTIL yoffset>2
	Pause(500)
end sub

Sub RandomLines()
	Paper 7 : CLS
	DIM x0,x1 as Float
	DIM count as UInteger
	for count=0 TO 9999
		ULASXSetInk(RND*32)
		x0=RND*256
		x1=RND*256
		Plot x0,0
		if(x0<x1) THEN
			Draw x1-x0,191
		else
			Draw -(x0-x1),191
		end if
	next count
end sub

'OrderedDither50Fill()
ULASXSetPalleteGrey32()
OUT outcommand,1 'REM ENABLE ULASX
PrintPatterns01()
PrintPatterns02()
'RandomLines()