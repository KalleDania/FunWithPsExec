@if (@CodeSection == @Batch) @then
@echo off
CScript //nologo //E:JScript "%~F0"
goto :EOF
@end
WScript.CreateObject("WScript.Shell").SendKeys("{ENTER}");