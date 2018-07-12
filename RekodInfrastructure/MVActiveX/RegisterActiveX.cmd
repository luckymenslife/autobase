echo Run with ADMINISTRATOR rights
c:
cd %~dps0
regsvr32 /u mvMapLib.ocx
regsvr32 mvMapLib.ocx
pause