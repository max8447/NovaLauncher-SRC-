using NovaLauncher.Models.API.NovaBackend;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace NovaLauncher.Models.Tools
{
    public class Injector
    {
        public const int PROCESS_CREATE_THREAD = 2;

        public const int PROCESS_VM_OPERATION = 8;
        public const int PROCESS_VM_WRITE = 0x0020;
        public const int PROCESS_VM_READ = 0x0010;

        public const int PROCESS_QUERY_INFORMATION = 0x0400;

        public const uint PAGE_READWRITE = 4;

        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, int dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern int SuspendThread(IntPtr hThread);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AllocConsole();

        public delegate bool HandlerRoutine(int dwCtrlType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine HandlerRoutine, bool Add);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes,
            uint dwStackSize, IntPtr lpStartAddress,
            IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        public static void InjectAsync(int processId, string path)
        {
            IntPtr handle;
            IntPtr loadLibrary;
            IntPtr address;

            handle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION |
                PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, processId);

            loadLibrary = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");

            var size = (uint)(path.Length * 2 + 1);
            address = VirtualAllocEx(handle, IntPtr.Zero,
                size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            WriteProcessMemory(handle, address,
                Encoding.Unicode.GetBytes(path), size, out _);

            Thread thread = new Thread(() => CreateRemoteThread(handle, IntPtr.Zero, 0, loadLibrary, address, 0, IntPtr.Zero));
            thread.Start();
            thread.Join();
        }

        public static async Task<bool> IsCumarLoaded(Process process)
        {
            bool isCumarDllPresent = false;

            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.Equals("Cumar.dll", StringComparison.InvariantCultureIgnoreCase))
                {
                    isCumarDllPresent = true;
                    break;
                }
            }

            if (isCumarDllPresent)
                return true;
            else
            {
                return false;
            }
        }

        public static string GetCumar()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string novaPath = Path.Combine(localAppData, "nova");
            string cumarPath = Path.Combine(novaPath, "Cumar.dll");
            if (File.Exists(cumarPath))
            {
                try
                {
                    File.Delete(cumarPath);
                }
                catch
                {
                    return null;
                }
            }

            try
            {
                byte[] byteArray = Properties.Resources.Cumar;
                if (Global.bDevMode)
                {
                    byteArray = Properties.Resources.Cumar_;
                }

                using (Stream stream = new MemoryStream(byteArray))
                {
                    using (FileStream fileStream = new FileStream(cumarPath, FileMode.Create))
                    {
                        stream.CopyTo(fileStream);
                        return cumarPath;

                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static string GetClient()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string novaPath = Path.Combine(localAppData, "Nova");

            string path = System.IO.Path.Combine(novaPath, "console.dll");

            if (!File.Exists(path))
            {
                return string.Empty;
            }


            return path;
        }

        public static string DownloadFile(string url, string path)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, path);
            }

            return path;
        }
        public static bool ByteArrayContains(byte[] source, params byte[][] patterns)
        {
            foreach (byte[] pattern in patterns)
            {
                for (int i = 0; i <= source.Length - pattern.Length; i++)
                {
                    if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static async Task<bool> VerifyClient(ACSeasonData aCSeasonData)
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string novaPath = Path.Combine(localAppData, "Nova");

                string path = System.IO.Path.Combine(novaPath, "console.dll");

                var FileBytes = await File.ReadAllBytesAsync(path);
                if (!ByteArrayContains(FileBytes, new byte[] { 0x6A, 0x87, 0x9C, 0x2E, 0x33, 0xC7, 0x14, 0x8A, 0x1D, 0xEF, 0x77, 0x9B, 0x46, 0xE9, 0x28, 0x55, 0x7F, 0x62, 0xF5, 0xAA, 0x0D, 0x98, 0x21, 0x36, 0xBF, 0x4C, 0xD0, 0xA3, 0x8F, 0xE4, 0x70 }))
                {
                    return false;
                }

                if (File.Exists(path) && CalculateFileHash(path) == aCSeasonData.fileHash)
                    return true;
                else
                    return false;
            }
            catch { return false; }
        }
        public static string CalculateFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(fileStream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
        
    }
}
