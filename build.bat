@echo off

cls

rem compile
c:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\csc /out:.\out\submit-effort.exe *.cs

rem copy bins to out dir
copy *.dll .\out\
copy app.config .\out\submit-effort.exe.config