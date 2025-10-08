#!/bin/bash

dpkg -i ./docker/noble/containerd.io.amd64.deb
dpkg -i ./docker/noble/docker-ce.amd64.deb
dpkg -i ./docker/noble/docker-ce-cli.amd64.deb
dpkg -i ./docker/noble/docker-ce-rootless-extras.amd64.deb
dpkg -i ./docker/noble/docker-buildx-plugin.amd64.deb
dpkg -i ./docker/noble/docker-compose-plugin.amd64.deb
dpkg -i ./docker/noble/docker-model-plugin.amd64.deb