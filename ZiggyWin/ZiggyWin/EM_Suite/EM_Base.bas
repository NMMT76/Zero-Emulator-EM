Function ticks() as ULong
	Dim b1,b2,b3 AS Ubyte
	b1=peek(UByte,23674)
	b2=peek(UByte,23673)
	b3=peek(UByte,23672)
	return 65536*b1+256*b2+ b3
END Function

Sub UIntegerToUInt162Bytes(ByRef msb as Ubyte,ByRef lsb as Ubyte,value as UInteger)
	msb=value/256
	lsb=value-(msb*256)
End Sub

Sub WriteStringToEM(stringtowrite$ as String)
	DIM index,stringlength AS ULong
	stringlength=LEN(stringtowrite$)
	'push string to EM
	FOR index=0 TO stringlength-1
		OUT 63,CODE(stringtowrite$(index))
	NEXT index
	'Zero terminate the string
	OUT 63,0
end Sub

Function ReadStringFromEM() as String
	DIM outstring$ as String
	DIM inbyte as Ubyte
	do
		'Read byte from EM
		inbyte=IN 63
		if inbyte<>0
			outstring$=outstring$+CHR(inbyte)
		end if
	Loop until inbyte=0
	return outstring$
end Function

Sub WriteFixedToEM(value as Fixed)
	'push Fixed to EM
	OUT 63,PEEK (@value)
	OUT 63,PEEK (@value+1)
	OUT 63,PEEK (@value+2)
	OUT 63,PEEK (@value+3)
end Sub

Function ReadFixedFromEM() as Fixed
	DIM retval as Fixed
	Poke (@retval),IN 63
	Poke (@retval+1),IN 63
	Poke (@retval+2),IN 63
	Poke (@retval+3),IN 63
	return retval
end Function

Sub WriteFloatToEM(value as Float)
	'push Fixed to EM
	OUT 63,PEEK (@value)
	OUT 63,PEEK (@value+1)
	OUT 63,PEEK (@value+2)
	OUT 63,PEEK (@value+3)
	OUT 63,PEEK (@value+4)
end Sub

Function ReadFloatFromEM() as Float
	DIM retval as Float
	Poke (@retval),IN 63
	Poke (@retval+1),IN 63
	Poke (@retval+2),IN 63
	Poke (@retval+3),IN 63
	Poke (@retval+4),IN 63
	return retval
end Function

Sub WriteULongToEM(value as ULong)
	'push Fixed to EM
	OUT 63,PEEK (@value)
	OUT 63,PEEK (@value+1)
	OUT 63,PEEK (@value+2)
	OUT 63,PEEK (@value+3)
end Sub

Function ReadULongFromEM() as ULong
	DIM retval as Ulong
	Poke (@retval),IN 63
	Poke (@retval+1),IN 63
	Poke (@retval+2),IN 63
	Poke (@retval+3),IN 63
	return retval
end Function

Sub WriteUIntegerToEM(value as UInteger)
	'push Fixed to EM
	OUT 63,PEEK (@value)
	OUT 63,PEEK (@value+1)
end Sub

Function ReadUIntegerFromEM() as UInteger
	DIM retval as UInteger
	Poke (@retval),IN 63
	Poke (@retval+1),IN 63
	return retval
end Function

sub EMRunCommand(index as Ubyte)
	'Start processing
	OUT 191,index
	'Wait for processing to end
	while IN 191>0
	END WHILE
end sub

sub EMRunCommandNoWait(index as Ubyte)
	'Start processing
	OUT 191,index
end sub

sub EMWaitReady()
	'Wait for processing to end
	while IN 191>0
	END WHILE
end sub

Sub EMRegisterAction(index as Ubyte, action as String)
	'Push RegisterAction
	OUT 63,index
	'Push GUID
	WriteStringToEM(action)
	'Run RegisterAction (always at vtable 1)
	OUT 191,1
	'Wait for processing to end
	while IN 191>0
	END WHILE
end sub

Sub GetGUIDTest()
	DIM index as Ubyte
	'Register the GUID command to index 3
	EMRegisterAction(3,"GUID")
	'Call GUID (which is now at vtable index 3)
	EMRunCommand(3)
	'Read and print data (GUID is 16bytes)
	for index=0 TO 15
		print STR(IN 63)+" ";
	next
end sub

Sub GetGUIDStringTest()
	DIM index as Ubyte
	'Register the GUIDS command to index 3
	EMRegisterAction(3,"GUIDS")
	'Call GUID (which is now at vtable index 3)
	EMRunCommand(3)
	'Read and print data (GUIDS is 36bytes as string)
	for index=0 TO 35
		print CHR(IN 63);
	next
end sub