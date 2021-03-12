cd brainflow 
echo building brainflow win32
call "tools\build_win32_vs19.cmd"
echo building brainflow win64
call "tools\build_win64_vs19.cmd"
cd ..
echo copying files
xcopy  brainflow\compiled\Release\BoardController32.dll ..\lib\brainflow\x86\ /Y
xcopy  brainflow\compiled\Release\DataHandler32.dll ..\lib\brainflow\x86\ /Y
xcopy  brainflow\compiled\Release\GanglionLib32.dll ..\lib\brainflow\x86\ /Y
xcopy  brainflow\compiled\Release\MLModule32.dll ..\lib\brainflow\x86\ /Y


xcopy  brainflow\compiled\Release\BoardController.dll ..\lib\brainflow\x64\ /Y
xcopy  brainflow\compiled\Release\DataHandler.dll ..\lib\brainflow\x64\ /Y
xcopy  brainflow\compiled\Release\GanglionLib.dll ..\lib\brainflow\x64\ /Y
xcopy  brainflow\compiled\Release\MLModule.dll ..\lib\brainflow\x64\ /Y
