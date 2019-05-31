# 1. Copy key file
Copy-Item -Force -Recurse -Verbose C:\balloon\key .

# 2. Remove Previous APK
Remove-Item -Force -ErrorAction:Ignore -Recurse -Verbose balloon.apk

$BUILD_LOG_FILE_NAME = 'build.log'

# 3. Build APK
if (-not (Test-Path env:DUMMY_BUILD)) {
    if (-not (Test-Path env:BUILD_NUMBER)) {
        $env:BUILD_NUMBER = '<NO ENV>'
    }

    & "C:\Program Files\Unity\Hub\Editor\2019.1.1f1\Editor\Unity.exe" `
        -quit `
        -batchmode `
        -executeMethod BalloonBuild.PerformAndroidBuild `
        -logfile $BUILD_LOG_FILE_NAME `
        -projectPath $pwd `
        -buildTarget Android `
        -keystorePass $env:KEYSTORE_PASS `
        -buildNumber $env:BUILD_NUMBER `
        -noGraphics | Out-Null
} else {
    New-Item -Force -Verbose $BUILD_LOG_FILE_NAME
    Set-Content $BUILD_LOG_FILE_NAME -Verbose 'test build.log content'

    New-Item -Force -Verbose balloon.apk
    Set-Content balloon.apk -Verbose 'test apk file content'
}

# 4. Check build.log
$ERROR_LINES = (Get-Content build.log | Select-String "\): error CS")
$ERROR_COUNT = $ERROR_LINES.length
if ($ERROR_COUNT -ne 0) {
    Write-Output "$ERROR_COUNT lines detected!"
	Write-Output $ERROR_LINES
    exit $ERROR_COUNT
}

$ERROR_LINES = (Get-Content build.log | Select-String " Error: ")
$ERROR_COUNT = $ERROR_LINES.length
if ($ERROR_COUNT -ne 0) {
    Write-Output "$ERROR_COUNT lines detected!"
	Write-Output $ERROR_LINES
    exit $ERROR_COUNT
}

$ERROR_LINES = (Get-Content build.log | Select-String ": Build Failed")
$ERROR_COUNT = $ERROR_LINES.length
if ($ERROR_COUNT -ne 0) {
    Write-Output "$ERROR_COUNT lines detected!"
	Write-Output $ERROR_LINES
    exit $ERROR_COUNT
}

# 5. Upload APK (disabled)
####

# 6. Deploy to d.yyt.life
.\upload-to-yyt.ps1 balloon android-armeabi-v7a top.plusalpha.balloon "balloon.apk\Balloon Rider.armeabi-v7a.apk"
.\upload-to-yyt.ps1 balloon android-arm64-v8a top.plusalpha.balloon "balloon.apk\Balloon Rider.arm64-v8a.apk"
.\upload-to-yyt.ps1 balloon android-x86 top.plusalpha.balloon "balloon.apk\Balloon Rider.x86.apk"

# 7.  Notify Developers on Telegram
.\notify-telegram-android-windows.ps1
