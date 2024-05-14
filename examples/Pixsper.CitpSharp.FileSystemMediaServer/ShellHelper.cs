using System.Diagnostics;

namespace Pixsper.CitpSharp.FileSystemMediaServer;

public static class ShellHelper
{
	public static string RunFfmpeg(this string cmd)
	{
			var escapedArgs = cmd.Replace("\"", "\\\"");
			
			var process = new Process()
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "ffmpeg",
					Arguments = cmd,
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				}
			};
			process.Start();
			string result = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			return result;
		}
}