'Demo of the LoadVideoFileBW/TransferVideoFrameBWDMA methods
'While from a "theoretical" view this CAN be done on real hardware
'there is NO way to accuratelly assure it in Zero as it would
'need cycle accurate emulation.
'If the "spare" time between VBI's is 11637 TStates this should be enough
'to write the 6144 bytes a full frame needs IF the DMA device can agressivelly
'drive the bus agressivelly at an optimal 1T. If the ULA can delay the writes,
'then all bets are off.

'Including the IM2 library
#include "IM2.bas"

#include <C:\Users\NT\Desktop\zxbasic\EM_Base.bas>

DIM framecount as ULong
DIM frameskip AS Ubyte
DIM timercounter AS ULong

SUB FASTCALL IntCall()
	if framecount<0
		return
	end if
	if frameskip=0
		if framecount>0
			EMRunCommandNoWait(11)
			framecount=framecount-1
		end if
		frameskip=1
	else
		frameskip=0
	end if
END SUB

CLS
EMRegisterAction(10,"LOADVIDEOFILEBW")
EMRegisterAction(11,"TRANSFERVIDEOFRAMEBWDMA")

WriteStringToEM("E:\\Dev\\Data\\Touhou-Bad-Apple.mp4")
'WriteStringToEM("E:\\Dev\\Data\\Cyberpunk_Retro.mp4")
'WriteStringToEM("E:\\Dev\\Data\\Train.mp4")
'WriteStringToEM("E:\\Dev\\Data\\StreetTimelapse.mp4")
OUT 63,0 'Quant mode
OUT 63,1 'Invert fg/bg
OUT 63,1 'Frameskip 0
EMRunCommandNoWait(10)

timercounter=0
while IN 191>0
	Print at 1,1;str(timercounter)
	timercounter=timercounter+1
END WHILE

framecount=ReadULongFromEM()
CLS
frameskip=0

IM2Start(@IntCall)

'Infinite loop so we know at what value we maxed out
While framecount>0
End While

