import sublime
import sublime_plugin
import time
import _thread


class msk_output_consoleCommand(sublime_plugin.TextCommand):

    def run(self, edit, position, text):
        # Command to output text on the outut console
        self.view.insert(edit, position, text)

#class View(sublime.View):

def set_console_message(self, text, show=False):
    output = self.window().create_output_panel("MsSQLKit")
    output.run_command(
        'msk_output_console', {'position': output.size(), 'text': text + '\n'})
    output.show(output.size())
    if (show == True):
        sublime.active_window().run_command(
            "show_panel", {"panel": "output.MsSQLKit", "toggle": True})

sublime.View.set_console_message = set_console_message

class SqlView(object):

    def __init__(self,view):
        self.view=view
        self.isConnected = False
        self.metaDataBinding = False

    def __del__(self):
        self.closeConnection()

    def __exit__(self):
        self.__del__()

    def setConnection(self,connectionName,connectionString):
        self.connectionName = connectionName
        self.connectionString = connectionString
        self.isConnected = True
        self.view.set_status("mskit", "Connected")
        self.view.set_status("mskit_connectionName", self.connectionName)

    def setDatabase(self,database):
        if (self.isConnected == True):
            self.view.set_status("mskit_db",database)

    def closeConnection(self):
        self.connectionName = None
        self.connectionString = None
        self.view.erase_status("mskit")
        self.view.erase_status("mskit_connectionName")
        self.view.erase_status("mskit_db")
        self.isConnected = False


    def get_text_content(self):
        return self.view.substr(sublime.Region(0, self.view.size()))

    def waiting_loop(self,waitingId,message):

        loopStr = [
            "[=    ]",
            "[ =   ]",
            "[  =  ]",
            "[   = ]",
            "[    =]",
            "[   = ]",
            "[  =  ]",
            "[ =   ]",
        ]
        i = 0
        while self.view.get_status("mskit_"+waitingId)!="STOP" and self.isConnected == True:
            if (i>=len(loopStr)):
                i=0
            self.view.set_status("mskit_"+waitingId,message+" "+loopStr[i])
            i = i + 1
            time.sleep(0.1)
        self.view.erase_status("mskit_"+waitingId)

    def start_waiting_loop(self,waitingId,message):
        _thread.start_new_thread(self.waiting_loop, (waitingId,message,))

    def stop_waiting_loop(self,waitingId):
        self.view.set_status("mskit_"+waitingId,"STOP")
