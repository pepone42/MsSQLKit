#import Pywin32.setup
import sublime
import sublime_plugin
import subprocess
import json


from plistlib import readPlistFromBytes

from MsSQLKit.mskitService import *

mskit = mskitInterface()

print("PluginLoaded")


def info(object, spacing=10, collapse=1):
    """Print methods and doc strings.

    Takes module, class, list, dictionary, or string."""
    methodList = [method for method in dir(
        object) if callable(getattr(object, method))]
    processFunc = collapse and (lambda s: " ".join(s.split())) or (lambda s: s)
    print("\n".join(["%s %s" %
                     (method.ljust(spacing),
                      processFunc(str(getattr(object, method).__doc__)))
                     for method in methodList]))


def get_filename(view):
    """ get the filename of the view or untiled_$id if it does not exist
        the filename is our key when we exchange data beetween the client and
        the server"""
    filename = view.file_name()
    if (filename is None and view.name() != ""):
        filename = str(view.id())+'_'+view.name()
    if (filename is None):
        filename = "Untilted_"+str(view.id())
    return filename


def get_settings(key, default_value=None):
    settings = sublime.load_settings('MsSQLKit.sublime-settings')
    return settings.get(key, default_value)


def get_connection_list():
    # build a connections list from the connections settings
    connections = get_settings("Connections")
    if connections is None:
        return None
    lists = []
    for con in connections:
        connextionString = 'Data Source='+con["Instance"]+';'
        if (con["Instegrated Security"] == True):
            connextionString += "integrated security=yes;"
        else:
            connextionString += "User Id=" + \
                con["User"]+";Password="+con["Password"]+";"
        if ("DefaultDatabase" in con):
            connextionString += 'Database='+con["DefaultDatabase"]+';'
        connextionString+='Asynchronous Processing=true;'
        lists.append([con["Name"], connextionString])
    return lists


def get_current_db(view):
    # get the current Database
    r = mskit.executeScalar(view.id(), 'select db_name()')
    return r.message


class msk_start_serverCommand(sublime_plugin.TextCommand):

    # Start Mssqlkit

    def run(self, edit):
        mskit.run()







def connect_to_database(view, connectionString, connectionName):
    r = mskit.connectDb(
        view, connectionName, connectionString, view.id(), get_filename(view))
    if (r.errorId != 0):
        view.set_console_message(r.message, True)
    else:
        view.set_console_message("", False)

        current_db = get_current_db(view)
        view.set_status("mskit_db", "Database: ["+current_db+"]")

        mskit.retreiveMetaData(view.id())


class msk_connectCommand(sublime_plugin.TextCommand):
    # Connect command

    connectionList = []

    def run(self, edit):
        self.connectionList = get_connection_list()
        if self.connectionList is not None:
            sublime.active_window().show_quick_panel(
                self.connectionList, self.on_chosen)
        else:
            # Todo : Add the possibility to input the connection manually
            view.set_status("mskit","There is no connections configured")

    def on_chosen(self, index):
        if (index >= 0):
            connect_to_database(
                self.view, self.connectionList[index][1], self.connectionList[index][0])


class msk_execute_queryCommand(sublime_plugin.TextCommand):
    # execute a sql query

    def run(self, edit):
        # if the current view has no connection attached, we ask for it
        sqlview = mskit.getSqlView(self.view.id())
        if (sqlview is None or sqlview.isConnected == False):
            self.view.run_command("msk_connect")
            return

        # get the first region selected, or all the file otherwise
        if self.view.sel()[0]:
            # Get the selected text
            s = self.view.substr(self.view.sel()[0])
        else:
            # Get all Region
            s = self.view.substr(sublime.Region(0, self.view.size()))
        r = mskit.executeQuery(self.view.id(), s)
        self.view.set_status("mskit", "Executing...")

ordering = {
'TableAlias': -1,
'BuiltInFunction':7,
'TableValuedFunction':6,
'Column':1,
'Database':2,
'Table':4,
'MultiTypeBuiltInFunction':8,
'View':5,
'Schema':3,
'IdentityColumn':0,
'StoredProcedure':9,
}

def sqlObjectSort(item):
    type = item[0].split('\t', 1)[1]
    if (type in ordering):
        return ordering[type]
    else:
        return 1000


