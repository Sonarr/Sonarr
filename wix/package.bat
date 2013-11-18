SET BUILD_NUMBER=1.9.9.9

"bin\candle.exe" -nologo "nzbdrone.wxs" -out "nzbdrone.wixobj"  -ext WixNetFxExtension -ext WixUIExtension
"bin\light.exe" -nologo "nzbdrone.wixobj" -out "nzbdrone.msi"  -ext WixNetFxExtension -ext WixUIExtension