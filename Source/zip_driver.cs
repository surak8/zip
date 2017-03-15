using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

// -o blah source

// DIR=C:\Users\RTCOUSENS\colt\projects\Laser\sql
// zip -o CT_2016 colt_tracking.2016.bcp ColtTrackingForMobius.sql

namespace NSZip {
    /// <summary>blah</summary>
    class Program {
        /// <summary>main-line porgram.</summary>
        /// <param name="args">an <see cref="Array"/> of <see cref="string"/>s.</param>
        /// <seealso cref="showUserHelp"/>
        /// <seealso cref="parseArguments"/>
        /// <seealso cref="populateZipfile"/>
        static void Main(string[] args) {
            List<string> argsToProcess;
            int nargs, exitcode = 0;
            string zipName = null;
            bool showHelp = false;

            if ((nargs = args.Length) > 0) {
                argsToProcess = parseArguments(args, out zipName, out showHelp);
                if (showHelp) {
                    Console.Out.WriteLine("user requested help.");
                    showUserHelp(Console.Out, Assembly.GetEntryAssembly());
                } else {
                    if (string.IsNullOrEmpty(zipName)) {
                        Console.Error.WriteLine("zip-name not specifified.");
                        showUserHelp(Console.Error, Assembly.GetEntryAssembly());
                        exitcode = 2;
                    } else if (argsToProcess.Count < 1) {
                        Console.Error.WriteLine("no files/directories to zip.");
                        showUserHelp(Console.Error, Assembly.GetEntryAssembly());
                        exitcode = 2;
                    } else {
                        populateZipfile(zipName, argsToProcess);
                    }
                }
            } else {
                Console.Out.WriteLine("no arguments entered.");
                showUserHelp(Console.Out, Assembly.GetEntryAssembly());
            }
            Environment.Exit(exitcode);
        }

        /// <summary>parse the command-line parameters.</summary>
        /// <param name="args"></param>
        /// <param name="zipName"></param>
        /// <param name="showHelp"></param>
        /// <returns></returns>
        static List<string> parseArguments(string[] args, out string zipName, out bool showHelp) {
            List<string> argsToProcess;
            int nargs = args.Length, len;
            string anArg;

            zipName = null;
            showHelp = false;
            argsToProcess = new List<string>();
            for (int i = 0 ; i < nargs ; i++) {
                anArg = args[i];
                if ((len = anArg.Length) >= 2) {
                    if (anArg[0] == '-' || anArg[0] == '/') {
                        switch (anArg[1]) {
                            case 'o':
                                if (len > 2)
                                    zipName = anArg.Substring(2).Trim();
                                else { zipName = args[i + 1]; i++; }
                                if (string.Compare(Path.GetExtension(zipName), ".zip", true) != 0)
                                    zipName = zipName + ".zip";
                                break;
                            case 'h': showHelp = true; break;
                            case '?': showHelp = true; break;
                        }
                    } else {
                        argsToProcess.Add(anArg);
                    }
                }
            }
            return argsToProcess;
        }

        /// <summary>Add files to the zip-file.</summary>
        /// <param name="zipName">a <see cref="string"/> containing the name of the zip-file to create/process.</param>
        /// <param name="argsToProcess">a  <see cref="List{String}"/></param>
        /// <seealso cref="replaceEntryIfExists"/>
        /// <seealso cref="addDirectoryContentsTo"/>
        static void populateZipfile(string zipName, List<string> argsToProcess) {
            FileStream fs;
            ZipArchiveMode zam = ZipArchiveMode.Create;
            ZipArchive za;
            string[] blah;
            string str;
            int pos;

            if (File.Exists(zipName))
                zam = ZipArchiveMode.Update;
            fs = new FileStream(zipName, FileMode.OpenOrCreate);
            za = new ZipArchive(fs, zam);
            foreach (string anArgs in argsToProcess) {
                if (File.Exists(anArgs)) {
                    replaceEntryIfExists(za, anArgs);
                } else if (Directory.Exists(anArgs)) {
                    addDirectoryContentsTo(za, anArgs);
                } else {
                    if ((pos = anArgs.LastIndexOf('*')) > 0) {
                        str = anArgs.Substring(0, pos - 1);
                        blah = Directory.GetFiles(str, anArgs.Substring(pos));
                        if (blah.Length > 0)
                            foreach (string aFile in blah)
                                replaceEntryIfExists(za, aFile);
                    } else {
                        str = Directory.GetCurrentDirectory();
                        blah = Directory.GetFiles(str, anArgs);
                        if (blah.Length > 0)
                            foreach (string aFile in blah)
                                replaceEntryIfExists(za, aFile.Substring(str.Length + 1));
                    }
                }
            }
//            za.fl
            za.Dispose();
            za = null;
        }

