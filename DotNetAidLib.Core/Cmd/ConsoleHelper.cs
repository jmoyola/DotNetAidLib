using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using DotNetAidLib.Core.Collections;
using DotNetAidLib.Core.Develop;
using DotNetAidLib.Core.Helpers;
using DotNetAidLib.Core.Serializer;

namespace DotNetAidLib.Core.Cmd
{
    public class ConsoleHelper
    {

        public static bool GetFile(String label, out String outValue){
            return GetFile(label, out outValue, null);
        }

        public static bool GetFile(String label, out String outValue, String defaultPath){
            if(TryParse<System.IO.FileInfo>.Parse(defaultPath, v=>new System.IO.FileInfo(v.ToString())).IsValid)
                return InputField(label, out outValue, (v => {
                    System.IO.FileInfo fi = new System.IO.FileInfo(v);
                    if (!fi.Exists)
                        throw new Exception("File don't exists.");
                    return fi.FullName;
                }), new Optional<string>(defaultPath), v => v.Message);
            else
                return InputField(label, out outValue, (v => {
                    System.IO.FileInfo fi = new System.IO.FileInfo(v);
                    if (!fi.Exists)
                        throw new Exception("File don't exists.");
                    return fi.FullName;
                }), null, v => v.Message);

        }

        public static bool GetDirectory(String label, out String outValue)
        {
            return GetDirectory(label, out outValue, null);
        }

        public static bool GetDirectory(String label, out String outValue, String defaultPath)
        {
            if (TryParse<DirectoryInfo>.Parse(defaultPath, v => new DirectoryInfo(v.ToString())).IsValid)
                return InputField<String>(label, out outValue, (v => {
                    DirectoryInfo fi = new DirectoryInfo(v);
                    if (!fi.Exists)
                        throw new Exception("Directory don't exists.");
                    return fi.FullName;
                }), new Optional<string>(defaultPath), v=>v.Message);
            else
                return InputField<String>(label, out outValue, (v => {
                    DirectoryInfo fi = new DirectoryInfo(v);
                    if (!fi.Exists)
                        throw new Exception("Directory don't exists.");
                    return fi.FullName;
                }), null, v => v.Message);

        }

        public static DirectoryInfo SelectDirectory(String caption, DirectoryInfo baseFolder, String searchPattern){
            return SelectDirectory(caption, baseFolder, searchPattern, null);
        }

        public static DirectoryInfo SelectDirectory(String caption, DirectoryInfo baseFolder, String searchPattern, DirectoryInfo defaultSelection) {
            return (DirectoryInfo)SelectPath(caption, baseFolder, searchPattern, null, defaultSelection);
        }

        public static FileInfo SelectFile(String caption, DirectoryInfo baseFolder, String searchPattern){
            return SelectFile(caption, baseFolder, searchPattern, null);
        }

        public static FileInfo SelectFile(String caption, DirectoryInfo baseFolder, String searchPattern, System.IO.FileInfo defaultSelection){
            return (System.IO.FileInfo)SelectPath(caption, baseFolder, null, searchPattern, defaultSelection);
        }

