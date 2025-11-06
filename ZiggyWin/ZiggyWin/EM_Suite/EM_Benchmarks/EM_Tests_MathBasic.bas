#include <C:\Users\NT\Desktop\zxbasic\EM_Base.bas>

Sub Test_ADDFL
	DIM time1,time2 as ULong
	DIM f1,f2,res as Float
	DIM counter,iters as UInteger
	f1=0.901
	f2=0.30905
	iters=5000
	EMRegisterAction(10,"ADDFL")
	Print "Add Float ";STR(iters);" iterations"
	time1=ticks()
	for counter=0 TO iters-1
		res=f1+f2
	next
	time2=ticks()
	Print "ROM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
	time1=ticks()
	for counter=0 TO iters-1
		WriteFloatToEM(f1)
		WriteFloatToEM(f2)
		EMRunCommand(10)
		res=ReadFloatFromEM()
	next
	time2=ticks()
	Print "EM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
end sub

Sub Test_SUBFL
	DIM time1,time2 as ULong
	DIM f1,f2,res as Float
	DIM counter,iters as UInteger
	f1=0.901
	f2=0.30905
	iters=5000
	EMRegisterAction(10,"SUBFL")
	Print "Sub Float ";STR(iters);" iterations"
	time1=ticks()
	for counter=0 TO iters-1
		res=f1-f2
	next
	time2=ticks()
	Print "ROM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
	time1=ticks()
	for counter=0 TO iters-1
		WriteFloatToEM(f1)
		WriteFloatToEM(f2)
		EMRunCommand(10)
		res=ReadFloatFromEM()
	next
	time2=ticks()
	Print "EM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
end sub

Sub Test_MULFL
	DIM time1,time2 as ULong
	DIM f1,f2,res as Float
	DIM counter,iters as UInteger
	f1=0.901
	f2=0.30905
	iters=5000
	EMRegisterAction(10,"SUBFL")
	Print "Mul Float ";STR(iters);" iterations"
	time1=ticks()
	for counter=0 TO iters-1
		res=f1*f2
	next
	time2=ticks()
	Print "ROM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
	time1=ticks()
	for counter=0 TO iters-1
		WriteFloatToEM(f1)
		WriteFloatToEM(f2)
		EMRunCommand(10)
		res=ReadFloatFromEM()
	next
	time2=ticks()
	Print "EM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
end sub

Sub Test_DIVFL
	DIM time1,time2 as ULong
	DIM f1,f2,res as Float
	DIM counter,iters as UInteger
	f1=0.901
	f2=0.30905
	iters=5000
	EMRegisterAction(10,"SUBFL")
	Print "Div Float ";STR(iters);" iterations"
	time1=ticks()
	for counter=0 TO iters-1
		res=f1/f2
	next
	time2=ticks()
	Print "ROM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
	time1=ticks()
	for counter=0 TO iters-1
		WriteFloatToEM(f1)
		WriteFloatToEM(f2)
		EMRunCommand(10)
		res=ReadFloatFromEM()
	next
	time2=ticks()
	Print "EM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
end sub

Sub Test_ADDSUBMULDIV
	DIM time1,time2 as ULong
	DIM f1,f2,f3,f4,f5,res as Float
	DIM counter,iters as UInteger
	f1=0.901
	f2=0.30905
	f3=1.705
	f4=2.051
	f5=1.207
	iters=5000
	EMRegisterAction(10,"ADDSUBMULDIV")
	Print "ADDSUBMULDIV Float ";STR(iters);" iterations"
	time1=ticks()
	for counter=0 TO iters-1
		res=(((f1+f2)-f3)*f4)/f5
	next
	time2=ticks()
	Print "ROM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
	time1=ticks()
	for counter=0 TO iters-1
		WriteFloatToEM(f1)
		WriteFloatToEM(f2)
		WriteFloatToEM(f3)
		WriteFloatToEM(f4)
		WriteFloatToEM(f5)
		EMRunCommand(10)
		res=ReadFloatFromEM()
	next
	time2=ticks()
	Print "EM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
end sub

Sub Test_ADDSUBMULDIVQDMA
	DIM time1,time2 as ULong
	DIM f1,f2,f3,f4,f5,res as Float
	DIM counter,iters as UInteger
	f1=0.901
	f2=0.30905
	f3=1.705
	f4=2.051
	f5=1.207
	iters=500
	EMRegisterAction(10,"ADDSUBMULDIV")
	Print "ADDSUBMULDIV Float ";STR(iters);" iterations"
	time1=ticks()
	for counter=0 TO iters-1
		res=(((f1+f2)-f3)*f4)/f5
	next
	time2=ticks()
	Print "ROM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
	time1=ticks()
	for counter=0 TO iters-1
		WriteFloatToEM(f1)
		WriteFloatToEM(f2)
		WriteFloatToEM(f3)
		WriteFloatToEM(f4)
		WriteFloatToEM(f5)
		EMRunCommand(10)
		res=ReadFloatFromEM()
	next
	time2=ticks()
	Print "EM: ";Str((time2-time1));"t ";Str((time2-time1)/50);"s "
end sub

Test_ADDFL()
Print
Test_SUBFL()
Print
Test_MULFL()
Print
Test_DIVFL()
Print
Test_ADDSUBMULDIV()