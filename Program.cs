// *--------------------------------------------------------------------------------------------------
// * @file    Program.cs
// * @brief   Initialize Cortex-M processor
// * @author  Cristian Croci <https://solidsnake72.github.io/>
// *--------------------------------------------------------------------------------------------------
// * Copyright (C) 2022 Cristian Croci
// *
// * This program is free software: you can redistribute it and/or modify
// * it under the terms of the GNU General Public License as published by
// * the Free Software Foundation, either version 3 of the License, or
// * (at your option) any later version.
// *
// * This program is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// * GNU General Public License for more details.
// *
// * You should have received a copy of the GNU General Public License
// * along with this program.  If not, see <https://www.gnu.org/licenses/>.
// *--------------------------------------------------------------------------------------------------
//
using System.Diagnostics;
using System.Reflection;

namespace BuildNumber;

internal static class Program {
  private enum OutputType : byte {
    CFileType = 0,
    CPPFileType,
    CSFileType,
    InvalidFileType = byte.MaxValue,
  }

  private const uint STARTBuildNumber = 1;
  private const uint MAXBuildNumber = 99999;
  private const string FILEBuildName = "build_no.";
    
#region Private Variables
  private static string _outputPath = string.Empty;
  private static string _outputType = string.Empty;
  private static uint _buildNumber = uint.MaxValue;
  private static string _fileOutput = string.Empty;
  private static OutputType _selectedOutputType = OutputType.InvalidFileType;
#endregion
  
  
#region Private Functions
  //--------------------------------------------------------------------------------------------------------------------
  private static void Usage() {
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(@"Usage: buildnumber -t [BUILD FILE TYPE] -p [PATH] (-s START BUILD NUMBER)");
    Console.WriteLine(@"       -t Build file type: C, C++ or C#");
    Console.WriteLine(@"       -p Output path");
    Console.WriteLine(@"       -s Start build number eg. 100 (Optional)");
    Console.WriteLine(@"       -h This help");
    Console.WriteLine();
    Console.WriteLine(@"  How to use");
    Console.WriteLine(@"       buildnumber -t C++ -p C:\myproject");
    Console.WriteLine(@"       ./buildnumber -t C++ -p /home/username/myproject");
    Console.WriteLine();
  }    
  //--------------------------------------------------------------------------------------------------------------------
  private static void SetTerminalSize(ushort width, ushort height) {
    if (OperatingSystem.IsLinux()) {
      // Set new size by telnet escape sequence 
      Console.WriteLine($"\x1b[8;{height};{width}t");
    } else if (OperatingSystem.IsWindows()) {
      Console.SetWindowSize(width, height);
    } else { }
  }
  //--------------------------------------------------------------------------------------------------------------------
private static bool ProcessArguments(string[]? args) {
    // Function variables
    int index = 0;

    // Check num of arguments
    if (args == null || args.Length == 0) {
      Usage();
      return false;
    }

    // Loop all arguments
    do {
      // Check if start with '-'
      if (args[index].ToCharArray()[0] == '-') {
        // Process parameter
        switch (args[index].ToCharArray()[1]) {
          //==================================================
          case 'p': {    // Output Path
            _outputPath = args[++index].Trim();
            break;
          }
          //==================================================
          case 't': {    // Output type
            _outputType = args[++index].Trim();
            break;
          }
          //==================================================
          case 's': {    // Start build number
            if (!uint.TryParse(args[++index].Trim(), out _buildNumber)) {
              _buildNumber = uint.MaxValue;
            }
            break;
          }
          //==================================================
          case 'h': {    // Help
            Usage();
            return false;              
          }
          //==================================================
          default: {      // Invalid option
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unknown option '{0}'", args[index].ToCharArray()[1]);
            Console.ForegroundColor = ConsoleColor.White;
            Usage();
            return false;
          }
          //==================================================
        }
      } else {
        // Check if present other parameter error
        if (index < args.Length - 1) {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("Invalid arguments!!!!!");
          Console.ForegroundColor = ConsoleColor.White;
          Usage();

          return false;
        }
      }

      // Increment Index
      index++;
    } while(index < args.Length);

    return true;
  }
  //--------------------------------------------------------------------------------------------------------------------
  private static bool Init() {
    // Check output path
    if (!Directory.Exists(_outputPath)) {
      return false;
    }
    
    // Check build number
    if (_buildNumber == uint.MaxValue) {
      try {
        string datFile = Path.Combine(_outputPath, "build_no.dat");
        if (File.Exists(datFile)) {
          string readValue = File.ReadAllText(datFile).Trim();

          // Try to parse string
          if (!uint.TryParse(readValue, out _buildNumber)) {
            _buildNumber = STARTBuildNumber;
          } else {
            // Increment build number
            ++_buildNumber;
          }
          
          // Verify build number range
          if (_buildNumber > MAXBuildNumber || _buildNumber == 0) {
            _buildNumber = STARTBuildNumber;
          }

          // Save file DAT
          File.WriteAllText(datFile, $"{_buildNumber}");
        }
      } catch (Exception ex) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error to process 'build.dat' file in {_outputPath}");
        Console.ForegroundColor = ConsoleColor.White;

        _ = ex.ToString();
        return false;
      }

      switch (_outputType) {
        //================================================
        case "C": {
          _selectedOutputType = OutputType.CFileType;
          _fileOutput = string.Concat(FILEBuildName, "h");
          break;
        }
        //================================================
        case "C++": {
          _selectedOutputType = OutputType.CPPFileType;
          _fileOutput = string.Concat(FILEBuildName, "hpp");
          break;
        }
        //================================================
        case "C#": {
          _selectedOutputType = OutputType.CSFileType;
          _fileOutput = string.Concat(FILEBuildName, "cs");
          break;
        }
        //================================================
        default: {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Unsupported output type!!!!");
          Console.ForegroundColor = ConsoleColor.White;
          return false;
        }
      }

    }