        private static FileSystemInfo SelectPath(String caption, DirectoryInfo baseFolder, String directoriesSearchPattern, string filesSearchPattern, FileSystemInfo defaultSelection)
        {
            int selected = -1;
            String aux;
            List<FileSystemInfo> files=new List<FileSystemInfo>();

            do
            {
                Console.WriteLine();
                Console.WriteLine(caption);
                if (!String.IsNullOrEmpty(directoriesSearchPattern))
                    files.AddRange(baseFolder.GetDirectories(directoriesSearchPattern).OrderBy(v=>v.Name));
                
                if (!String.IsNullOrEmpty(filesSearchPattern))
                    files.AddRange(baseFolder.GetFiles(filesSearchPattern).OrderBy(v=>v.Name));

                for (int i = 0; i < files.Count; i++){
                    FileSystemInfo o = files[i];
                    String line = " " + (i + 1);

                    if (defaultSelection!=null && o.Equals(defaultSelection))
                        line = line + ".>";
                    else
                        line = line + ". ";

                    if (o is DirectoryInfo)
                        line+= "[" + o.Name + "]";
                    else
                        line += o.Name;

                    Console.WriteLine(line);
                }

                Console.WriteLine("Select item (0 or ESC for cancel" + (defaultSelection!=null ? ", ENTER for default '" + defaultSelection.ToString()+"'" : "") + "): ");

                if (!ReadLineESC(out aux))
                    return null;

                if (defaultSelection!=null && String.IsNullOrEmpty(aux))
                    return defaultSelection;

            } while (!Int32.TryParse(aux, out selected) && (selected > -1 && selected < (files.Count+1)));

            if (selected > 0)
                return files[selected - 1];
            else
                return null;
        }

        public static bool SelectItemList<T>(String caption, IList<T> items, out T selectedItem, bool enableDefaultItem, T defaultItem)
        {
            return SelectItemList<T>(caption, items, out selectedItem, (v => v.ToString()), enableDefaultItem, defaultItem);
        }

        public static bool SelectItemList<T>(String caption, IList<T> items, out T selectedItem)
        {
            return SelectItemList<T>(caption, items, out selectedItem, (v => v.ToString()), false, default(T));
        }

        public static bool SelectItemList<T>(String caption, IList<T> items, out T selectedItem, Func<T, String> toStringFunction) {
            return SelectItemList<T>(caption, items, out selectedItem, toStringFunction, false, default(T));
        }

        public static bool SelectItemList<T>(String caption, IList<T> items, out T selectedItem, Func<T, String> toStringFunction, bool enableDefaultItem, T defaultItem, bool includeDefaultItemIfNotExists = false)
        {
            selectedItem = default(T);
            int selected = -1;
            String aux;

            if (enableDefaultItem && !items.Contains(defaultItem)) { 
                if (includeDefaultItemIfNotExists)
                    items.Add(defaultItem);
                //else
                //    enableDefaultItem = false;
            }

            do
            {
                Console.WriteLine();
                Console.WriteLine(caption);
                for (int i = 0; i < items.Count; i++)
                {
                    String line = " " + (i+1);
                    if (enableDefaultItem && items[i].Equals(defaultItem))
                        line = line + ".[" + toStringFunction.Invoke(items[i]) + "]";
                    else
                        line = line + ". " + toStringFunction.Invoke(items[i]);
                    Console.WriteLine(line);
                }
                Console.WriteLine("Select item (0 or ESC for cancel"+ (enableDefaultItem?", ENTER for default":"") +"): ");

                if (!ReadLineESC(out aux))
                    return false;

                if (enableDefaultItem && String.IsNullOrEmpty(aux)) { 
                    selectedItem = defaultItem;
                    return true;
                }

            } while (!Int32.TryParse(aux, out selected) && (selected > -1 && selected < (items.Count+1)));

            if (selected > 0){
                selectedItem = items[selected - 1];
                return true;
            }
            else
                return false;
        }

        public class ConsoleKeyEvent
        {
            private ConsoleKeyInfo keyInfo;
            private IList<char> buffer;
            private int cursorPosition;
            private bool cancel = false;

            public ConsoleKeyEvent(ConsoleKeyInfo keyInfo, IList<char> buffer, int cursorPosition)
            {
                Assert.NotNull(buffer, nameof(buffer));
                Assert.NotNull(keyInfo, nameof(keyInfo));
                
                this.keyInfo = keyInfo;
                this.buffer = buffer;
                this.cursorPosition = cursorPosition;
            }

            public bool Cancel
            {
                get => cancel;
                set => cancel = value;
            }

            public int CursorPosition
            {
                get=>cursorPosition;
                set=>cursorPosition=value;
            }
            
            public IList<char> Buffer => buffer;

