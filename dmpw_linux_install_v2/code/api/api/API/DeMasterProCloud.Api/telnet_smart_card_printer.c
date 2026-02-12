/* http://l3net.wordpress.com/2012/12/09/a-simple-telnet-client/ */

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>
#include <arpa/inet.h>
#include <termios.h>
#include <fcntl.h>

#define DO 0xfd
#define WONT 0xfc
#define WILL 0xfb
#define DONT 0xfe
#define CMD 0xff
#define CMD_ECHO 1
#define CMD_WINDOW_SIZE 31

void SendImage(int sock, fd_set fds, char *fileName);
void SaveImage(char *fileName);

static int g_sock = -1;

void negotiate(int sock, unsigned char *buf, int len)
{
    int i;

    if (buf[1] == DO && buf[2] == CMD_WINDOW_SIZE)
    {
        unsigned char tmp1[10] = {255, 251, 31};
        if (send(sock, tmp1, 3, 0) < 0)
            exit(1);

        unsigned char tmp2[10] = {255, 250, 31, 0, 80, 0, 24, 255, 240};
        if (send(sock, tmp2, 9, 0) < 0)
            exit(1);
        return;
    }

    for (i = 0; i < len; i++)
    {
        if (buf[i] == DO)
            buf[i] = WONT;
        else if (buf[i] == WILL)
            buf[i] = DO;
    }

    if (send(sock, buf, len, 0) < 0)
        exit(1);
}

static struct termios tin;

static void terminal_set(void)
{
    // save terminal configuration
    tcgetattr(STDIN_FILENO, &tin);

    static struct termios tlocal;
    memcpy(&tlocal, &tin, sizeof(tin));
    cfmakeraw(&tlocal);
    tcsetattr(STDIN_FILENO, TCSANOW, &tlocal);
}

static void terminal_reset(void)
{
    // restore terminal upon exit
    tcsetattr(STDIN_FILENO, TCSANOW, &tin);
    if(g_sock > 0)
        close(g_sock);
}

int sendCommand(int sock, fd_set *pFds, char *command)
{
    int cnt = 0;
    unsigned char buf[21];

    // printf("%s:%d\r\n", __FUNCTION__, __LINE__);

    fd_set fds;
    FD_ZERO(&fds);
    if (sock != 0)
        FD_SET(sock, &fds);
    FD_SET(0, &fds);

    while (FD_ISSET(0, &fds) == 0)
    {
        if (cnt++ > 3)
        {
            // perror("send timeout");
            printf("send timeout\r\n");
            return 1;
        }
    }

    if (send(sock, command, strlen(command), 0) < 0)
    {
        perror("send command error");
        return 1;
    }

    char pRecv[1024] = {
        0,
    };
    int nRecv = 0;
    while (1)
    {
        struct timeval ts;
        // ts.tv_sec = 1; // 1 second
        //     ts.tv_usec = 0;
        if(nRecv == 0)
        {
            ts.tv_sec = 3; // 1 second
            ts.tv_usec = 0;
        }
        else
        {
            ts.tv_sec = 0; // 1 second
            ts.tv_usec = 500 * 1000;
        }

        int nready = select(sock + 1, &fds, (fd_set *)0, (fd_set *)0, &ts);
        if (nready < 0)
        {
            perror("select. Error");
            return 1;
        }
        else if (nready == 0)
        {
            // printf("recv timeout\r\n");
            break;
        }
        else if (FD_ISSET(sock, &fds))
        {
            int rv = recv(sock, buf, 1, 0);
            int len;
            if (rv < 0)
            {
                perror("recv error");
                return 1;
            }
            else if (rv == 0)
            {
                printf("Connection closed by the remote end\n\r");
                return 1;
            }
            if (buf[0] == CMD)
            {
                // read 2 more bytes
                len = recv(sock, buf + 1, 2, 0);
                if (len < 0)
                    return 1;
                else if (len == 0)
                {
                    printf("Connection closed by the remote end\n\r");
                    return 0;
                }
                negotiate(sock, buf, 3);
            }
            else
            {
                pRecv[nRecv++] = buf[0];
                fflush(0);
            }
        }
    }

    printf("%s\r\n", pRecv);
    return 0;
}

