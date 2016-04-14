import Pywin32.setup

import win32pipe
import win32file
import win32api
import winerror
import pywintypes
import json
import os
import stat
import subprocess



from datetime import datetime

def now():
    return datetime.utcnow().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3]

def debug(msg):
    print(now()+" : " + msg)


class pipeio(object):

    def __init__(self,name, type="server"):
        self.pipeName = name
        self.type = type
        if (type == "server"):
            self.pipeHandle = win32pipe.CreateNamedPipe(self.pipeName,
                                                        win32pipe.PIPE_ACCESS_DUPLEX,
                                                        win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_WAIT,
                                                        1,
                                                        1000000,
                                                        1000000,
                                                        50,
                                                        None)
        elif (type == "client"):
            self.pipeHandle = win32file.CreateFile(self.pipeName,
                                             win32file.GENERIC_READ | win32file.GENERIC_WRITE,
                                             0, None,
                                             win32file.OPEN_EXISTING,
                                             0, None)
        else:
            raise("Unknow param")

    def __del__(self):
        try:
            win32file.WriteFile(self.pipeHandle,  b'')  # try to clear up anyone waiting
        except (pywintypes.error, win32pipe.error):  # no one's listening
            pass
        self.close()

    def __exit__(self):
        self.__del__()

    def connect(self):
        win32pipe.ConnectNamedPipe(self.pipeHandle, None)

    def close(self):
        if(self.type == "server"):
            try:
                win32pipe.DisconnectNamedPipe(self.pipeHandle)
            except (pywintypes.error, win32pipe.error):
                pass
        win32file.CloseHandle(self.pipeHandle)

    def sendMessage(self,message):
        win32file.WriteFile(self.pipeHandle,  bytes(message, 'utf-8'))
        r = win32file.ReadFile(self.pipeHandle, 1000000, None)
        return r[1].decode("utf-8")

    def getMessage(self):
        r = win32file.ReadFile(self.pipeHandle, 1000000, None)
        message = r[1].decode("utf-8")
        win32file.WriteFile(self.pipeHandle,  bytes('{"errorId" : 0, "message" : "OK"}', 'utf-8'))
        return message

