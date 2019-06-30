#!/bin/bash

# should be executed as bash script, not sh script!

# https://stackoverflow.com/questions/10794300/how-to-compose-git-log-for-pending-changes-in-teamcity

BUILD_TYPE="${system.teamcity.buildType.id}"
# *****************
DOWNLOAD_URL_ARMEABI_V7A=$(curl -s -XGET https://api.yyt.life/d/balloon/android-armeabi-v7a?count=1 | cut -d'"' -f12)
DOWNLOAD_URL_ARM64_V8A=$(curl -s -XGET https://api.yyt.life/d/balloon/android-arm64-v8a?count=1 | cut -d'"' -f12)
DOWNLOAD_URL_X86=$(curl -s -XGET https://api.yyt.life/d/balloon/android-x86?count=1 | cut -d'"' -f12)
# *****************
BUILD_NUMBER="${build.number}"

MESSAGE="[From iMac] Build Number: $BUILD_NUMBER\nBuild Type: $BUILD_TYPE\nDownload: <a href='$DOWNLOAD_URL_ARMEABI_V7A'>ARMEABI_V7A</a> / <a href='$DOWNLOAD_URL_ARM64_V8A'>ARM64_V8A</a> / <a href='$DOWNLOAD_URL_X86'>X86</a>"

echo -n "$MESSAGE" | curl -s -X POST -H "Content-Type: text/plain" -d@- https://plusalpha.top/balloon-telegram/$BOT_TOKEN/teamcity-build-event