    return true;
  }
  //--------------------------------------------------------------------------------------------------------------------
#endregion
  
#region PRIVATE - Application Events
  private static void CurrentDomain_ProcessExit(object? sender, EventArgs e) {
    Console.WriteLine("Terminated");
  }
#endregion
  
  
  private static void Main(string[]? args) {
    // Exit function event
    Console.CancelKeyPress += CurrentDomain_ProcessExit;
    Process currentProcess = Process.GetCurrentProcess();
    currentProcess.Exited += CurrentDomain_ProcessExit;
    AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
    AppDomain.CurrentDomain.DomainUnload += CurrentDomain_ProcessExit;

    // Set terminal size
    SetTerminalSize(120, 40);
    
    Console.WriteLine($"Build Number Utility Tools - Version: {Assembly.GetExecutingAssembly().GetName().Version?.ToString()}");
    Console.WriteLine($"Running on : " + (OperatingSystem.IsLinux() ? "Linux" : "Windows"));
    Console.WriteLine($"Copyright @ 2023 by Cristian Croci (https://solidsnake72.github.io/)");
    Console.WriteLine($"License: GNU General Public License V3");
    
    // Process Argument
    if (!ProcessArguments(args)) {
      Environment.Exit(-1);
    }
    
    // Init System
    if (!Init()) {
      Environment.Exit(-2);
    }

    StreamWriter? fileOut;
    string outFile = Path.Combine(_outputPath, _fileOutput);
    
    // Delete old file
    File.Delete(outFile);

    // Create File
    fileOut = File.CreateText(outFile);
    
    // If file C of C++
    if (_selectedOutputType == OutputType.CFileType || _selectedOutputType == OutputType.CPPFileType) {
      // Write File
      fileOut.WriteLine("/**");
      fileOut.WriteLine($" * @file    build_no.{(_selectedOutputType == OutputType.CPPFileType ? "hpp" : "h")}" );
      fileOut.WriteLine(" *");
      fileOut.WriteLine($" * @brief   Build number header file for {( _selectedOutputType == OutputType.CPPFileType ? "C++" : "C")}");
      fileOut.WriteLine(" */");
      fileOut.WriteLine($"#ifndef _BUILD_NUMBER_{(_selectedOutputType == OutputType.CPPFileType ? "HPP_" : "H_")}");
      fileOut.WriteLine($"#define _BUILD_NUMBER_{(_selectedOutputType == OutputType.CPPFileType ? "HPP_" : "H_")}");
      fileOut.WriteLine();
      fileOut.WriteLine("// *****************************************************************************");
      fileOut.WriteLine("// -----------------------------------------------------------------------------");
      fileOut.WriteLine("// Building Number *************************************************************");
      fileOut.WriteLine();
      fileOut.WriteLine("/**");
      fileOut.WriteLine(" * @brief Build number in number");
      fileOut.WriteLine(" */");
      if (_selectedOutputType == OutputType.CPPFileType) {
        fileOut.WriteLine($"constexpr uint32_t BUILDNUMBER = {_buildNumber}");
      } else {
        fileOut.WriteLine($"#define BUILDNUMBER         {_buildNumber}");
      }
      fileOut.WriteLine();
      fileOut.WriteLine("/**");
      fileOut.WriteLine(" * @brief Build number in string format");
      fileOut.WriteLine(" */");
      if (_selectedOutputType == OutputType.CPPFileType) {
        fileOut.WriteLine($"constexpr char[] BUILDNUMBER = \"{_buildNumber}\0\"");
      } else {
        fileOut.WriteLine($"#define BUILDNUMBER_STR     \"{_buildNumber}\"");
      }
      fileOut.WriteLine();
      fileOut.WriteLine($"#endif // _BUILD_NUMBER_{(_selectedOutputType == OutputType.CPPFileType ? "HPP_" : "H_")}");
      fileOut.WriteLine();
    } else if (_selectedOutputType == OutputType.CSFileType) {
      // Write File
      fileOut.WriteLine("/**");
      fileOut.WriteLine(" * @file    build_no.cs");
      fileOut.WriteLine(" *");
      fileOut.WriteLine(" * @brief   Building number class for C#");
      fileOut.WriteLine(" */");
      fileOut.WriteLine("namespace BuildNumber {");
      fileOut.WriteLine("  public static class BuildNo {");
      fileOut.WriteLine();
      fileOut.WriteLine("    // *****************************************************************************");
      fileOut.WriteLine("    // -----------------------------------------------------------------------------");
      fileOut.WriteLine("    // Building Number *************************************************************");
      fileOut.WriteLine();
      fileOut.WriteLine("    // Build number in number");
      fileOut.WriteLine($"    public const uint BUILDNUMBER               = {_buildNumber};");
      fileOut.WriteLine();
      fileOut.WriteLine("    // Build number in string format");
      fileOut.WriteLine($"    public const string BUILDNUMBER_STR         = \"{_buildNumber}\";");
      fileOut.WriteLine();
      fileOut.WriteLine("  }");
      fileOut.WriteLine("}");
      fileOut.WriteLine();
    }
    
    fileOut.Flush();
    fileOut.Close();
    
    // Success
    Console.ForegroundColor = ConsoleColor.Green;        
    Console.WriteLine("File '{0}' success generated with new Build Number {1}.", _fileOutput, _buildNumber);
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine();
  }
}
