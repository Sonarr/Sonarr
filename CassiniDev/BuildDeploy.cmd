del ..\deploy\*.* /s/q
xcopy bin\x86 ..\deploy /S /I /F /Y <nul:
del ..\deploy\*vshost* /s
del ..\deploy\release\*.pdb /s
del ..\deploy\*.db3 /s
rem tree ..\deploy /f>..\deploy.txt
copy ..\*.txt ..\deploy\  /Y
