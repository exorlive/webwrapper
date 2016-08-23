using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ExorLiveNamedPipeSampleClient
{
	class ProcessTool
	{
		/// <summary>
		/// Find a process by name amoung the processes running by the current user.
		/// </summary>
		/// <param name="name"></param>
		public static int FindProcessIdByName(string name)
		{
			string currentUser = GetProcessUser(Process.GetCurrentProcess());
			foreach(Process process in Process.GetProcessesByName(name))
			{
				if(currentUser == GetProcessUser(process))
				{
					return process.Id;
				}
			}
			return 0;
		}

		internal static void DumpProcesses()
		{
			foreach(Process process in Process.GetProcesses())
			{
				Console.WriteLine(" {0}: {1}", process.Id, process.ProcessName);
			}
		}

		private static string GetProcessUser(Process process)
		{
			IntPtr processHandle = IntPtr.Zero;
			try
			{
				OpenProcessToken(process.Handle, 8, out processHandle);
				WindowsIdentity wi = new WindowsIdentity(processHandle);
				string user = wi.Name;
				return user.Contains(@"\") ? user.Substring(user.IndexOf(@"\") + 1) : user;
			}
			catch
			{
				return null;
			}
			finally
			{
				if (processHandle != IntPtr.Zero)
				{
					CloseHandle(processHandle);
				}
			}
		}

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(IntPtr hObject);
	}
}
