#!/bin/bash

UNITY_VERSION=2019.1.8f1
UNAME_OUT="$(uname -s)"
case "${UNAME_OUT}" in
    Linux*)     UNITY_EDITOR="/home/gb/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity";;
    Darwin*)    UNITY_EDITOR="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity";;
    *)          UNITY_EDITOR="UNKNOWN:${UNAME_OUT}"
esac

if [ -z "${BUILD_NUMBER}" ]; then
  BUILD_NUMBER="<NO ENV>"
fi

${UNITY_EDITOR} \
    -quit \
    -batchmode \
    -executeMethod BalloonBuild.PerformAndroidBuild \
    -logfile >(tee build.log) \
    -projectPath `pwd` \
    -buildTarget Android \
    -keystorePass ${KEYSTORE_PASS} \
    -buildNumber ${BUILD_NUMBER} \
    -noGraphics
