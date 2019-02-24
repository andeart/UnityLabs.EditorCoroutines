set configurationName=%1
set targetDir=%2
set targetName=%3
set targetCopyDir=%4

if "%configurationName%"=="Release" (
    if not exist %targetCopyDir% mkdir %targetCopyDir%
    cd /d %targetCopyDir%
    for %%a in (*.dll*) do (
        del "%%a"
    )
    for %%a in (*.pdb*) do (
        del "%%a"
    )
    for %%a in (*.xml*) do (
        del "%%a"
    )
    for %%a in (*.mdb*) do (
        del "%%a"
    )
    
    cd /d %targetDir%
    copy "%targetName%.dll" %targetCopyDir% /y
    copy "%targetName%.pdb" %targetCopyDir% /y
    copy "%targetName%.xml" %targetCopyDir% /y
)