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

echo "KILL SESSION : printer_${rid}"
tmux kill-session -t "printer_${rid}"