            public ConsoleKeyInfo KeyInfo => keyInfo;
        }

        public static bool ReadLineESC(out String text, Action<ConsoleKeyEvent> keyEvent=null)
        {
            List<char> buffer = new List<char>();
            int curPos = 0;
            
            String ret=null;
            ConsoleKeyInfo key;
            do
            {
                if (Console.KeyAvailable)
                {
                    key = Console.ReadKey(false);
                    if (keyEvent != null)
                    {
                        ConsoleKeyEvent ke = new ConsoleKeyEvent(key, buffer, curPos);
                        keyEvent.Invoke(ke);
                        if(ke.Cancel)
                            continue;
                        curPos = ke.CursorPosition;
                    }

                    if (key.Key == ConsoleKey.Escape)
                    {
                        text = null;
                        return false;
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        text = ret;
                        break;
                    }
                    else
                    {
                        ret += key.KeyChar;
                    }
                }
            } while (true);

            return true;
        }

        public static bool ReadLineBuffer(out String text, Action<ConsoleKeyEvent> keyEvent=null)
        {
            List<char> buffer = new List<char>();
            int curPos = 0;
            bool ret = false;
            bool insert = true;
            
            text = null;
            
            String lastText = "";
            
            ConsoleKeyInfo key;
            do
            {
                if (Console.KeyAvailable)
                {
                    key = Console.ReadKey(true);
                    if (keyEvent != null)
                    {
                        ConsoleKeyEvent ke = new ConsoleKeyEvent(key, buffer, curPos);
                        keyEvent.Invoke(ke);
                        if(ke.Cancel)
                            continue;
                        curPos = ke.CursorPosition;
                    }

                    if (key.Key == ConsoleKey.Escape)
                        break;
                    else if (key.Key == ConsoleKey.Insert)
                    {
                        insert = !insert;
                        if (!insert)
                            Console.CursorSize = 5;
                        else
                            Console.CursorSize = 50;
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        ret = true;
                        break;
                    }
                    else if (key.Key == ConsoleKey.Backspace && curPos > 0)
                    {
                        buffer.RemoveAt(curPos-1);
                        curPos--;
                    }
                    else if (key.Key == ConsoleKey.Delete && curPos < buffer.Count)
                        buffer.RemoveAt(curPos);
                    else if (key.Key == ConsoleKey.LeftArrow && curPos > 0)
                        curPos--;
                    else if (key.Key == ConsoleKey.RightArrow && curPos < buffer.Count)
                        curPos++;
                    else if(key.KeyChar >= 32) // Caracteres imprimibles
                    {
                        if (insert || buffer.Count==0)
                            buffer.Insert(curPos, key.KeyChar);
                        else
                            buffer[curPos] = key.KeyChar;
                        curPos++;
                    }
                    Debug.WriteLine("Buffer(" + curPos + "): " + buffer.Select((v,i)=>(i==curPos?"|":"")+v).ToStringJoin());
                    String newText = new String(buffer.ToArray());
                    Console.CursorLeft = 0;
                    if(newText.Length>lastText.Length)
                        Console.Write(newText);
                    else
                        Console.Write(newText + new String(' ', lastText.Length-newText.Length));
                    Console.CursorLeft = curPos;
                    lastText = newText;
                }

                
            } while (true);

            text = new String(buffer.ToArray());
            
            return ret;
        }
        public class ReadKeyEventArgs : EventArgs {
            private ConsoleKeyInfo keyInfo;
            private String text=null;
            private bool cancel = false;

            public ConsoleKeyInfo KeyInfo { get => keyInfo; set => keyInfo = value; }
            public string Text { get => text; set => text = value; }
            public bool Cancel { get => cancel; set => cancel = value; }
        }

