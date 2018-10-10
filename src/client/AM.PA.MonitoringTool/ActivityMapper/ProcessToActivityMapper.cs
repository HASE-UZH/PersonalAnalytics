// Created by André Meyer (ameyer@ifi.uzh.ch) from the University of Zurich
// Created: 2017-01-04
// 
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ActivityMapper
{
    /// <summary>
    /// This helper class is used to map a program to an activity
    /// category based on heuristics (defined as lists below)
    /// </summary>
    public static class ProcessToActivityMapper
    {
        #region Context Mapping Lists

        private static readonly List<string> EditorApps = new List<string> { "notepad", "xmlspy", "sublime", "emacs", "vim", "atom", "texteditor", "editplus", "gedit", "textpad" };
        private static readonly List<string> EditorNotEnoughInfoList = new List<string> { "go to...", "find", "untitled - notepad", "save", "speichern unter", "speichern", "suche", "replace", "ersetzen", "*new", "open", "reload", "new", "save as", "untitled", "no name", "plugin manager" }; // from notepad++

        private static readonly List<string> CodingApps = new List<string> { "rstudio", "sdv", "webplatforminstaller", "xts", "build", "cleardescribe", "clearfindco", "alm-client", "ildasm", "ssms", "mintty", "xming", "clearprojexp", "clearmrgman", "kitty", "bc2", "bcompare", "mobaxterm", "webmatrix", "mexplore", "linqpad", "android sdk manager", "windows phone application deployment", "ilspy", "tortoiseproc", "xsd", "eclipse", "fiddler", "xamarin", "netbeans", "intellij", "sql", "sqlitebrowser", "devenv", "visual studio", "code", "vs_enterprise", "vs2013", "microsoftazuretools", "webstorm", "phpstorm", "source insight", "zend", "console", "powershell", "shell", "cmd", "tasktop", "android studio", "ide", "filezilla", "flashfxp", "charles", "delphi32", "modelbrowser", "ibq", "pyscripter" };
        private static readonly List<string> CodeFileTypes = new List<string> { "src", ".proj", ".cmd", ".ps1", ".ini", ".ts", ".err", ".sql", ".ksh", ".dat", ".xaml", ".rb", ".kml", ".log", ".bat", ".cs", ".vb", ".py", ".xml", ".dtd", ".xs", ".h", ".cpp", ".java", ".class", ".js", ".asp", ".aspx", ".nuspec", ".css", ".html", ".htm", ".psm1", ".view", ".script", ".ccproj", "js", ".php", ".xhtml", ".sh", ".sln", ".vcxproj", ".pl", ".gitignore", ".exe", ".config" };
        private static readonly List<string> CodingDebugAppsAndKeywords = new List<string> { "vshost", "xde", "javaw", "debug", "bugger", "windbg", "perfview", "cdb", "msmunittest", "bug", "snoop" }; // works for visual studio, eclipse (if view changes)
        private static readonly List<string> CodingReviewApps = new List<string> { "codeflow", "gerrit", "stash", "kallithea", "code review", "rhodecode", "rietveld", "crucible", "phabricator" };
        private static readonly List<string> CodingVersionControlApps = new List<string> { "diff", "repository", "cleardiffbl", "cleardlg", "cleardiffmrg", "clearhistory", "clearvtree", "sourcetree", "svn", "tortoiseproc", "scm", "tfs", "push", "pull", "commit", "git", "bitbucket", "visual studio online", "thgw", "thg" };

        private static readonly List<string> EmailAppsAndKeywords = new List<string> { "mail", "outlook", "thunderbird", "outlook.com" }; // incudes gmail, yahoo mail, mac mail, outlook.com
        private static readonly List<string> PlanningAppsAndKeywords = new List<string> { "backlog", "winproj", "trello", "wunderlist", "todoist", "personalanalytics", "time tracking", "track time", "rescuetime", "clearquest", "scrum", "kanban", "codealike", "jira", "rally", "versionone", "calendar", "kalender", "sprint", "user story", "plan", "to-do", "todo", "task", "aufgabe", "vorgangsliste", "work item" };
        private static readonly List<string> ReadingWritingAppsAndKeywords = new List<string> { "aether", "insight3", "snagiteditor", "confluence", "picasa", "quora", "windows photo viewer", "visio", "flashmedialiveencoder", "photofiltre", "jmp", "treepad", "winword", "word", "leo", "translate", "übersetzer", "wordpress", "mspub", ".doc", ".xls", ".ppt", ".pub", "excel", "powerpnt", "onenote", "evernote", "acrord", "sharepoint", "pdf", "foxitreader", "adobe reader", "reader", "glcnd", "wiki", "keep", "google docs", "yammer", "docs", "office", "paint", "gimp", "photoshop", "lightroom", "miktex", "texmaker", "latex", "texstudio", "latech studio", "photo", "foto", "picpick", "greenshot" }; //not "note" as notepad is more coding
        private static readonly List<string> InstantMessagingAppsAndKeywords = new List<string> { "slack", "skype", "lync", "wechat", "sip", "g2mlauncher", "ciscowebexstart", "nbrplay", "g2mui", "chatter", "atmgr", "hangout", "viber", "messaging", "whatsapp", "messenger", "ctimon" }; // includes skype for business

        private static readonly List<string> BrowserApps = new List<string> { "iexplore", "chrome", "firefox", "opera", "safari", "applicationframehost", "edge" }; // ApplicationFrameHost stands for Edge
        private static readonly List<string> WorkUnrelatedBrowsingKeywords = new List<string> { "yelp", "instagram", "verge", "season", "food", "vincere", "agar.io", "gopro", "saldo", "halo", "book", "party", "swag", "birthday", "therapy", "vacation", "wohnung", "flat", "airbnb", "money", "hotel", "mietwagen", "rental", "credit", "hockeybuzz.com", "empatica", "wallpaper", "flight", "travel", "store", "phone", "buy", "engadget", "motorcycle", "car", "auto", "honda", "bmw", "nissan", "subaru", "winter", "summer", "bike", "bicycle", "arcgis", "finance", "portfolio", "toy", "gadget", "geek", "wellness", "health", "saturday", "sunday", "weekend", "sushi", "eat", "dessert", "restaurant", "holiday", "hotel", "cafe", "gas", "deal", "shop", "shopping", "craigslist", "vancouver", "indoor", "club", "loan", "maps", "flower", "florist", "valentine", "zalando", "tripadvisor", "golem", "tilllate", "heise", "jedipedia", "blick", "daydeal.ch", "renovero", "brack.ch", "skyscanner", "easyjet", "booking.com", "meteocheck", "scientific american", "ars technica", "national post", "sensecore", "core pro", "| time", "hockey inside/out", "netflix", "wired", "popular science", "habsrus", "flickr", "imdb", "xkcd", "derStandard.at", "amazon", "nhl.com", "20 minuten", "facebook", "reddit", "twitter", "google+", "news", "aktuell", "9gag", "yahoo", "comic", "ebay", "ricardo", "stream", "movie", "cinema", "kino", "tumblr" };
        private static readonly List<string> WorkRelatedBrowsingKeywords = new List<string> { "linkedin", "techready", "powerapps", "angular", "connect", "typescript", "release", "batmon", "calculator", "analytics", "azure", "power bi", "business", "googleearth", "php", "proffix", "centmin", "picturex", "ios", "schmelzmetall", "natur- und tierpark goldau", "tierpark", "amazon web service", "cyon", "salesforce.com", "silverlight", "issue", "junit", "mylyn", "jetbrains", "telerik", "testcomplete", "application lifecycle management", "all reports", "advanced search", ".net", "c#", "java", "vbforums", "dashboard", "virtualbox", "document", "dropbox", "onedrive", "proxy", "jenkins", "databasics", "suite", "abb", "shadowbot", "office", "windows", "namespace", "ventyx", "api", "apache", "oracle", "server", "system", "ibm", "code", "codeplex", "retrospection", "stack overflow", "msdn", "developer", "documentation", "blog", "coding", "programmer" };

        private static readonly List<string> FileNavigationExplorerApps = new List<string> { "explorer", "speedcommander", "q-dir" };

        private static readonly List<string> OtherMusicAppsAndKeywords = new List<string> { "groove", "zune", "itunes", "vlc", "music", "musik", "spotify", "wmplayer", "video", "youtube", "vimeo" };
        private static readonly List<string> OtherRdpApps = new List<string> { "rdcman", "mstsc", "vmconnect", "virtualbox", "vmware", "vpxclient", "msiexec", "pageant", "putty" };
        private static readonly List<string> OtherKeywords = new List<string> { "mmc", "dfsvc", "procmon64", "mousewithoutboarders", "vpnui", "dinotify", "perfmon", "agentransack", "lockapp", "searchui", "pwsafe", "wuauclt", "calc", "zip", "rar", "wwahost", "update", "avpui", "procexp64", "taskmgr", "pgp", "explorer", "groove", "dwm", "rstrui", "snippingtool", "onedrive", "settings", "einstellungen" };

        #endregion

        #region Context Mapping Logic

        /// <summary>
        /// Map a windowsactivity-entry (given process and window name) to 
        /// an activity category.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static ActivityCategory Map(string process, string window)
        {
            return GetCategory(process, window);
        }

        /// <summary>
        /// Mapping algorithm, which maps an activity to an activity
        /// category, based on the heuristcs (defined in the lists above).
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="windowName"></param>
        /// <returns></returns>
        private static ActivityCategory GetCategory(string processName, string windowName)
        {
            try
            {
                if (string.IsNullOrEmpty(processName) && string.IsNullOrEmpty(windowName))
                {
                    return ActivityCategory.Unknown;
                }

                windowName = windowName?.ToLower();
                processName = processName?.ToLower();


                // all IDLE, will later manually check with more info to find meetings, breaks, etc.
                if (processName != null && processName.Equals("idle"))
                {
                    return ActivityCategory.Idle;
                }

                // check with planning keywords
                if (IsCategory(ActivityCategory.Planning, processName, windowName))
                {
                    // this is needed because "task" could be mapped for the "task manager"
                    return IsCategory(ActivityCategory.Other, processName, windowName)
                        ? ActivityCategory.Other
                        : ActivityCategory.Planning;
                }
                // if not planning, check with email keywords
                if (IsCategory(ActivityCategory.Email, processName, windowName))
                {
                    return ActivityCategory.Email;
                }
                // if editor, might be reading/writing OR coding (if common coding file type extension or the window
                // title has not enough information to accurately map by hand, then map to coding category,
                // else: manual mapping until no longer mapping possible (then: map to ReadWriteDocument)
                if (IsEditor(processName))
                {
                    if (IsCodeFile(windowName))
                    {
                        return ActivityCategory.DevCode;
                    }
                    if (EditorNotEnoughInfo(windowName))
                    {
                        return ActivityCategory.ReadWriteDocument; // when we don't know
                    }
                    return ActivityCategory.DevCode; // should be manually coded, default: DevCode ActivityCategory.ManualEditor; // continue manual mapping
                }
                // check with debugging keywords (manual because of manual checking later)
                if (IsCategory(ActivityCategory.DevDebug, processName, windowName))
                {
                    return ActivityCategory.DevDebug;
                }
                // check with review keywords (manual because of manual checking later)
                if (IsCategory(ActivityCategory.DevReview, processName, windowName))
                {
                    return ActivityCategory.DevReview;
                }
                // check with version control keywords (manual because of manual checking later)
                if (IsCategory(ActivityCategory.DevVc, processName, windowName))
                {
                    return ActivityCategory.DevVc;
                }
                // check with coding keywords (there might be more from editors, manual mapping)
                if (IsCategory(ActivityCategory.DevCode, processName, windowName))
                {
                    return ActivityCategory.DevCode;
                }
                // check with read/write keywords (there might be more from editors, manual mapping)
                if (IsCategory(ActivityCategory.ReadWriteDocument, processName, windowName))
                {
                    return ActivityCategory.ReadWriteDocument;
                }

                // NO automated mapping of formal meetings: will look at self-reported tasks & IDLE times
                // NO automated mapping of in-formal meetings: will look at self-reported tasks & IDLE times

                // check with instant messaging keywords (subcategory of INFORMAL MEETING)
                if (IsCategory(ActivityCategory.InstantMessaging, processName, windowName))
                {
                    return ActivityCategory.InstantMessaging;
                }
                // check if its a browser (and it did not yet fit into other categories!)
                // then it's work related / unrelated web browsing which is manually mapped
                if (IsBrowser(processName))
                {
                    // map according to keywords and websites
                    if (IsWebsiteWorkRelated(windowName))
                    {
                        return ActivityCategory.WorkRelatedBrowsing;
                    }
                    if (IsWebsiteWorkUnrelated(windowName))
                    {
                        return ActivityCategory.WorkUnrelatedBrowsing;
                    }
                    if (IsCodeFile(windowName))
                    {
                        return ActivityCategory.DevCode;
                    }

                    // map remaining (manually)
                    return ActivityCategory.WorkRelatedBrowsing; // should be manually coded, default: work related
                }
                // check with file explorer (navigation) keywords
                if (IsCategory(ActivityCategory.FileNavigationInExplorer, processName, windowName))
                {
                    return ActivityCategory.FileNavigationInExplorer;
                }
                // check with music keywords (subcategory of Other)
                //if (IsCategory(ActivityCategory.OtherMusic, processName, windowName))
                //{
                //    return ActivityCategory.Other;
                //}
                // check with rdp keywords (subcategory of Other)
                if (IsCategory(ActivityCategory.OtherRdp, processName, windowName))
                {
                    return ActivityCategory.OtherRdp;
                }
                // check if it's something else (OS related, navigating, etc.)
                if (IsCategory(ActivityCategory.Other, processName, windowName))
                {
                    return ActivityCategory.Other;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("> ERROR while Mapping: {0}", e.Message);
            }

            return ActivityCategory.Unknown;
        }

        private static bool IsCategory(ActivityCategory category, string processName, string windowName)
        {
            var listToCheck = GetListForCategory(category);
            if (listToCheck == null) return false;
            return listToCheck.Any(processName.Contains) || listToCheck.Any(windowName.Contains);
        }

        // only look for process
        private static bool IsEditor(string processName) //, string windowName)
        {
            return EditorApps.Any(processName.ToLower().Contains); // || EditorApps.Any(windowName.Contains);
        }

        public static bool IsBrowser(string processName)
        {
            return BrowserApps.Any(processName.ToLower().Contains);
        }

        private static bool IsWebsiteWorkRelated(string windowName)
        {
            return WorkRelatedBrowsingKeywords.Any(windowName.ToLower().Contains);
        }

        private static bool IsWebsiteWorkUnrelated(string windowName)
        {
            return WorkUnrelatedBrowsingKeywords.Any(windowName.ToLower().Contains);
        }

        private static bool IsCodeFile(string windowName)
        {
            return CodeFileTypes.Any(windowName.ToLower().Contains);
        }

        private static bool EditorNotEnoughInfo(string windowName)
        {
            return string.IsNullOrEmpty(windowName) || EditorNotEnoughInfoList.Any(windowName.ToLower().Contains) || EditorApps.Any(windowName.Equals);
        }

        private static List<string> GetListForCategory(ActivityCategory cat)
        {
            switch (cat)
            {
                case ActivityCategory.DevCode:
                    return CodingApps;
                case ActivityCategory.DevDebug:
                    return CodingDebugAppsAndKeywords;
                case ActivityCategory.DevReview:
                    return CodingReviewApps;
                case ActivityCategory.DevVc:
                    return CodingVersionControlApps;
                case ActivityCategory.Email:
                    return EmailAppsAndKeywords;
                case ActivityCategory.Planning:
                    return PlanningAppsAndKeywords;
                case ActivityCategory.ReadWriteDocument:
                    return ReadingWritingAppsAndKeywords;
                case ActivityCategory.InstantMessaging:
                    return InstantMessagingAppsAndKeywords;
                case ActivityCategory.FileNavigationInExplorer:
                    return FileNavigationExplorerApps;
                case ActivityCategory.Other:
                    return OtherKeywords;
                //case ActivityCategory.OtherMusic:
                //    return OtherMusicAppsAndKeywords;
                case ActivityCategory.OtherRdp:
                    return OtherRdpApps;
            }
            return null;
        }

        #endregion
    }
}
