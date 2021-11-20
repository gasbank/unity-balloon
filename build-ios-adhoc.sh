#!/bin/bash

UNITY_VERSION=`cat ProjectSettings/ProjectVersion.txt | head -n1 | awk '{print $2;}'`
UNAME_OUT="$(uname -s)"
case "${UNAME_OUT}" in
    Linux*)     UNITY_EDITOR="/home/gb/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity";;
    Darwin*)    UNITY_EDITOR="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity";;
    *)          UNITY_EDITOR="UNKNOWN:${UNAME_OUT}"
esac

if [ -z "${BUILD_NUMBER}" ]; then
  BUILD_NUMBER="<NO ENV>"
fi

/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity \
    -quit \
    -batchmode \
    -executeMethod BalloonBuild.PerformIosAdHocBuild \
    -logfile build.log \
    -projectPath `pwd` \
    -buildTarget iOS \
    -buildNumber ${BUILD_NUMBER} \
    -noGraphics \
    -username ${UNITY_USERNAME} \
    -password ${UNITY_PASSWORD}