        public static bool ReadLineESC(out String text, Action<ReadKeyEventArgs> readKeyEventHandler)
        {
            ReadKeyEventArgs arg = new ReadKeyEventArgs();
            do
            {
                if (Console.KeyAvailable)
                {
                    arg.KeyInfo=Console.ReadKey(false);
                    arg.Cancel = false;

                    readKeyEventHandler.Invoke(arg);

                    if (!arg.Cancel)
                    {
                        if (arg.KeyInfo.Key == ConsoleKey.Escape)
                        {
                            text = null;
                            return false;
                        }
                        else if (arg.KeyInfo.Key == ConsoleKey.Enter)
                        {
                            text = arg.Text;
                            break;
                        }
                        else
                        {
                            arg.Text += arg.KeyInfo.KeyChar;
                        }
                    }
                }
            } while (true);

            return true;
        }

        public class KeyLineInfo {
            private bool cancel;
            private TimeSpan time;
            private ConsoleKeyInfo key;
            private IList<char> buffer= new List<char>();
            private int index;
            private int pageWidth;
            private bool insertMode;

            public bool Cancel { get => cancel; set => cancel = value; }
            public ConsoleKeyInfo Key { get => key; set => key = value; }
            public IList<char> Buffer { get => buffer;}
            public int Index { get => index; set => index = value; }
            public TimeSpan Time { get => time; set => time = value; }
            public int PageWidth { get => pageWidth; set => pageWidth = value; }
            public bool InsertMode { get => insertMode; set => insertMode = value; }

            public override string ToString()
            {
                return time.ToString(@"s\.fff") +"(" + index + "/" + buffer.Count + ")" + (insertMode ?"I":"O") +"[" + (key.Modifiers>0?key.Modifiers.ToStringFlags(false)+"+":"") + key.Key.ToString() + (key.KeyChar>0?"'" + key.KeyChar + "'":"") + "]: " + buffer.ToStringJoin("");
            }
        }

