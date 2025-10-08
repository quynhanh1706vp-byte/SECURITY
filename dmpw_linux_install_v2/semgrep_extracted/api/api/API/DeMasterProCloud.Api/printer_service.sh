#!/bin/bash

while true;
do
	if [ -e "/app/card_printer" ]; then
		for directory in "/app/card_printer"/*
		do
			if [ ! $directory == "/app/card_printer/*" ]; then
				echo "$directory/script.sh";
				chmod +x $directory/script.sh;
				$directory/script.sh;
				rm -rf $directory;
				sleep 5;
			fi
		done
	else
		sleep 5;
	fi
done