        /// <summary>find all files in the given directory.</summary>
        /// <param name="za">a <see cref="ZipArchive"/> which will contain the results.</param>
        /// <param name="dirName">a <see cref="string"/> containing the directory to be scanned.</param>
        /// <remarks><para>Find child entries of this directory, and add them to the zip-file.</para></remarks>
        /// <seealso cref="findChildrenOf"/> 
        /// <seealso cref="replaceEntryIfExists"/> 
        static void addDirectoryContentsTo(ZipArchive za, string dirName) {
            List<string> files = new List<string>();
            string[] kids;

            kids = findChildrenOf(dirName);
            if (kids.Length > 0)
                files.AddRange(kids);
            foreach (string aKid in kids)
                replaceEntryIfExists(za, aKid);
        }

        /// <summary>Replace the given element, if it exists.</summary>
        /// <param name="za">a <see cref="ZipArchive"/> instance whiich will contain the new child-element.</param>
        /// <param name="aKid">a <see cref="string"/> containing the name of the file to add to the archive.</param>
        /// <seealso cref="ZipArchiveEntry"/>
        /// <seealso cref="ZipArchiveEntry.Delete"/>
        /// <seealso cref="ZipArchive.GetEntry"/>
        /// <remarks>
        /// <para>Add a <see cref="ZipArchiveEntry"/> after conditionally removing the previous instance.</para>
        /// </remarks>
        static void replaceEntryIfExists(ZipArchive za, string aKid) {
            ZipArchiveEntry z, znew;
            //            zipar
            bool bupdated = false;

            if (za.Mode != ZipArchiveMode.Create)
                if ((z = za.GetEntry(aKid)) != null) {
                    z.Delete();
                    Console.WriteLine("updated: " + aKid);
                    bupdated = true;
                }
            znew = za.CreateEntryFromFile(aKid, aKid, CompressionLevel.Optimal);
            //            if (!wroteMessage)
            Console.WriteLine((bupdated ? "updated" : "added") + ":" + aKid);
        }

        /// <summary>Gather files in the directory.</summary>
        /// <param name="dirName"></param>
        /// <returns>an <see cref="Array"/> of <see cref="string"/>s containing all child-objects of a given directory.</returns>
        /// <seealso cref="Directory.GetDirectories(string)"/>
        /// <seealso cref="Directory.GetFiles(string)"/>
        static string[] findChildrenOf(string dirName) {
            List<string> ret = new List<string>();
            string[] tmp, kids;

            tmp = Directory.GetDirectories(dirName);
            foreach (string aDir in tmp) {
                kids = findChildrenOf(aDir);
                if (kids.Length > 0)
                    ret.AddRange(kids);
            }
            if ((tmp = Directory.GetFiles(dirName)).Length > 0)
                ret.AddRange(tmp);
            return ret.ToArray();
        }

        /// <summary>Show help messages.</summary>
        /// <param name="tw">a <see cref="TextWriter"/> to which to write the information.</param>
        /// <param name="a">a <see cref="Assembly"/> containing the information about ....blah...</param>
        /// <seealso cref="Path.GetFileNameWithoutExtension(string)"/> 
        static void showUserHelp(TextWriter tw, Assembly a) {
            tw.WriteLine("usage:");
            // -D devexpress
            // -g generate-code
            // -isDebug phibro-style
            // -s simple

            tw.WriteLine("\t" + Path.GetFileNameWithoutExtension(a.Location) +
                ": -o out_zipfile file_or_directory [...file_or_directory]");
        }
    }
}