        public static bool ReadLine(out String text, bool echo = true, int timeoutSeconds = 0, int width = 0, Func<KeyLineInfo, bool> keyEventHandler = null)
        {
            ConsoleKey cancelKey = ConsoleKey.Escape;
            ConsoleKey acceptKey = ConsoleKey.Enter;
            bool ret = false;
            text = null;
            KeyLineInfo keyEvent = new KeyLineInfo()
            {
                Index = 0,
                PageWidth = 0,
                InsertMode = true,
                Cancel = false
            };

            if (width <0)
                throw new Exception("Width must be zero (auto) or page width.");

            Console.CancelKeyPress += (sender, e) => keyEvent.Cancel = true;

            DateTime now = DateTime.Now;
            int cursorX = Console.CursorLeft;
            do
            {
                if (keyEvent.Cancel
                    || (timeoutSeconds>0 && DateTime.Now.Subtract(now).TotalSeconds > timeoutSeconds))
                {
                    keyEvent.Buffer.Clear();
                    ret = false;
                    break;
                }

                if (Console.KeyAvailable)
                {
                    bool discard = false;

                    DateTime keyPressTime = DateTime.Now;
                    keyEvent.Time =  keyPressTime.Subtract(now);
                    now = keyPressTime;

                    keyEvent.Key = Console.ReadKey(true);

                    if (keyEventHandler != null)
                        discard=!keyEventHandler.Invoke(keyEvent);

                    if (!discard)
                    {
                        if (keyEvent.Key.Key == cancelKey)
                        {
                            keyEvent.Cancel = true;
                        }
                        else if (keyEvent.Key.Key == ConsoleKey.Insert)
                        {
                            keyEvent.InsertMode = !keyEvent.InsertMode;
                            if (keyEvent.InsertMode)
                                Console.CursorSize = 1;
                            else
                                Console.CursorSize = 100;
                        }
                        else if (keyEvent.Key.Key == acceptKey)
                        {
                            ret = true;
                            text = keyEvent.Buffer.ToStringJoin("");
                            break;
                        }
                        else if (keyEvent.Key.Key == ConsoleKey.Delete)
                        {
                            if (keyEvent.Index < keyEvent.Buffer.Count)
                                keyEvent.Buffer.RemoveAt(keyEvent.Index);
                        }
                        else if (keyEvent.Key.Key == ConsoleKey.Backspace)
                        {
                            if (keyEvent.Index > 0)
                            {
                                keyEvent.Buffer.RemoveAt(keyEvent.Index - 1);
                                keyEvent.Index--;
                            }
                        }
                        else if (keyEvent.Key.Key == ConsoleKey.LeftArrow)
                        {
                            if (keyEvent.Index > 0)
                                keyEvent.Index--;
                        }
                        else if (keyEvent.Key.Key == ConsoleKey.RightArrow)
                        {
                            if (keyEvent.Index < keyEvent.Buffer.Count)
                                keyEvent.Index++;
                        }
                        else if (keyEvent.Key.Key == ConsoleKey.Home)
                        {
                            keyEvent.Index = 0;
                        }
                        else if (keyEvent.Key.Key == ConsoleKey.End)
                        {
                            keyEvent.Index = keyEvent.Buffer.Count;
                        }
                        else
                        {
                            if (keyEvent.Key.KeyChar > 31)
                            {
                                // Si estoy en modo inserción o en modo sustitución pero al final
                                if (keyEvent.InsertMode || (!keyEvent.InsertMode && keyEvent.Index == keyEvent.Buffer.Count))
                                    keyEvent.Buffer.Insert(keyEvent.Index, keyEvent.Key.KeyChar);
                                else
                                    keyEvent.Buffer[keyEvent.Index] = keyEvent.Key.KeyChar;

                                keyEvent.Index++;
                            }
                        }


                        if (echo)
                        {
                            // Si se especificó ancho y hay suficiente ancho para mostrarlo
                            if (width > 0 && (Console.WindowWidth - cursorX) >= width)
                                keyEvent.PageWidth = width;
                            else // Si no se especificó ancho o no hay suficiente ancho para mostrarlo
                                keyEvent.PageWidth = Console.WindowWidth - cursorX-1;

                            Console.CursorLeft = cursorX;

                            int pageIndex = keyEvent.Index / keyEvent.PageWidth;
                            int pageIndexIni = pageIndex * keyEvent.PageWidth;
                            Console.Write(keyEvent.Buffer.Skip(pageIndexIni).Take(keyEvent.PageWidth).ToStringJoin("").PadRight(keyEvent.PageWidth));

                            int cursorLeft = keyEvent.Index - pageIndexIni;

                            Console.CursorLeft = cursorX + cursorLeft;
                        }
                        Debug.WriteLine(keyEvent.ToString());
                    }
                }

            } while (true);


            return ret;
        }

        public static bool KeyPressed(out ConsoleKeyInfo keyInfo)
        {

            if (Console.KeyAvailable)
            {
                keyInfo = Console.ReadKey(true);
                return true;
            }
            else
            {
                keyInfo = default(ConsoleKeyInfo);
                return false;
            }
        }

        public static ConsoleKeyInfo ReadKey()
        {
            while (!Console.KeyAvailable);
            return Console.ReadKey(true);
        }

        public static bool InputField<T>(String label, out T outValue){
            return InputField<T>(label, out outValue, (v=>v.Cast<T>()), Optional<T>.FromDefault());
        }

        public static bool InputField<T>(String label, out T outValue, Func<String, T> castFunction){
            return InputField<T>(label, out outValue, castFunction, Optional<T>.FromDefault());
        }

        public static bool BinaryQuestion(String label) {
            ConsoleKeyInfo keyInfo;
            do
            {
                Console.WriteLine(label + " (ENTER: confirm, ESC: cancel): ");
                keyInfo = ConsoleHelper.ReadKey();
                if (keyInfo.Key == ConsoleKey.Enter || keyInfo.Key == ConsoleKey.Escape)
                    return keyInfo.Key == ConsoleKey.Enter;
            } while (true);
        }

