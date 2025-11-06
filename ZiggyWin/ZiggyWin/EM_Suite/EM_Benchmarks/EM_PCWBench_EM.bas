#include <C:\Users\NT\Desktop\zxbasic\EM_Base.bas>

EMRegisterAction(10,"PCWBM3")
EMRegisterAction(11,"PCWBM4567")
EMRegisterAction(12,"POW2FL")
EMRegisterAction(13,"LNFL")
EMRegisterAction(14,"SINFL")

function bm1() as ULong
	DIM time1,time2 as ULong
	time1=ticks()
	DIM k as Float
	PRINT "START"
		FOR K=1 TO 1000
	NEXT K
	PRINT "STOP"
	time2=ticks()
	return (time2-time1)
end function

function bm2() as ULong
	DIM time1,time2 as ULong
	time1=ticks()
	DIM k as Float
	PRINT "START"
	k=0
bm230:
	k=k+1
	IF k<1000 THEN GOTO bm230
	PRINT "STOP"
	time2=ticks()
	return (time2-time1)	
end function

function bm3() as ULong
	DIM time1,time2 as ULong
	time1=ticks()
	DIM k,a as Float
	PRINT "START"
	LET k=0
bm330:
	LET k=k+1
	'Write k to EM
	WriteFloatToEM(k)
	'Execute Command
	EMRunCommand(10)
	LET a=ReadFloatFromEM()
	IF k<1000 THEN GOTO bm330
	PRINT "STOP"
	time2=ticks()
	return (time2-time1)
end function

function bm4() as ULong
	DIM time1,time2 as ULong
	time1=ticks()
	DIM k,a as Float
	PRINT "START"
	LET k=0
bm430:
	LET k=k+1
	'Write k to EM
	WriteFloatToEM(k)
	'Execute Command
	EMRunCommand(11)
	LET a=ReadFloatFromEM()
	IF k<1000 THEN GOTO bm430
	PRINT "STOP"
	time2=ticks()
	return (time2-time1)	
end function

sub bm5700()
end sub

function bm5() as ULong
	DIM time1,time2 as ULong
	time1=ticks()
	DIM k,a as Float
	PRINT "START"
	LET k=0
bm530:
	LET k=k+1
	'Write k to EM
	WriteFloatToEM(k)
	'Execute Command
	EMRunCommand(11)
	LET a=ReadFloatFromEM()
	bm5700()
	IF k<1000 THEN GOTO bm530
	PRINT "STOP"
	time2=ticks()
	return (time2-time1)	
end function

sub bm6700()
end sub

function bm6() as ULong
	DIM time1,time2 as ULong
	time1=ticks()
	DIM k,a,l as Float
	PRINT "START"
	LET k=0
	DIM m(5) as Float
bm630:
	LET k=k+1
	'Write k to EM
	WriteFloatToEM(k)
	'Execute Command
	EMRunCommand(11)
	LET a=ReadFloatFromEM()
	bm6700()
	FOR l=1 TO 5
	NEXT l
	IF k<1000 THEN GOTO bm630
	PRINT "STOP"
	time2=ticks()
	return (time2-time1)	
end function

sub bm7700()
end sub

function bm7() as ULong
	DIM time1,time2 as ULong
	time1=ticks()
	DIM k,a,l as Float
	PRINT "START"
	LET k=0
	DIM m(5) as Float
bm730:
	LET k=k+1
	'Write k to EM
	WriteFloatToEM(k)
	'Execute Command
	EMRunCommand(11)
	LET a=ReadFloatFromEM()
	bm7700()
	FOR l=1 TO 5
		LET m(l)=a
	NEXT l
	IF k<1000 THEN GOTO bm730
	PRINT "STOP"
	time2=ticks()
	return (time2-time1)	
end function

function bm8() as ULong
	DIM time1,time2 as ULong
	time1=ticks()
	DIM k,a,b,c as Float
	PRINT "START"
	LET k=0
	DIM m(5) as Float
bm830:
	LET k=k+1
	'Write k to EM
	WriteFloatToEM(k)
	'Execute Command
	EMRunCommand(12)
	LET a=ReadFloatFromEM()
	'Write k to EM
	WriteFloatToEM(k)
	'Execute Command
	EMRunCommand(13)
	LET b=ReadFloatFromEM()
	'Write k to EM
	WriteFloatToEM(k)
	'Execute Command
	EMRunCommand(14)
	LET c=ReadFloatFromEM()
	IF k<1000 THEN GOTO bm830
	PRINT "STOP"
	time2=ticks()
	return (time2-time1)	
end function

DIM tbm1,tbm2,tbm3,tbm4,tbm5,tbm6,tbm7,tbm8 as ULong

tbm1=bm1()
tbm2=bm2()
tbm3=bm3()
tbm4=bm4()
tbm5=bm5()
tbm6=bm6()
tbm7=bm7()
tbm8=bm8()

Print "BM 1 : ";str(tbm1/50)
Print "BM 2 : ";str(tbm2/50)
Print "BM 3 : ";str(tbm3/50)
Print "BM 4 : ";str(tbm4/50)
Print "BM 5 : ";str(tbm5/50)
Print "BM 6 : ";str(tbm6/50)
Print "BM 7 : ";str(tbm7/50)
Print "BM 8 : ";str(tbm8/50)
Print
Print
Print
Print