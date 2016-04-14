import sublime
import sublime_plugin
import time
import _thread


from MsSQLKit.pipe import *
from MsSQLKit.sqlView import *

class MSKITReturn(object):
    # Represent the Json value returned by the client 
    def __init__(self, errorId, message, data=None):
        self.errorId = errorId
        self.message = message
        self.data = data

    def serialize(self):
        return json.dumps({"errorId" : self.errorId, "message" : self.message, "data" : self.data})


# def set_console_message(view, text, show=False):
#     output = view.window().create_output_panel("MsSQLKit")
#     output.run_command(
#         'msk_output_console', {'position': output.size(), 'text': text + '\n'})
#     output.show(output.size())
#     if (show == True):
#         sublime.active_window().run_command(
#             "show_panel", {"panel": "output.MsSQLKit", "toggle": True})


class mskitInterface(object):
    def __init__(self):
        self.commandPipeName = '\\\\.\\pipe\\MsKitPipe'
        self.answerPipeName = '\\\\.\\pipe\\MsKitPipeAns'
        self.settings = sublime.load_settings('MsSQLKit.sublime-settings')
        self.isRunning = False
        self.views = {}
        self.alltype = []

    def __del(self):
        self.answerPipeHandle.close()
        self.commandPipeHandle.close()

    def getSqlView(self,tabId):
        if (str(tabId) in self.views and self.isRunning == True):
            return self.views[str(tabId)]
        else:
            return None

    def isViewReady(self,tabId):
        if (self.isRunning == False):
            return False
        v = self.getSqlView(tabId)
        if (v is not None and v.isConnected == True):
            return True
        return False

    def getSqlViewIfReady(self,tabId):
        if (self.isViewReady(tabId)):
            return self.getSqlView(tabId)

    def stop(self):
        for k, view in self.views.items():
            view.closeConnection()

        self.isRunning = False
        self.answerPipeHandle.close()
        self.commandPipeHandle.close()

    def processAnswer(self,answer):
        print(now())
        print(answer)
        if (answer["answerId"] == "ALIVE"):
            debug("MsSQLKit is up and running")
            self.isRunning = True

        elif (answer["answerId"] == "EXECRESULT"):
            view = self.views[str(answer["tabId"])].view
            if (answer["errorId"] != 0):
                view.set_console_message(answer["message"], True)
                view.set_status("mskit", "Error executing query")
            else:
                view.set_console_message(answer["message"], True)
                view.set_status("mskit", "Query executed successfully")

        elif (answer["answerId"] == "EXECTXTRESULT"):
            # TODO : Finish this. Render the table in text
            view = self.views[str(answer["tabId"])].view
            if (answer["errorId"] != 0):
                view.set_console_message(answer["message"], True)
                view.set_status("mskit", "Error executing query")
            else:
                view.set_status("mskit", "Query executed successfully")
                #set_console_message(view, answer["tableResults"], True)

        elif (answer["answerId"] == "RETREIVEMETADATA"):
            sqlView = self.getSqlView(answer["tabId"])
            sqlView.stop_waiting_loop("metadata")
            sqlView.metaDataBinding = True
            
        elif (answer["answerId"] == "STOP"):
            self.isRunning = False
            self.stop()
            return True
        return False

    def answerLoop(self):
        debug("Create server Handle")
        self.answerPipeHandle = pipeio(self.answerPipeName,"server")
        debug("Connect server Handle")
        self.answerPipeHandle.connect()
        while(True):
            debug("Process answer")
            try:
                m = json.loads(self.answerPipeHandle.getMessage())
                r = self.processAnswer(m)
                if (r==True):
                    break
            except pywintypes.error as e:
                if (e.winerror == winerror.ERROR_BROKEN_PIPE):
                    # MsSqlKit has ended
                    debug("Mskit closed?")

                    # for k, view in self.views.items():
                    #     view.closeConnection()

                    # self.isRunning = False
                    # self.answerPipeHandle.close()
                    # self.commandPipeHandle.close()
                    break
                else:
                    raise(e)

            # process m

    def startMskit(self):
        # TODO : Don't start if already running
        debug("start MsSQLKit")
        mskitPath = sublime.load_settings('MsSQLKit.sublime-settings').get("MskitPath", ".")
        subprocess.Popen(mskitPath + '\\mssqlkit.exe')

    def sendCommand(self, **kwargs):
        r = json.loads(self.commandPipeHandle.sendMessage(json.dumps(kwargs)))
        r["message"] = r["message"].replace('\r\n', '\n')
        return MSKITReturn(r["errorId"], r["message"], r["data"])

    def sendString(self, data):
        r = json.loads(self.commandPipeHandle.sendMessage(data))
        r["message"] = r["message"].replace('\r\n', '\n')
        return MSKITReturn(r["errorId"], r["message"], r["data"])

    def waitMskit(self,timeout):
        debug("Start wait")
        while (self.isRunning == False and timeout>0):
            timeout = timeout - 1;
            time.sleep(0.001)
        debug("End Wait")
        
    def run(self):
        print(self.isRunning)
        print(now()+" Launch answer loop")
        # run the answer message loop in his own thread
        #sublime.set_timeout_async(self.answerLoop, 0)
        _thread.start_new_thread(self.answerLoop, ())

        # start the Client
        self.startMskit()
        self.waitMskit(10000)
        self.commandPipeHandle = pipeio(self.commandPipeName,"client")


    def connectDb(self, view, connectionName, connectionString, tabId, filename):
        if (self.isRunning == False):
            self.run()
        

        r =  self.sendCommand(commandId="CONNECT", connectionString=connectionString, tabId=tabId, filename=filename)
        if (r.errorId==0):
            self.views[str(tabId)] = SqlView(view)
            self.views[str(tabId)].setConnection(connectionName, connectionString)
            db = self.executeScalar(tabId,"select db_name()").message
            self.views[str(tabId)].setDatabase(db)
        else:
            debug("error connecting: " + r.message)

        return r

    def executeScalar(self, tabId, sql):
        if (self.isViewReady(tabId)):
            r = self.sendCommand(
                commandId="EXECSCALAR", tabId=tabId, optionalMsgCount=1)
            return self.sendString(sql)

    def executeQuery(self, tabId, sql):
        if (self.isViewReady(tabId)):
            r = self.sendCommand(
                commandId="EXECSQL", tabId=tabId, optionalMsgCount=1)
            return self.sendString(sql)

    def executeQueryText(self, tabId, sql):
        if (self.isViewReady(tabId)):
            r = self.sendCommand(
                commandId="EXECTXTSQL", tabId=tabId, optionalMsgCount=1)
            return self.sendString(sql)

    def closeTab(self, tabId):
        if (self.isViewReady(tabId)):
            return self.sendCommand(commandId="CLOSETAB", tabId=tabId)

    def selectTab(self, tabId):
        if (self.isViewReady(tabId)):
            return self.sendCommand(commandId="SELECTTAB", tabId=tabId)

    def applyTheme(self, font_face , font_size, background, foreground, selection):
        if (self.isRunning == True):
            print("Apply theme")
            self.sendCommand(commandId="THEME", optionalMsgCount = 1)
            self.sendCommand(FontFace = font_face , FontSize = font_size, Background = background, Foreground = foreground, Selection = selection)

    def complete(self, tabId, sqltext, line, col):
        sqlView = self.getSqlViewIfReady(tabId)
        if (sqlView is not None):
            s = sqlView.get_text_content()
            self.sendCommand(commandId="COMPLETE", tabId=tabId, line=line, col=col, optionalMsgCount=1)
            return self.sendString(s)

    def retreiveMetaData(self, tabId):
        sqlView = self.getSqlViewIfReady(tabId)
        if (sqlView is not None):
            s = sqlView.get_text_content()

            sqlView.start_waiting_loop("metadata","Retreiving MetaData...")

            self.sendCommand(commandId="RETREIVEMETADATA", tabId=tabId, optionalMsgCount=1)
            return self.sendString(s)
