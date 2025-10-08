#!/bin/bash

for image in images/*
do
   echo "Install $image service"
   docker load -i $image
   sleep 1
done