        public static bool InputField<T>(String label, out T outValue, Func<String, T> castFunction, Optional<T> defaultValue)
        {
            outValue = default(T);
            return InputField<T>(label, out outValue, castFunction, defaultValue, null);
        }

        public static bool InputField<T>(String label, out T outValue, Func<String, T> castFunction, Optional<T> defaultValue, Func<Exception, String> onErrorMessage)
        {
            String aux = null;
            outValue = default(T);

            if (defaultValue == null)
                defaultValue = new Optional<T>();

            do
            {
                Console.Write(label + (defaultValue.HasValue? " [" + (defaultValue.Value==null?"":defaultValue.Value.ToString()) + "]": "") + ": ");

                if (!ConsoleHelper.ReadLineESC(out aux))
                    return false;

                if (defaultValue.HasValue && aux == null){
                    outValue = defaultValue;
                    return true;
                }
                else
                {
                    try
                    {
                        outValue = castFunction(aux);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if(onErrorMessage!=null)
                            Console.WriteLine(onErrorMessage.Invoke(ex));
                        else
                            Console.WriteLine(ex.Message);
                    }
                }
            } while (true);
        }
        
        public static bool InputKeyValue(String label, IDictionary<String, Object> values, IStringParser stringParser)
        {
            Assert.NotNull(values, nameof(values));
            Assert.NotNull(stringParser, nameof(stringParser));
            
            bool ret = false;
            
            Regex cmd = new Regex(@"^(\D+)\s([^=]+)=(.*)$");
            String aux = null;
            
            
            IDictionary<String, Object> editValues = new Dictionary<String, Object>(values);
                


            Action showHelp =()=>
            {
                Console.WriteLine("Key Value List Command Help: ");
                Console.WriteLine(" - list                  List key-values.");
                Console.WriteLine(" - syntax                Value syntax.");
                Console.WriteLine(" - add <key>=<value>     Add key-value. ");
                Console.WriteLine(" - edit <key>=<value>    Edit key-value. ");
                Console.WriteLine(" - remove <key>          Remove key-value.");
                Console.WriteLine(" - save                  Save and Quit.");
                Console.WriteLine(" - help                  Show this help.");
            };
            Action showList = () => Console.WriteLine(values.Select(kv => kv.Key + " = " + stringParser.Parse(kv.Value))
                .ToStringJoin(Environment.NewLine));
            
            Action showSyntax = () => Console.WriteLine(stringParser.Syntax);

            Action save = () =>
            {
                values.Clear();
                values.AddAll(editValues);
            };
            

            bool b = true;
            do
            {
                Console.WriteLine(label);
                
                Console.WriteLine("   Value syntax: " + stringParser.Syntax);

                Console.WriteLine("Finish: [ESC]");

                if (!ConsoleHelper.ReadLineESC(out aux))
                    return false;

                Match m = cmd.Match(aux);

                if (!m.Success)
                {
                    showList();
                    continue;
                }

                switch (m.Groups[1].Value.ToLower()) {
                    case "list":
                        showList();
                        break;
                    case "syntax":
                        showSyntax();
                        break;
                    case "save":
                        save();
                        b = false;
                        ret = true;
                        break;
                    case "help":
                        showHelp();
                        break;
                    case "add":
                        editValues.Add(m.Groups[2].Value.Trim(), stringParser.Unparse(m.Groups[3].Value.Trim()));
                        break;
                    case "edit":
                        editValues[m.Groups[2].Value.Trim()]=stringParser.Unparse(m.Groups[3].Value.Trim());
                        break;
                    case "remove":
                        editValues.Remove(m.Groups[2].Value.Trim());
                        break;
                }
            } while (b);

            return ret;
        }


        
        public static bool InputKeyValue<T>(String label, IDictionary<String, T> values, Func<String, T> parseValue)
        {
            bool ret = false;
            
            Regex cmd = new Regex(@"^(\D+)\s([^=]+)=(.*)$");
            String aux = null;

            Assert.NotNull( values, nameof(values));
            Assert.NotNull( parseValue, nameof(parseValue));
            
            IDictionary<String, T> editValues = new Dictionary<String, T>(values);
                
            
            Action showHelp =()=>
            {
                Console.WriteLine("Key Value List Command Help: ");
                Console.WriteLine(" - list                  List key-values.");
                Console.WriteLine(" - add <key>=<value>     Add key-value. ");
                Console.WriteLine(" - edit <key>=<value>    Edit key-value. ");
                Console.WriteLine(" - remove <key>          Remove key-value.");
                Console.WriteLine(" - save                  Save and Quit.");
                Console.WriteLine(" - help                  Show this help.");
            };
            Action showList = () => Console.WriteLine(values.Select(kv => kv.Key + " = " + kv.Value.ToString())
                .ToStringJoin(Environment.NewLine));

            Action save = () =>
            {
                values.Clear();
                values.AddAll(editValues);
            };
            

            bool b = true;
            do
            {
                Console.WriteLine(label);

                Console.WriteLine("Finish: [ESC]");

                if (!ConsoleHelper.ReadLineESC(out aux))
                    return false;

                Match m = cmd.Match(aux);

                if (!m.Success)
                {
                    showList();
                    continue;
                }

                switch (m.Groups[1].Value.ToLower()) {
                    case "list":
                        showList();
                        break;
                    case "save":
                        save();
                        b = false;
                        ret = true;
                        break;
                    case "help":
                        showHelp();
                        break;
                    case "add":
                        editValues.Add(m.Groups[2].Value.Trim(), parseValue(m.Groups[3].Value.Trim()));
                        break;
                    case "edit":
                        editValues[m.Groups[2].Value.Trim()]=parseValue(m.Groups[3].Value.Trim());
                        break;
                    case "remove":
                        editValues.Remove(m.Groups[2].Value.Trim());
                        break;
                }
            } while (b);

            return ret;
        }

