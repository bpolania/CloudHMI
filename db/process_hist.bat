@ECHO OFF

rem look for process already running, if found get out
wmic process get commandline |find /I "process_hist.bat"   |find /C /I "cmd" |find /I "2"

if %ERRORLEVEL% EQU 0 GOTO FIM

cd \oshmi\db

IF EXIST forfiles.exe (

FOR /L %%i IN (0,0,0) DO ( 
FORFILES -mhist_*.sql   -c"CMD /c ..\bin\sqlite3.exe -init pragmas_hist.sql -batch hist.sl3   < @FILE && del @FILE & ECHO @FILE" & PING -n 3 127.0.0.1 > nul 
)

) ELSE (

FOR /L %%i IN (0,0,0) DO ( 
FORFILES -m hist_*.sql   -c "CMD /c ..\bin\sqlite3.exe -init pragmas_hist.sql -batch hist.sl3   < @FILE && del @FILE & ECHO @FILE" & PING -n 3 127.0.0.1 > nul 
)

)

:FIM
