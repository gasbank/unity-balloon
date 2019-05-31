#!/bin/bash

ERROR_LINES=$(cat build.log | grep "): error CS")
if [ -z "$ERROR_LINES" ];
then
    exit 0
else
    echo $ERROR_LINES
    exit 1
fi

ERROR_LINES=$(cat build.log | grep " Error: ")
if [ -z "$ERROR_LINES" ];
then
    exit 0
else
    echo $ERROR_LINES
    exit 1
fi

ERROR_LINES=$(cat build.log | grep ": Build Failed")
if [ -z "$ERROR_LINES" ];
then
    exit 0
else
    echo $ERROR_LINES
    exit 1
fi