#define BUFLEN 20
int main(int argc, char *argv[])
{
    struct sockaddr_in server;
    unsigned char buf[BUFLEN + 1];
    int len;
    int i;

    if (argc < 4)
    {
        printf("Usage: %s address [port] front_image back_image [file_image_stamp]\n", argv[0]);
        return 1;
    }
    int port = 23;
    if (argc >= 3)
        port = atoi(argv[2]);

    //Create socket
    g_sock = socket(AF_INET, SOCK_STREAM, 0);
    if (g_sock == -1)
    {
        perror("Could not create socket. Error");
        return 1;
    }

    server.sin_addr.s_addr = inet_addr(argv[1]);
    server.sin_family = AF_INET;
    server.sin_port = htons(port);

    //Connect to remote server
    if (connect(g_sock, (struct sockaddr *)&server, sizeof(server)) < 0)
    {
        perror("connect failed. Error");
        return 1;
    }
    puts("Connected...\n");

    // set terminal
    terminal_set();
    atexit(terminal_reset);

    struct timeval ts;
    ts.tv_sec = 1; // 1 second
    ts.tv_usec = 0;

    int cnt = 0;

    fd_set fds;
    while (1)
    {
        // select setup
        FD_ZERO(&fds);
        if (g_sock != 0)
            FD_SET(g_sock, &fds);
        FD_SET(0, &fds);

        // wait for data
        int nready = select(g_sock + 1, &fds, (fd_set *)0, (fd_set *)0, &ts);
        if (nready < 0)
        {
            perror("select. Error");
            return 1;
        }
        else if (nready == 0)
        {
            ts.tv_sec = 1; // 1 second
            ts.tv_usec = 0;
            break;
        }
        else if (g_sock != 0 && FD_ISSET(g_sock, &fds))
        {
            // start by reading a single byte
            int rv;
            if ((rv = recv(g_sock, buf, 1, 0)) < 0)
                return 1;
            else if (rv == 0)
            {
                printf("Connection closed by the remote end\n\r");
                return 0;
            }

            if (buf[0] == CMD)
            {
                // read 2 more bytes
                len = recv(g_sock, buf + 1, 2, 0);
                if (len < 0)
                    return 1;
                else if (len == 0)
                {
                    printf("Connection closed by the remote end\n\r");
                    return 0;
                }
                negotiate(g_sock, buf, 3);
            }
            else
            {
                len = 1;
                buf[len] = '\0';
                printf("%s", buf);
                fflush(0);
            }
        }
        else if (FD_ISSET(0, &fds))
        {
            buf[0] = getc(stdin); //fgets(buf, 1, stdin);
            if (send(g_sock, buf, 1, 0) < 0)
                return 1;
            if (buf[0] == '\n') // with the terminal in raw mode we need to force a LF
                putchar('\r');
            // break;
        }
    }
    
    sendCommand(g_sock, &fds, "OpenDevice\n");
    sendCommand(g_sock, &fds, "InitPage PORTRAIT\n");
    sendCommand(g_sock, &fds, "SetPage SIDE FRONT\n");
    sendCommand(g_sock, &fds, "SetPage EJECT NO\n");
    sendCommand(g_sock, &fds, "DrawImage color 0 0 636 1012 255 255 255 BEST\n");
    SendImage(g_sock, fds, argv[3]);
    
    // [COMMAND_CODE_PRINT_STAMP_FONT]
    
    sendCommand(g_sock, &fds, "print\n");

    sendCommand(g_sock, &fds, "InitPage PORTRAIT\n");
    sendCommand(g_sock, &fds, "SetPage SIDE BACK\n");
    sendCommand(g_sock, &fds, "DrawImage color 0 0 636 1012 255 255 255 BEST\n");
    SendImage(g_sock, fds, argv[4]);
    
    // [COMMAND_CODE_PRINT_STAMP_BACK]
    
    sendCommand(g_sock, &fds, "print\n");
    sendCommand(g_sock, &fds, "CloseDevice\n");
    sendCommand(g_sock, &fds, "Exit\n");
    close(g_sock);
    return 0;
}

void SaveImage(char *fileName)
{
    FILE *rfp = fopen(fileName, "rb"); 
    FILE *wfp = fopen("hexdump", "w"); 
    char c;
    for (int i = 0; 0 < fread(&c, 1, 1, rfp); i++) 
    {
        fprintf(wfp, "%02X", c & 0xFF);
        if (i % 32 == 31)
            fprintf(wfp, "\n");
    }
    fprintf(wfp, ".\n"); 
    fclose(wfp);         
    fclose(rfp);         
}

void SendImage(int sock, fd_set fds, char *fileName)
{
    FILE *fp = fopen(fileName, "rb");
    if (fp < 0)
    {
        perror("failed to open file");
        return;
    }

    // get file size
    fseek(fp, 0, SEEK_END);
    int size = ftell(fp);
    printf("%s file size %d\r\n", fileName, size);

    const int HEX_LENGTH = size * 2;
    const int CR = (size / 32) + 1;
    const int NUL = 1;
    const int DOT = 1;
    int dataSize = HEX_LENGTH + CR + NUL + DOT; // '\n' and '\0'

    char *data = malloc(dataSize);
    memset(data, 0, dataSize);

    int cnt = 0;
    int len = 0;
    fseek(fp, 0, SEEK_SET);
    while (1)
    {
        len = 0;
        for (len = 0; len < 32; len++)
        {
            char ch;
            int ret = fread(&ch, 1, 1, fp);
            if (ret <= 0)
                break;
            sprintf(data + cnt + (len * 2), "%02X", ch & 0xFF);
        }
        if (len != 32)
        {
            cnt += len * 2;
            data[cnt++] = '.';
            data[cnt++] = '\n';
            break;
        }

        cnt += len * 2;
        data[cnt++] = '\n';
    }
    fclose(fp);
    
    // printf("\nsize: %d, HEX_LENGTH: %d, CR: %d\r\n", size, HEX_LENGTH, CR);
    // printf("cnt: %d, dataSize: %d\r\n", cnt, dataSize);

    for(int i = 0 ; i < dataSize ; i += 65000)
    {
        sendCommand(sock, &fds, data + i);
    }

    free(data);
}