class MyEvents(sublime_plugin.EventListener):
    # Events handler

    def on_post_window_command(self, window, command_name, args):
        # Update theme visual
        if (command_name == "set_user_setting" and args.get("setting") == "color_scheme" and get_settings("use_sublime_theme", False) != False):
            apply_theme(window.active_view())

    def on_close(self, view):
        # Tab Managment
        sqlview = mskit.getSqlView(view.id())
        if (sqlview is not None and sqlview.isConnected == True):
            mskit.closeTab(view.id())

    def on_activated_async(self, view):
        sqlview = mskit.getSqlView(view.id())
        if (sqlview is not None and sqlview.isConnected == True):
            mskit.selectTab(view.id())

    def on_query_completions(self, view, prefix, locations):
        # Competion management
        sqlview = mskit.getSqlView(view.id())
        if (sqlview is not None
                and sqlview.isConnected == True
                and sqlview.metaDataBinding == True):

            (row, col) = view.rowcol(locations[0])
            s = view.substr(sublime.Region(0, view.size()))
            # print("complete")
            r = mskit.complete(view.id(), s, row+1, col+1)

            if (r.data is None):
                return [], sublime.INHIBIT_WORD_COMPLETIONS

            # I like to have an ordering by type (column first etc.)
            l = sorted([(x, x.split('\t', 1)[0]) for x in r.data],key=sqlObjectSort)
            return l, sublime.INHIBIT_WORD_COMPLETIONS


def apply_theme(view):
    theme_settings = sublime.load_resource(view.settings().get('color_scheme'))
    theme = readPlistFromBytes(theme_settings.encode("utf-8"))
    mskit.applyTheme(view.settings().get('font_face'),
                     view.settings().get('font_size'),
                     theme["settings"][0]["settings"]["background"][:7],
                     theme["settings"][0]["settings"]["foreground"][:7],
                     theme["settings"][0]["settings"]["selection"][:7])
    # todo : Handle alpha for selection (by blending background and selection
    # color)


class msk_apply_themeCommand(sublime_plugin.TextCommand):

    def run(self, edit):
        apply_theme(view)


class msk_sphelp_objectCommand(sublime_plugin.TextCommand):

    def run(self, edit):
        sqlview = mskit.getSqlViewIfReady(view.id())
        if (sqlview is None):
            return
        if self.view.sel()[0] and len(self.view.sel()) == 1:
            # Get the selected text
            s = self.view.substr(self.view.sel()[0])

            # safe guard
            if ("'" in s):
                self.view.set_console_message("Invalid Identifier "+s, True)
                return
        r = mskit.executeQuery(self.view.id(), "sp_help '"+s+"'")

class msk_script_objectCommand(sublime_plugin.TextCommand):
    # script an object
    # TODO : script table and other object
    # TODO : script under cursor

    scirptObjectQuery = '''
    declare @script varchar(max)='',@objectname varchar(500)='$OBJECTNAME$'
    select @script +=def from (
    select
    'USE ['+db_name()+']
    GO
    /****** Object:  StoredProcedure ['+schema_name(schema_id)+'].['+name+']    Script Date: '+convert(varchar,getdate())+' ******/
    SET ANSI_NULLS ON
    GO
    SET QUOTED_IDENTIFIER ON
    GO
    '
    + OBJECT_DEFINITION(object_id) def from sys.objects where name = @objectname
    union all
    select 
    'GO
    /****** Object:  NumberedStoredProcedure ['+schema_name(schema_id)+'].['+a.name+'];'+convert(varchar,b.procedure_number)+'    Script Date: '+convert(varchar,getdate())+' ******/
    SET ANSI_NULLS ON
    GO
    SET QUOTED_IDENTIFIER ON
    GO
    ' 
    + b.definition from sys.objects a
    join sys.numbered_procedures b on a.object_id=b.object_id
    where a.name = @objectname
    ) a 
    select @script'''

    def run(self, edit):
        sqlview = mskit.getSqlView(self.view.id())
        if (sqlview is None or sqlview.isConnected == False):
            return

        if self.view.sel()[0] and len(self.view.sel()) == 1:
            # Get the selected text
            s = self.view.substr(self.view.sel()[0])

            # safe guard
            if ("'" in s):
                self.view.set_console_message("Invalid Identifier "+s, True)
                return
            query = self.scirptObjectQuery.replace("$OBJECTNAME$", s)
            r = mskit.executeScalar(self.view.id(), query)

            if (r.message != ""):
                v = sublime.active_window().new_file()
                sublime.active_window().focus_view(v)
                v.run_command(
                    'msk_output_console', {'position': v.size(), 'text': r.message})
                v.set_syntax_file(self.view.settings().get('syntax'))
                v.set_name(s+'.sql')
                connect_to_database(v, sqlview.connectionString,sqlview.connectionName)
            else:
                self.view.set_console_message("Identifier "+s+" not found", True)



