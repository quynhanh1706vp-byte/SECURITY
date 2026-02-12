#!/bin/bash

while getopts "h:" opt
do
    case "$opt" in
    	h ) rid="$OPTARG" ;;
	esac
done

if [[ -z "$rid" ]]
then
	echo "Printer ID is not provide, please use script with option -h"
	exit 1
fi

tmux new-session -d -s "printer_${rid}" "/app/printer_service2.sh -h ${rid} 2>&1 | tee printer_${rid}.log"
