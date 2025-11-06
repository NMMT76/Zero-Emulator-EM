'Tests raw read/write throughput to EdgeMasterTester. Data is random garbage,
'ie, floating bus value.

#include <C:\Users\NT\Desktop\zxbasic\EMTester_RawThroughputUnrolls.bas>

Function ticks() as ULong
	Dim b1,b2,b3 AS Ubyte
	b1=peek(UByte,23674)
	b2=peek(UByte,23673)
	b3=peek(UByte,23672)
	return 65536*b1+256*b2+ b3
END Function

Sub Test_Read(size as ULong)
	DIM count,time1,time2,rounded as ULong
	'Dim rounded as UInteger
	DIM inbyte as Ubyte
	time1=ticks()
	For count=0 TO size-1
		'Test_In_Unroll_2()
		inbyte=IN 99
	NEXT count
	time2=ticks()
	rounded=Cast(ULong,(size/1024)/((time2-time1)/50))
	Print "R: ";Str(size);"b ";Str((time2-time1));"t ";Str((time2-time1)/50);"s ";Str(rounded);"KB/s"
end sub
Sub Test_Write(size as ULong)
	DIM count,time1,time2,rounded as ULong
	'Dim rounded as UInteger
	time1=ticks()
	For count=0 TO size-1
		'Test_OUT_Unroll_2()
		OUT 99,0
	NEXT count
	time2=ticks()
	rounded=Cast(ULong,(size/1024)/((time2-time1)/50))
	Print "W: ";Str(size);"b ";Str((time2-time1));"t ";Str((time2-time1)/50);"s ";Str(rounded);"KB/s"
end sub

CLS
Test_Read(1024*1024*2)
Print
Test_Write(1024*1024*2)
Print
