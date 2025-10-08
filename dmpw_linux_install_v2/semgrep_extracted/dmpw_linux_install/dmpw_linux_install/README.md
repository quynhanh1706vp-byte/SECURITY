# DMPW

## System requirement

 - CPU: >= 2
 - RAM: >= 16G
 - OS:  linux based operating system (Ubuntu, Debian, Centos,...)
 - Docker and docker compose

## Getting Started
The project include services that are run with Docker environment.

## Extract file backup install

    $ mkdir dmpw_linux_install
    $ tar -xzvf dmpw_linux_install.tar.gz -C ./dmpw_linux_install

## Setup docker

    $ chmod +x docker/install_docker_noble.sh
    $ ./docker/install_docker_noble.sh

## Setup environment

    $ chmod +x install.sh
    $ ./install.sh -h {IP_OF_SERVER}

## Start service

    $ chmod +x start_services.sh
    $ ./start_services.sh

## Stop service

    $ chmod +x stop_services.sh
    $ ./stop_services.sh