'80989 > 1544
#include <C:\Users\NT\Desktop\zxbasic\EM_Base.bas>

#define width 256
#define height 192

Function EM_Test_MANDELBB() as ULONG
 
	DIM x,y AS FIXED
	DIM xstart,xstep,ystart,ystep AS FIXED
	DIM xend,yend AS FIXED
	DIM z,zi,newz,newzi AS FIXED
	DIM colour AS BYTE
	DIM iter AS UINTEGER
	DIM col AS UINTEGER
	DIM i,k AS UBYTE
	DIM j AS UINTEGER
	DIM inset AS UBYTE
	 
	DIM time1,time2 as ULong
	 
	xstart=-1.6
	xend=0.65
	ystart=-1.15
	yend=-ystart
	iter=24
	 
	xstep=(xend-xstart)/width
	ystep=(yend-ystart)/height
	 
	'Main loop
	x=xstart
	y=ystart
	 
	BORDER 0
	PAPER 0
	INK 7
	CLS

	time1=ticks()

	FOR i=0 TO ( height -1 )/2 +1
			FOR j=0 TO width -1
				z=0
				zi=0
				inset=1
					FOR k=0 TO iter
						';z^2=(a+bi)*(a+bi) = a^2+2abi-b^2
						newz=(z*z)-(zi*zi)+x
						newzi=2*z*zi+y
						z=newz
						zi=newzi
					   
						IF (z*z)+(zi*zi) > 4 THEN
							inset=0
							colour=k
							GOTO screen
						END IF
					NEXT k
				   
	screen:               
					IF NOT inset THEN
						IF colour BAND 1 THEN
							PLOT j,i
							PLOT j,192-i
						END IF
					END IF
					   
					x=x+xstep
			 NEXT j
				   
			y=y+ystep
			x=xstart
	NEXT i

	time2=ticks()
	
	return (time2-time1)

End function

Function EM_Test_MANDELEM() as ULONG

	DIM x,y AS FIXED
	DIM xstart,xstep,ystart,ystep AS FIXED
	DIM xend,yend AS FIXED
	DIM z,zi,newz,newzi AS FIXED
	DIM colour AS BYTE
	DIM iter AS UByte
	DIM i,k AS UBYTE
	DIM j AS UINTEGER
	DIM inset AS UBYTE
	DIM time1,time2 as ULONG

	xstart=-1.6
	xend=0.65
	ystart=-1.15
	yend=-ystart
	iter=24

	xstep=(xend-xstart)/width
	ystep=(yend-ystart)/height

	'Main loop
	x=xstart
	y=ystart

	BORDER 0
	PAPER 0
	INK 7
	CLS

	EMRegisterAction(10,"MANDELCALC")

	time1=ticks()

	FOR i=0 TO ( height -1 )/2 +1
		FOR j=0 TO width -1
			'Write colour to EM
			OUT 63,colour
			'Write iter to EM
			OUT 63,iter
			'Write x to EM
			WriteFixedToEM(x)
			'Write y to EM
			WriteFixedToEM(y)
			'Run MANDELCALC
			EMRunCommand(10)
			'Read colour from EM
			colour=IN 63
			'Read inset from EM
			inset=IN 63
			
			IF NOT inset THEN
				IF colour BAND 1 THEN
					PLOT j,i
					PLOT j,192-i
				END IF
			END IF
			
			x=x+xstep
			
		NEXT j
		y=y+ystep
		x=xstart
	NEXT i

	time2=ticks()

	return (time2-time1)

END Function

DIM timebb,timeem as ULong
timeem=EM_Test_MANDELEM()
CLS
timebb=EM_Test_MANDELBB()
CLS
print "BB : ";str(timebb);" - EM : ";str(timeem)