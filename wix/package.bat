bin\heat.exe dir ..\_output\ -out _output.wxs -cg OUTPUT_GROUP -dr BIN_DIR -gg -scom -srd -sfrag -sreg -suid -svb6

"bin\candle.exe" nzbdrone.wxs _output.wxs -ext WixNetFxExtension -ext WixUIExtension
"bin\light.exe"  nzbdrone.wixobj _output.wixobj -out "nzbdrone.msi"  -ext WixNetFxExtension -ext WixUIExtension -b ..\_output

