#!/bin/bash
tmux new-session -d -s printer '/app/printer_service.sh 2>&1 | tee printer.log'
