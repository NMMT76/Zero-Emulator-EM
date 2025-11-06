#include <C:\Users\NT\Desktop\zxbasic\EM_Base.bas>

Sub EM_Test_BUTTERFLYCURVE()
	DIM t,x,y,stp as Float
	DIM time1,time2,time3,time4 as ULong
	t=1.0
	x=1.0
	y=1.0
	stp=0.1
	time1=0.0
	time2=0.0
	time3=0.0
	time4=0.0

	EMRegisterAction(10,"BUTTERFLY")
	
BBME1:	
	'BB Method
	Print "BUTTERFLY ROM ";
	time1=ticks()
	For t=0 to 12*PI STEP stp
		x = sin(t)*(exp(cos(t))-2*cos(4*t)-(sin(t/12))^5)
		y = cos(t)*(exp(cos(t))-2*cos(4*t)-(sin(t/12))^5)
		PLOT x*20.0+80.0,y*20.0+60.0
	Next
	time2=ticks()
	CLS
	
EMBC:	
	'EM HAPYTH
	Print "BUTTERFLY EM ";
	time3=ticks()
	For t=0 to 12*PI STEP stp
		WriteFloatToEM(t)
		EMRunCommand(10)
		x=ReadFloatFromEM()
		y=ReadFloatFromEM()
		PLOT x*20.0+80.0,y*20.0+60.0
	Next
	time4=ticks()
	
	print ""
	
	print STR(time2-time1)
	print STR(time4-time3)
end sub

EM_Test_BUTTERFLYCURVE()