        public static IList<String> ParseParameterList(String parameterSpaceSeparatedString)
        {
            List<String> ret = new List<String>();
            Regex regex = new Regex(@"(""[^""]+"")|([^\s]+)");

			foreach (Match m in regex.Matches(parameterSpaceSeparatedString))
            {
                if (!String.IsNullOrEmpty(m.Groups[1].Value))
                    ret.Add(m.Groups[1].Value);
                else if (!String.IsNullOrEmpty(m.Groups[2].Value))
                    ret.Add(m.Groups[2].Value);
            }

            return ret;
        }


        /// <summary>
        /// WriteLine with color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void WriteLine(string text, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                var oldColor = System.Console.ForegroundColor;
                if (color == oldColor)
                    Console.WriteLine(text);
                else
                {
                    Console.ForegroundColor = color.Value;
                    Console.WriteLine(text);
                    Console.ForegroundColor = oldColor;
                }
            }
            else
                Console.WriteLine(text);
        }

        /// <summary>
        /// Writes out a line with a specific color as a string
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="color">A console color. Must match ConsoleColors collection names (case insensitive)</param>
        public static void WriteLine(string text, string color)
        {
            if (string.IsNullOrEmpty(color))
            {
                WriteLine(text);
                return;
            }

            if (!Enum.TryParse(color, true, out ConsoleColor col))
            {
                WriteLine(text);
            }
            else
            {
                WriteLine(text, col);
            }
        }

        /// <summary>
        /// Write with color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        public static void Write(string text, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                var oldColor = System.Console.ForegroundColor;
                if (color == oldColor)
                    Console.Write(text);
                else
                {
                    Console.ForegroundColor = color.Value;
                    Console.Write(text);
                    Console.ForegroundColor = oldColor;
                }
            }
            else
                Console.Write(text);
        }

        /// <summary>
        /// Writes out a line with color specified as a string
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="color">A console color. Must match ConsoleColors collection names (case insensitive)</param>
        public static void Write(string text, string color)
        {
            if (string.IsNullOrEmpty(color))
            {
                Write(text);
                return;
            }

            if (!Enum.TryParse(color, true, out ConsoleColor col))
            {
                Write(text);
            }
            else
            {
                Write(text, col);
            }
        }

        #region Wrappers and Templates


        /// <summary>
        /// Writes a line of header text wrapped in a in a pair of lines of dashes:
        /// -----------
        /// Header Text
        /// -----------
        /// and allows you to specify a color for the header. The dashes are colored
        /// </summary>
        /// <param name="headerText">Header text to display</param>
        /// <param name="wrapperChar">wrapper character (-)</param>
        /// <param name="headerColor">Color for header text (yellow)</param>
        /// <param name="dashColor">Color for dashes (gray)</param>
        public static void WriteWrappedHeader(string headerText,
                                                char wrapperChar = '-',
                                                ConsoleColor headerColor = ConsoleColor.Yellow,
                                                ConsoleColor dashColor = ConsoleColor.DarkGray)
        {
            if (string.IsNullOrEmpty(headerText))
                return;

            string line = new string(wrapperChar, headerText.Length);

            WriteLine(line, dashColor);
            WriteLine(headerText, headerColor);
            WriteLine(line, dashColor);
        }

        private static Lazy<Regex> colorBlockRegEx = new Lazy<Regex>(
            () => new Regex("\\[(?<color>.*?)\\](?<text>[^[]*)\\[/\\k<color>\\]", RegexOptions.IgnoreCase),
            isThreadSafe: true);

        /// <summary>
        /// Allows a string to be written with embedded color values using:
        /// This is [red]Red[/red] text and this is [cyan]Blue[/blue] text
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="baseTextColor">Base text color</param>
        public static void WriteEmbeddedColorLine(string text, ConsoleColor? baseTextColor = null)
        {
            if (baseTextColor == null)
                baseTextColor = Console.ForegroundColor;

            if (string.IsNullOrEmpty(text))
            {
                WriteLine(string.Empty);
                return;
            }

            int at = text.IndexOf("[", StringComparison.InvariantCulture);
            int at2 = text.IndexOf("]", StringComparison.InvariantCulture);
            if (at == -1 || at2 <= at)
            {
                WriteLine(text, baseTextColor);
                return;
            }

            while (true)
            {
                var match = colorBlockRegEx.Value.Match(text);
                if (match.Length < 1)
                {
                    Write(text, baseTextColor);
                    break;
                }

                // write up to expression
                Write(text.Substring(0, match.Index), baseTextColor);

                // strip out the expression
                string highlightText = match.Groups["text"].Value;
                string colorVal = match.Groups["color"].Value;

                Write(highlightText, colorVal);

                // remainder of string
                text = text.Substring(match.Index + match.Value.Length);
            }

            Console.WriteLine();
        }

        #endregion

        #region Success, Error, Info, Warning Wrappers

        /// <summary>
        /// Write a Success Line - green
        /// </summary>
        /// <param name="text">Text to write out</param>
        public static void WriteSuccess(string text)
        {
            WriteLine(text, ConsoleColor.Green);
        }

        /// <summary>
        /// Write a Error Line - Red
        /// </summary>
        /// <param name="text">Text to write out</param>
        public static void WriteError(string text)
        {
            WriteLine(text, ConsoleColor.Red);
        }

        /// <summary>
        /// Write a Warning Line - Yellow
        /// </summary>
        /// <param name="text">Text to Write out</param>
        public static void WriteWarning(string text)
        {
            WriteLine(text, ConsoleColor.DarkYellow);
        }


        /// <summary>
        /// Write a Info Line - dark cyan
        /// </summary>
        /// <param name="text">Text to write out</param>
        public static void WriteInfo(string text)
        {
            WriteLine(text, ConsoleColor.DarkCyan);
        }
        #endregion
    }
}
