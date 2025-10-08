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


while true;
do
	if [ -e "/app/card_printer/${rid}" ]; then
		for directory in "/app/card_printer/${rid}"/*
		do
			if [ ! $directory == "/app/card_printer/${rid}/*" ]; then
				echo "DIRECTORY : $directory";
				sleep 1;
				echo "chmod +x $directory/script.sh";
				chmod +x $directory/script.sh;
				sleep 1;
				echo "$directory/script.sh";
				$directory/script.sh;
				sleep 1;
				echo "rm -rf $directory";
				rm -rf $directory;
				sleep 5;
		    else
				sleep 3;
			fi
		done
	else
		sleep 5;
	fi
done