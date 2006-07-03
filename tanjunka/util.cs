#region Using directives

using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Globalization;
using System.Reflection;

#endregion

namespace Tanjunka
{
    class Util
    {
        static private int tempCounter;    // for temp filenames
        static private int processID;      // for temp filenames
        static private Dictionary<string, Regex> regexDict;
        static private string localDirectory;

        static Util()
        {
            Process currentProcess = Process.GetCurrentProcess();
            processID = currentProcess.Id;
            tempCounter = 0;

            localDirectory = "langs\\tanjunka-language-en-us.xml";
            if (!Util.FindLocalFile(ref localDirectory))
            {
                MessageBox.Show("Could not find language files!\n");
            }
            localDirectory = Path.GetDirectoryName(Path.GetDirectoryName(localDirectory));
        }

        static public string readFileToString(string filename)
        {
//            FileStream file = File.OpenRead(filename);
            StreamReader sr = new StreamReader(filename);
            string s = sr.ReadToEnd();
            sr.Close();
            return s;
        }

        static public byte[] readFile(string filename)
        {
            FileStream file = File.OpenRead(filename);
            int fileSize = (int)file.Length;

            byte[] buf = new byte[fileSize];

            file.Read(buf, 0, fileSize);
            file.Close();

            return buf;
        }

        static public void writeFile(string filename, byte[] buf)
        {
            FileStream file = File.OpenWrite(filename);
            file.Write(buf, 0, buf.Length);
            file.Close();
        }

        // I'm afraid to use File.Copy() because it might copy permissions
        // and I need the dstFile to be under my control
        static public void copyFile(string srcFilename, string dstFilename)
        {
            byte[] data = readFile(srcFilename);
            writeFile(dstFilename, data);

            // do I need to dispose of this data?
        }

