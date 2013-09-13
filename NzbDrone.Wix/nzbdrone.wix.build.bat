rd _raw  /s /q
rd _setup /s /q
xcopy  ..\SyntikX.Client\bin\release\*.*  _raw\ /S /V /I /F /R

"C:\Program Files (x86)\WiX Toolset v3.6\bin\candle.exe" -nologo "syntik.wix.build.wxs" -out "_setup\SyntikX.Wix.wixobj"  -ext WixNetFxExtension -ext WixUIExtension
"C:\Program Files (x86)\WiX Toolset v3.6\bin\light.exe" -nologo "_setup\SyntikX.Wix.wixobj" -out "_setup\SyntikX.msi"  -ext WixNetFxExtension -ext WixUIExtension

pause