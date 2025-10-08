#!/bin/bash

dpkg -i ./docker/jammy/containerd.io.amd64.deb
dpkg -i ./docker/jammy/docker-ce.amd64.deb
dpkg -i ./docker/jammy/docker-ce-cli.amd64.deb
dpkg -i ./docker/jammy/docker-ce-rootless-extras.amd64.deb
dpkg -i ./docker/jammy/docker-buildx-plugin.amd64.deb
dpkg -i ./docker/jammy/docker-compose-plugin.amd64.deb
dpkg -i ./docker/jammy/docker-model-plugin.amd64.deb