        static public void deleteFileIfExists(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        // converts a character into %XXXX
        static private string ReplaceCharToEsc(Match m)
        {
            char c = m.Value[0];

            return String.Format("%{0:x4}", m.Value[0]);
        }

        // converts %XXXX to a character
        static private string ReplaceEscToChar(Match m)
        {
            char c = (char)Int32.Parse(m.Value.Substring(1), NumberStyles.HexNumber);
            string s = new string(c, 1);
            return s;
        }

        static private string UnescapeArrayString(string str)
        {
            Regex r = Util.MakeRegex("%\\w\\w\\w\\w", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return r.Replace(str, new MatchEvaluator(ReplaceEscToChar));
        }

        static private string EscapeArrayString(string str)
        {
            Regex r = Util.MakeRegex("\\W", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return r.Replace(str, new MatchEvaluator(ReplaceCharToEsc));
        }


        // converts a string "a|b|c|" to a list of strings ("a", "b", "c")
        // unescaping as neccesary
        static public List<string> StringToStringList(string str)
        {
            List<string> strs = new List<string>();

            Regex r = Util.MakeRegex("(?<1>.*?)\\|", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            MatchCollection mc = r.Matches(str);

            for (int ii = 0; ii < mc.Count; ii++)
            {
                Match m = mc[ii];

                strs.Add(UnescapeArrayString(m.Groups[1].ToString()));
            }

            return strs;
        }

        // converts an list of strings to ("a", "b", "c") to one string
        // "a|b|c|" escaping as neccesary
        static public string StringListToString(List<string> strs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in strs)
            {
                sb.Append(EscapeArrayString(str));
                sb.Append("|");
            }
            #if false
            List<string>?.Enumerator de = strs.GetEnumerator();
            while (de.MoveNext())
            {
                sb.Append(de.Current.Key);
                sb.Append("|");
            }
            #endif

            return (string)sb.ToString();
        }

        static public bool StringInStringList(string str, List<string> strs)
        {
            foreach (string listStr in strs)
            {
                if (String.Compare(str,listStr) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        static public String StringFromDate(DateTime date)
        {
            return String.Format("{0:d4}/{1:d2}/{2:d2}", date.Year, date.Month, date.Day);
        }

        static public DateTime DateFromString(string dstr)
        {
            if (dstr.Length > 0)
            {
                Regex r = Util.MakeRegex("(?<1>\\d+)/(?<2>\\d+)/(?<3>\\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Match m = r.Match(dstr);
                if (m.Groups.Count > 3)
                {
                    int year  = int.Parse(m.Groups[1].ToString());
                    int month = int.Parse(m.Groups[2].ToString());
                    int day   = int.Parse(m.Groups[3].ToString());

                    return new DateTime(year, month, day);
                }
                else
                {
                    throw new ApplicationException("wrong format for date");
                }
            }
            else
            {
                return DateTime.Now;
            }
        }

        // removes "" or '' around a string.  Assumes
        // if the string starts with " it ends with " etc.
        static public string RemoveQuotes(string str)
        {
            if (str[0] == '\'' || str[0] == '"')
            {
                str = str.Substring(1, str.Length - 2);
            }
            return str;
        }


        static public int SafeIntParse (string str)
        {
            int value = 0;

            try
            {
                value = int.Parse(str);
            }
            catch (Exception ex)
            {
                // don't care
                Log.Printn(ex.Message);
            }

            return value;
        }

        static public string SafeGetString(string str)
        {
            return (str != null) ? str : "";
        }

        static public string UnEntify(string html)
        {
            // replace common entities;
            html = Regex.Replace( html,  "&lt;"  , "<"  );
            html = Regex.Replace( html,  "&gt;"  , ">"  );
            html = Regex.Replace( html,  "&quot;", "\"" );
            html = Regex.Replace( html,  "&nbsp;", " "  );
            html = Regex.Replace( html,  "&amp;" , "&"  );
            html = Regex.Replace( html,  "Åc"    , "...");

            return html;
        }
        static public string StripHTML(string html)
        {
            Regex r = Util.MakeRegex("</*\\w+(?:\"(?:[^\"]*)\"|'(?:[^']*)'|[^>\"'])*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            html = r.Replace(html, "");

            return UnEntify(html);
        }

        static public string MergeURLPaths(string start, string end)
        {
            if (start.Length > 0 && end.Length > 0)
            {
                bool bStartHasSlash = (start[start.Length - 1] == '/' || start[start.Length - 1] == '\\');
                bool bEndHasSlash   = (end[0] == '/' || end[0] == '\\');

                if (bStartHasSlash && bEndHasSlash)
                {
                    return start + end.Substring(1);
                }
                else if (!bStartHasSlash && !bEndHasSlash)
                {
                    return start + "/" + end;
                }
            }
            return start + end;
        }

        static public bool FindLocalFile (ref string filename)
        {
            string work = filename;
            {
                for (int ii = 0; ii < 3; ++ii)
                {
                    if (File.Exists(work))
                    {
                        filename = Path.GetFullPath(work);
                        return true;
                    }
                    work = "../" + work;
                }

                // make sure we are checking the app path
                work = Path.Combine(Application.StartupPath, filename);
                if (File.Exists(work))
                {
                    filename = Path.GetFullPath(work);
                    return true;
                }
            }
            return false;
        }

        static private string loadSupportFile(string filename)
        {
            // we expect these files in the same folder as the executeable but
            // this little mess is basically so we can find it in the
            // project folder up from project/debug/bin
            //
            string formHTML;
            if (FindLocalFile(ref filename))
            {
                formHTML = Util.readFileToString(filename);
            }
            else
            {
                formHTML = "<html><body><table width='100%' height='100%'><tr valign='middle'><td align='center' style='color: red; font-weight: bold;'>could not find " + filename + "</td></tr></table></body></html>";
            }
            return formHTML;
        }

        class TextSub
        {
            string _currentFile;

            public TextSub(string currentFile)
            {
                _currentFile = currentFile;
            }

            public string ReplaceVars(Match m)
            {
                string var = m.Groups[1].ToString();
                if (m.Groups[2].Success)
                {
                    // it's func
                    string args = m.Groups[2].ToString();
                    if (var.CompareTo("include") == 0)
                    {
                        string incFilename = Path.Combine(Path.GetDirectoryName(_currentFile), args);
                        string str = Util.readFileToString(incFilename);

                        return LocalizeString(incFilename, str);
                    }
                    return "$" + var + "(" + args + ")";
                }
                else
                {
                    // it's a plain substitution
                    if (Localize.G.TextExists(var))
                    {
                        return Localize.G.GetText(var);
                    }
                    return "$" + var;
                }
            }
        }

        static public string LocalizeString (string filename, string str)
        {
            TextSub ts = new TextSub(filename);
            Regex r = Util.MakeRegex("\\$(?:(?<1>\\w+)\\((?<2>.*?)\\)|(?<1>\\w+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return r.Replace(str, new MatchEvaluator(ts.ReplaceVars));
        }

        static public void LocalizeFile (string srcFilename, string dstFilename)
        {
            string str = Util.readFileToString(srcFilename);

            str = LocalizeString(srcFilename, str);

            StreamWriter sw = new StreamWriter(dstFilename);
            sw.Write(str);
            sw.Close();
        }

        static public void SetFormHTML (WebBrowser wb, string filename)
        {
            if (Path.GetDirectoryName(filename).Length == 0)
            {
                filename = "forms\\" + filename;
                if (FindLocalFile(ref filename))
                {
                    // we cache tanjunka files
                    // the cached files have to be in the same folder otherwise we'd have to fix all the src= url(), etc.
                    string cacheFilename = Path.Combine(Path.Combine(GetLocalDirectory(), "cache"), GetVersion() + "_" + Path.GetFileName(filename));
                    bool bRemake = false;

                    // if the cache does not exists make it
                    if (!File.Exists(cacheFilename))
                    {
                        bRemake = true;
                    }
                    else
                    {

                        DateTime cacheTime  = File.GetLastWriteTime(cacheFilename);
                        DateTime srcTime    = File.GetLastWriteTime(filename);
                        DateTime setTime    = Localize.G.GetLanguageSetTime();
                        DateTime createTime = Localize.G.GetLanguageCreateTime();

                        bool bSrcNewer    = (srcTime.CompareTo(cacheTime)    > 0);
                        bool bSetNewer    = (setTime.CompareTo(cacheTime)    > 0);
                        bool bCreateNewer = (createTime.CompareTo(cacheTime) > 0);

                        if (bSrcNewer || bSetNewer || bCreateNewer)
                        {
                            bRemake = true;
                        }
                    }

                    if (bRemake)
                    {
                        LocalizeFile(filename, cacheFilename);
                    }

                    // use the cached one!
                    filename = cacheFilename;
                }
            }

            if (File.Exists(filename))
            {
                wb.Navigate(filename);
            }
            else
            {
                wb.DocumentText = "<html><body><table width='100%' height='100%'><tr valign='middle'><td align='center' style='color: red; font-weight: bold;'>could not find " + filename + "</td></tr></table></body></html>";
            }
        }

        static public string GetLocalDirectory()
        {
            return localDirectory;
        }

        static public void InitCache()
        {

            string cacheDir =  Path.Combine(GetLocalDirectory(), "cache");
            string formDir  =  Path.Combine(GetLocalDirectory(), "forms");

            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            string[] formFiles = Directory.GetFiles(formDir);
            foreach (string formFile in formFiles)
            {
                string dstFile = Path.Combine(cacheDir, Path.GetFileName(formFile));
                if (!File.Exists(dstFile) ||
                    File.GetLastWriteTime(formFile).CompareTo(File.GetLastWriteTime(dstFile)) > 0)
                {
                    copyFile(formFile, dstFile);
                }
            }
        }

        // save off regexes since making them is expensive
        static public Regex MakeRegex (string pattern, RegexOptions options)
        {
            if (regexDict == null)
            {
                regexDict = new Dictionary<string, Regex>();
            }

            if (regexDict.ContainsKey(pattern))
            {
                return regexDict[pattern];
            }

            Regex r = new Regex(pattern, options | RegexOptions.Singleline);
            regexDict[pattern] = r;

            return r;
        }

        // convert %dd to characters, change all non alnum to _
        static public string MakeFilenameSafe(string str)
        {
            //Regex r1 = new Regex("%\\d\\d", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //str = r1.Replace(str, "_");
            Regex r2 = Util.MakeRegex("[^\\w\\.]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            str = r2.Replace(str, "_");

            return str;
        }

        static public string CombineUnixPath(string part1, string part2)
        {
            int p1len = part1.Length;
            if (p1len > 0)
            {
                if (part1[p1len - 1] == '/' || part1[p1len - 1] == '\\')
                {
                    p1len -= 1;
                }
            }

            int p2start = 0;
            int p2len = part2.Length;

            if (p2len > 0)
            {
                if (part2[0] == '/' || part2[0] == '\\')
                {
                    p2start = 1;
                }
            }

            string sep = ((p1len > 0) && (p2len > 0)) ? "/" : "";
            string newpath = part1.Substring(0, p1len) + sep + MakeFilenameSafe(part2.Substring(p2start));

            return newpath.Replace('\\', '/');
        }


        // ---------------- utils -------------------------------------------------

        static public string GetTempPath()
        {
            string path = Path.Combine(Path.GetTempPath(), "tanjunka");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        static public string getTempFilename(string ext)
        {
            tempCounter++;
            return Path.Combine(Util.GetTempPath(), "tanjunka-" + processID.ToString() + "-" + tempCounter.ToString() + ext);
        }
        static public string changeExtension (string filename, string ext)
        {
            return Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ext);
        }

        static public string GetVersion ()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo FVI = FileVersionInfo.GetVersionInfo(assembly.Location);

            return String.Format("{0}.{1}.{2}", FVI.FileMajorPart, FVI.FileMinorPart, FVI.FileBuildPart, FVI.FilePrivatePart);
        }
    }

    public static class SerializationUtil
    {
       static public Version GetVersion(SerializationInfo info)
       {
          string assemblyName = info.AssemblyName;
          /* AssemblyName is in the form of "MyAssembly, Version=1.2.3.4,
                                  Culture=neutral,PublicKeyToken=null" */
          char[] separators = {',','='};
          string[] nameParts = assemblyName.Split(separators);
          return new Version(nameParts[2]);
       }
       //Rest of SerializationUtil
    }
}


