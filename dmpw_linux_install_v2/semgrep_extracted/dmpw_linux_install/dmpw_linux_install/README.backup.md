# DMPW

## System requirement

 - CPU: >= 2
 - RAM: >= 16G
 - OS:  linux based operating system (Ubuntu, Debian, Centos,...)
 - Docker and docker compose
  
## Getting Started
The project include services that are run with Docker environment.

## Compress and copy folder installation
Go to folder dmpw_linux_install

    $ mkdir database
    $ docker exec -it postgres-db pg_dump -U postgres -d demasterpro > ./database/backup.sql
    $ ./stop_services.sh
    $ tar --exclude='dmpw_linux_install_backup.tar.gz' -cvzf dmpw_linux_install_backup.tar.gz .

## Install new server
Extract file backup install

    $ mkdir dmpw_linux_install
    $ tar -xzvf dmpw_linux_install_backup.tar.gz -C ./dmpw_linux_install

Run setup environment

    $ cd dmpw_linux_install
    $ chmod +x load_image.sh
    $ ./load_image.sh

## Start service

    $ chmod +x start_services.sh
    $ ./start_services.sh

## Stop service

    $ chmod +x stop_services.sh
    $ ./stop_services.sh