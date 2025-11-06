#include <C:\Users\NT\Desktop\zxbasic\EM_Base.bas>

Sub EM_Test_SINFL()
	DIM a,b as Float
	DIM iters,counter,time1,time2,time3,time4 as ULong
	a=0.78539816339
	b=1.0
	iters=99
	time1=0.0
	time2=0.0
	time3=0.0
	time4=0.0

	EMRegisterAction(10,"SINFL")

ROMSIN:
	Print "SINFL ROM ";
	'ROM method
	time1=ticks()
	For counter=0 to iters
		b=SIN(a)
	Next
	time2=ticks()
	print STR(b)
	
EMSINFL:
	Print "SINFL EM ";
	'EM SINFL
	time3=ticks()
	For counter=0 to iters
		WriteFloatToEM(a)
		EMRunCommand(10)
		b=ReadFloatFromEM()
	Next
	time4=ticks()
	print STR(b)

	print STR(time2-time1);" - ";STR(time4-time3)
	print ""
end sub

Sub EM_Test_SQRTSINFL()
	DIM a,b as Float
	DIM iters,counter,time1,time2,time3,time4 as ULong
	a=0.78539816339
	b=1.0
	iters=99
	time1=0.0
	time2=0.0
	time3=0.0
	time4=0.0

	EMRegisterAction(10,"SRQTSINFL")

ROMSQRSIN:
	Print "SRQTSINFL ROM ";
	'ROM method 
	time1=ticks()
	For counter=0 to iters
		b=SQR(SIN(a))
	Next
	time2=ticks()
	print STR(b)
	
EMSQRTSINFL:
	Print "SRQTSINFL EM ";
	'EM SRQTSINFL
	time3=ticks()
	For counter=0 to iters
		WriteFloatToEM(a)
		EMRunCommand(10)
		b=ReadFloatFromEM()
	Next
	time4=ticks()
	print STR(b)

	print STR(time2-time1);" - ";STR(time4-time3)
	print ""
end sub

Sub EM_Test_COSSQRTSINFL()
	DIM a,b as Float
	DIM iters,counter,time1,time2,time3,time4 as ULong
	a=0.78539816339
	b=1.0
	iters=99
	time1=0.0
	time2=0.0
	time3=0.0
	time4=0.0

	EMRegisterAction(10,"COSSRQTSINFL")

ROMCOSSQRSIN:
	Print "COSSRQTSINFL ROM ";
	'ROM method 
	time1=ticks()
	For counter=0 to iters
		b=COS(SQR(SIN(a)))
	Next
	time2=ticks()
	print STR(b)
	
EMCOSSQRTSINFL:
	Print "COSSRQTSINFL EM ";
	'EM SRQTSINFL
	time3=ticks()
	For counter=0 to iters
		WriteFloatToEM(a)
		EMRunCommand(10)
		b=ReadFloatFromEM()
	Next
	time4=ticks()
	print STR(b)

	print STR(time2-time1);" - ";STR(time4-time3)
	print ""
end sub

Sub EM_Test_LOGCOSSQRTSINFL()
	DIM a,b as Float
	DIM iters,counter,time1,time2,time3,time4 as ULong
	a=0.78539816339
	b=1.0
	iters=99
	time1=0.0
	time2=0.0
	time3=0.0
	time4=0.0

	EMRegisterAction(10,"LOGCOSSRQTSINFL")

ROMLOGCOSSQRSIN:
	Print "LOGCOSSRQTSINFL ROM ";
	'ROM method 
	time1=ticks()
	For counter=0 to iters
		b=LN(COS(SQR(SIN(a))))
	Next
	time2=ticks()
	print STR(b)
	
EMLOGCOSSQRTSINFL:
	Print "LOGCOSSRQTSINFL EM ";
	'EM SRQTSINFL
	time3=ticks()
	For counter=0 to iters
		WriteFloatToEM(a)
		EMRunCommand(10)
		b=ReadFloatFromEM()
	Next
	time4=ticks()
	print STR(b)

	print STR(time2-time1);" - ";STR(time4-time3)
	print ""
end sub

EM_Test_SINFL()
EM_Test_SQRTSINFL()
EM_Test_COSSQRTSINFL()
EM_Test_LOGCOSSQRTSINFL()